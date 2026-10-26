using UnityEngine;
using System;

/// <summary>
/// Represents a wallet component in a Unity application.
/// </summary>
public class WalletModel 
{
    private readonly int[] betOptions;
    private int betIndex;

    public int Balance { get; private set; }
    public int Bet => betOptions[betIndex];
    public bool CanAffordBet => Balance >= Bet;

    ////<summary> Raised whenever balance or bet changes so the UI can refresh.</summary>

    public event Action Changed;

    public WalletModel(int startingBalance, int[] betOptions)
    {
        if(betOptions == null || betOptions.Length == 0)
        {
            throw new ArgumentException("At least one bet option is required.");
        }
        this.betOptions = betOptions;
        Balance = startingBalance;
    }
    ///<summary> Deducts the current bet.Return false if the player cannot afford it.</summary>
    public bool TryPlaceBet()
    {
        if (!CanAffordBet) return false;

        Balance -= Bet;
        Changed?.Invoke();
        return true;

    }
    public void Credit(int amout)
    {
        Balance += amout;
        Changed?.Invoke();

    }
    ///<summary> Moves to the next (+1) or previous (-1) bet option.</summary>

public void ChangeBet(int direction)
    {
        betIndex = Math.Min(Math.Max(betIndex + direction, 0), betOptions.Length - 1);
        Changed?.Invoke();

    }
    public void Reset(int startingBalance)
    {
        Balance = startingBalance;
        Changed?.Invoke();

    }
}
