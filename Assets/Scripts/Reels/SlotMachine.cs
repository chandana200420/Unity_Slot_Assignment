using UnityEngine;
using System.Collections;
using System;
// <summary>
/// Coordinates a full spin: place bet -> pick outcome with RNG -> animate reels -> evaluate -> pay out.
/// The outcome is decided BEFORE the animation starts, so the animation can never influence fairness.
/// </summary>
public class SlotMachine : MonoBehaviour
{

    [Header("Setup")]
    [SerializeField] private PayoutTable payoutTable;
    [SerializeField] private Reel[] reels;

    [Header("Economy")]
    [SerializeField] private int startingBalance = 1000;
    [SerializeField] private int[] betOptions = { 10, 20, 50, 100 };


    [Header("Timing")]
    [SerializeField] private float minSpinTime = 1f;
    [SerializeField] private float delayBetweenReelStops = 0.4f;
    [SerializeField] private float autoplayDelay = 1f;

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spinClip;
    [SerializeField] private AudioClip winClip;

    private RngService rng;
    private bool autoplayEnabled;

    public WalletModel Wallet { get; private set; }
    public bool IsBusy { get; private set; }

    public event Action SpinStarted;
    public event Action<bool> BusyChanged;

    /// <summary>Raised after each spin with the result and the credits won (0 on a loss).</summary>
    public event Action<SpinResult, int> SpinFinished;

    private void Awake()
    {
        rng = new RngService(payoutTable);
        Wallet = new WalletModel(startingBalance, betOptions);

        foreach (Reel reel in reels)
        {
            reel.Initialize(rng);
        }
    }
    // ---- Called by UI buttons ----
    public void Spin()
    {
        if (IsBusy || !Wallet.CanAffordBet) return;
        StartCoroutine(SpinLoop());
    }
    public void SetAutoPlay(bool enabled)
    {
        autoplayEnabled = enabled;

        if (autoplayEnabled && !IsBusy && Wallet.CanAffordBet)
        {
            StartCoroutine(SpinLoop());
        }
    }
    public void ResetBalance()
    {
        if (!IsBusy) Wallet.Reset(startingBalance);
    }
    // ---- Spin flow ----

    /// <summary>Runs one spin, or keeps spinning while autoplay is on and the player can afford it.</summary>
    private IEnumerator SpinLoop()
    {
        SetBusy(true);

        do
        {
            if (!Wallet.TryPlaceBet())
            {
                autoplayEnabled = false;
                break;
            }

            yield return PlayOneSpin();

            if (autoplayEnabled)
            {
                yield return new WaitForSeconds(autoplayDelay);
            }
        }
        while (autoplayEnabled && Wallet.CanAffordBet);

        SetBusy(false);
    }
    private IEnumerator PlayOneSpin()
    {
        int bet = Wallet.Bet;
        SpinStarted?.Invoke();

        //1.decide the outcome first(one symbol per reel on the pay row).
        var outcome = new SymbolData[reels.Length];
        for(int i = 0; i < outcome.Length; i++)
        {
            outcome[i] = rng.Pick();
        }
        //2,Animate :all reels spin , then stop one after another left to right
        PlaySound(spinClip);

        foreach (Reel reel in reels) reel.StartSpin();

        yield return new WaitForSeconds(minSpinTime);

        for(int i=0;i < reels.Length; i++)
        {
            reels[i].StopOn(outcome[i]);
            yield return new WaitForSeconds(delayBetweenReelStops);
        }
        while (AnyReelSpinning()) yield return null;

        //3.Evaluate and pay out .
        SpinResult result = WinEvaluator.Evaluate(outcome);
        int payout = result.IsWin ? result.Multiplier * bet : 0;

        if(payout > 0)
        {
            Wallet.Credit(payout);
            PlaySound(winClip);

        }
        SpinFinished?.Invoke(result, payout);

    }
    private bool AnyReelSpinning()
    {
        foreach(Reel reel in reels)
        {
            if (reel.IsSpinning) return true;

        }
        return false;
    }
    private void SetBusy(bool busy)
    {
        IsBusy = busy;
        BusyChanged?.Invoke(busy);

    }
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }
}
