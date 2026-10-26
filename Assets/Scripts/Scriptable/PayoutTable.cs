using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PayoutTable", menuName = "Slots/PayoutTable")]
public class PayoutTable : ScriptableObject
{
    [SerializeField]
    private List<SymbolData> symbolId = new List<SymbolData>();

    public IReadOnlyList<SymbolData> Symbols => symbolId;

    [ContextMenu("Simulate 100k spins (See Console)")]
    private void SimulateSpins()
    {
        const int spins = 100000;

        // Change this to match your slot game's actual reel count.
        const int reelCount = 3;

        var rng = new RngService(this);
        var line = new SymbolData[reelCount];

        long returned = 0;
        int wins = 0;

        // Simulate spins.
        for (int i = 0; i < spins; i++)
        {
            for (int r = 0; r < reelCount; r++)
            {
                line[r] = rng.Pick();
            }

            SpinResult result = WinEvaluator.Evaluate(line);

            if (!result.IsWin)
                continue;

            wins++;
            returned += result.Multiplier;
        }

        float winRate = 100f * wins / spins;
        float rtp = 100f * returned / spins;

        Debug.Log(
            $"Simulated {spins} spins: Win Rate: {winRate:F2}% | RTP: {rtp:F2}%"
        );
    }
}