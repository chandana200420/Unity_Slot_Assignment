
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private SlotMachine slotMachine;

    [Header("Buttons")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button betUpButton;
    [SerializeField] private Button betDownButton;
    [SerializeField] private Button restartButton;

    [SerializeField] private Toggle autoplayToggle;

    [Header("Restart Popup")]
    [SerializeField] private GameObject restartPopup;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text betText;
    [SerializeField] private TMP_Text winText;
    [SerializeField] private float countUpDuration = 0.6f;

    private Coroutine winRoutine;

    private void Start()
    {
        spinButton.onClick.AddListener(slotMachine.Spin);

        betUpButton.onClick.AddListener(
            () => slotMachine.Wallet.ChangeBet(+1));

        betDownButton.onClick.AddListener(
            () => slotMachine.Wallet.ChangeBet(-1));

        restartButton.onClick.AddListener(ShowRestartPopup);

        yesButton.onClick.AddListener(ConfirmRestart);
        noButton.onClick.AddListener(CloseRestartPopup);

        autoplayToggle.onValueChanged.AddListener(
            slotMachine.SetAutoPlay);

        slotMachine.Wallet.Changed += Refresh;
        slotMachine.BusyChanged += OnBusyChanged;
        slotMachine.SpinStarted += OnSpinStarted;
        slotMachine.SpinFinished += OnSpinFinished;

        restartPopup.SetActive(false);

        winText.text = string.Empty;

        Refresh();
    }

    private void OnDestroy()
    {
        if (slotMachine == null) return;

        slotMachine.Wallet.Changed -= Refresh;
        slotMachine.BusyChanged -= OnBusyChanged;
        slotMachine.SpinStarted -= OnSpinStarted;
        slotMachine.SpinFinished -= OnSpinFinished;
    }

    private void Refresh()
    {
        WalletModel wallet = slotMachine.Wallet;
        bool busy = slotMachine.IsBusy;

        balanceText.text = $"Balance: {wallet.Balance}";
        betText.text = $"Bet: {wallet.Bet}";

        spinButton.interactable = !busy && wallet.CanAffordBet;
        betUpButton.interactable = !busy;
        betDownButton.interactable = !busy;

        // Keep the existing behavior: show Restart when
        // the player cannot afford the current bet.
        restartButton.gameObject.SetActive(
            !busy && !wallet.CanAffordBet);
    }

    private void OnBusyChanged(bool busy)
    {
        Refresh();
    }

    private void OnSpinStarted()
    {
        if (winRoutine != null)
        {
            StopCoroutine(winRoutine);
            winRoutine = null;
        }

        winText.text = string.Empty;
    }

    private void OnSpinFinished(SpinResult result, int payout)
    {
        if (payout <= 0) return;

        if (winRoutine != null)
        {
            StopCoroutine(winRoutine);
        }

        winRoutine = StartCoroutine(CountUpWin(payout));
    }

    private IEnumerator CountUpWin(int target)
    {
        for (float t = 0f; t < countUpDuration; t += Time.deltaTime)
        {
            int shown = Mathf.RoundToInt(
                Mathf.Lerp(0f, target, t / countUpDuration));

            winText.text = $"WIN {shown}";

            yield return null;
        }

        winText.text = $"WIN {target}";
        winRoutine = null;
    }

    private void ShowRestartPopup()
    {
        if (slotMachine.IsBusy) return;

        restartPopup.SetActive(true);
    }

    private void ConfirmRestart()
    {
        if (slotMachine.IsBusy) return;

        CloseRestartPopup();

        slotMachine.ResetBalance();

        winText.text = string.Empty;
    }

    private void CloseRestartPopup()
    {
        restartPopup.SetActive(false);
    }
}
