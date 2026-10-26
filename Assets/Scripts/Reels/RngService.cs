using System;
using System.Collections.Generic;

/// <summary>
/// Weighted random picker.
/// Kept as plain C# for easy testing.
/// </summary>
public class RngService
{
    private readonly System.Random random;
    private readonly IReadOnlyList<SymbolData> symbols;
    private readonly int totalWeight;

    public RngService(PayoutTable table, int? seed = null)
    {
        symbols = table.Symbols;
        random = seed.HasValue
            ? new System.Random(seed.Value)
            : new System.Random();

        int weightSum = 0;

        foreach (SymbolData symbol in symbols)
        {
            if (symbol.Weight > 0)
            {
                weightSum += symbol.Weight;
            }
        }

        totalWeight = weightSum;

        if (symbols.Count == 0 || totalWeight <= 0)
        {
            throw new InvalidOperationException(
                "PayoutTable needs at least one symbol with weight > 0."
            );
        }
    }

    public SymbolData Pick()
    {
        int roll = random.Next(totalWeight);

        foreach (SymbolData symbol in symbols)
        {
            if (symbol.Weight <= 0)
                continue;

            roll -= symbol.Weight;

            if (roll < 0)
                return symbol;
        }

        throw new InvalidOperationException(
            "Failed to select a symbol."
        );
    }
}