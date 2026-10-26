
using UnityEngine;

[CreateAssetMenu(fileName = "Symbol_New", menuName = "Slots/Symbol Data")]
public class SymbolData : ScriptableObject
{
    [SerializeField] private string symbolId = "symbol";
    [SerializeField] private Sprite sprite;

    [Tooltip("Relative chance of this Symbol being picked. Higher = more common.")]
    [SerializeField, Min(0)] private int weight = 10;

    [Tooltip("Payout = bet * payoutMultiplier when reels show this symbol.")]
    [SerializeField, Min(0)] private int payoutMultiplier = 0;

    [Tooltip("Wild symbols substitute for any other symbols.")]
    [SerializeField] private bool isWild = false;

    [Header("Visual Settings")]
    [Tooltip("Relative display size of this symbol.")]
    [SerializeField, Range(0.5f, 1.5f)] private float displayScale = 1f;

    public string SymbolId => symbolId;
    public Sprite Sprite => sprite;
    public int Weight => weight;
    public int PayoutMultiplier => payoutMultiplier;
    public bool IsWild => isWild;
    public float DisplayScale => displayScale;
}
