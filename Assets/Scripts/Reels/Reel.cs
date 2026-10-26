using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One vertical reel. The strip holds (visibleRows + 2) slots:
/// one spare above and one below.
/// </summary>
public class Reel : MonoBehaviour
{
    private enum ReelState { Idle, Spinning, Stopping, Landing }

    [Header("Layout")]
    [SerializeField] private RectTransform strip;
    [SerializeField] private ReelSlot slotPrefab;
    [SerializeField, Min(1)] private int visibleRows = 3;
    [SerializeField] private float slotWidth = 150f;
    [SerializeField] private float slotHeight = 150f;


    [Header("Symbol Appearance")]
    [SerializeField, Range(0.4f, 1f)]
    private float symbolScale = 0.75f;

    [Header("Motion")]
    [Tooltip("Scroll speed in UI pixels per second.")]
    [SerializeField] private float spinSpeed = 2200f;
    [SerializeField] private float landDuration = 0.45f;

    [Tooltip("How far the reel passes its final position before settling back.")]
    [SerializeField] private float overshoot = 18f;
    [SerializeField] private float settleDuration = 0.12f;

    private readonly List<ReelSlot> slots = new List<ReelSlot>();
    private readonly Queue<SymbolData> pendingSymbols = new Queue<SymbolData>();

    private RngService rng;
    private ReelState state = ReelState.Idle;
    private float offset;

    private int SlotCount => visibleRows + 2;
    private int MiddleRow => visibleRows / 2;

    public bool IsSpinning => state != ReelState.Idle;

    public SymbolData CenterSymbol => slots[MiddleRow + 1].Symbol;

    public event Action<Reel> Stopped;

    public void Initialize(RngService rngService)
    {
        rng = rngService;

        for (int i = 0; i < SlotCount; i++)
        {
            ReelSlot slot = Instantiate(slotPrefab, strip);
            slot.Rect.sizeDelta = new Vector2(slotWidth, slotHeight);
            slot.Rect.localScale = Vector3.one * symbolScale;
            slot.SetSymbol(rng.Pick());
            slots.Add(slot);
        }

        ApplyPositions();
    }

    public void StartSpin()
    {
        if (state != ReelState.Idle)
            return;

        state = ReelState.Spinning;
    }

    /// <summary>
    /// Stops with centerSymbol on the pay row.
    /// The other visible rows receive random symbols.
    /// </summary>
    public void StopOn(SymbolData centerSymbol)
    {
        if (state != ReelState.Spinning)
            return;

        pendingSymbols.Clear();

        for (int row = visibleRows - 1; row >= 0; row--)
        {
            pendingSymbols.Enqueue(
                row == MiddleRow ? centerSymbol : rng.Pick()
            );
        }

        state = ReelState.Stopping;
    }

    private void Update()
    {
        if (state != ReelState.Spinning &&
            state != ReelState.Stopping)
            return;

        AdvanceOffset(spinSpeed * Mathf.Min(Time.deltaTime, 0.05f));

        if (state == ReelState.Stopping && pendingSymbols.Count == 0)
        {
            state = ReelState.Landing;
            StartCoroutine(LandRoutine());
        }
    }

    /// <summary>
    /// Scrolls the strip and recycles slots as they pass a full row.
    /// </summary>
    private void AdvanceOffset(float delta)
    {
        offset += delta;

        while (offset >= slotHeight)
        {
            offset -= slotHeight;
            RecycleBottomSlot();
        }

        ApplyPositions();
    }

    /// <summary>
    /// Moves the bottom slot to the top and assigns its next symbol.
    /// </summary>
    private void RecycleBottomSlot()
    {
        ReelSlot bottom = slots[slots.Count - 1];

        slots.RemoveAt(slots.Count - 1);
        slots.Insert(0, bottom);

        bottom.SetSymbol(
            pendingSymbols.Count > 0
                ? pendingSymbols.Dequeue()
                : rng.Pick()
        );
    }

    private void ApplyPositions()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Rect.anchoredPosition =
                new Vector2(0f, RestY(i) - offset);
        }
    }

    /// <summary>
    /// Calculates the resting Y position of a slot.
    /// </summary>
    private float RestY(int index)
    {
        return ((SlotCount - 1) * 0.5f - index) * slotHeight;
    }

    /// <summary>
    /// Eases the reel onto the grid with a small overshoot.
    /// </summary>
    private IEnumerator LandRoutine()
    {
        float distance = slotHeight + overshoot - offset;
        float travelled = 0f;

        for (float t = 0f; t < landDuration;
             t += Mathf.Min(Time.deltaTime, 0.05f))
        {
            float eased = EaseOutCubic(t / landDuration) * distance;

            AdvanceOffset(eased - travelled);
            travelled = eased;

            yield return null;
        }

        AdvanceOffset(distance - travelled);

        float start = offset;

        for (float t = 0f; t < settleDuration;
             t += Mathf.Min(Time.deltaTime, 0.05f))
        {
            offset = Mathf.Lerp(
                start,
                0f,
                Mathf.SmoothStep(0f, 1f, t / settleDuration)
            );

            ApplyPositions();

            yield return null;
        }

        offset = 0f;
        ApplyPositions();

        state = ReelState.Idle;
        Stopped?.Invoke(this);
    }

    private static float EaseOutCubic(float t)
    {
        float inv = 1f - Mathf.Clamp01(t);
        return 1f - inv * inv * inv;
    }
}