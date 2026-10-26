using UnityEngine;
using System.Collections.Generic;

public readonly struct SpinResult
{
    public bool IsWin { get; }
    public SymbolData WinningSymbol { get; }
    public int Multiplier { get; }

    public SpinResult(bool isWin, SymbolData winningSymbol,int multiplier)
    {
        IsWin = isWin;
        WinningSymbol = winningSymbol;
        Multiplier = multiplier;
    }
    public static SpinResult Loss => new SpinResult(false, null, 0);
}
/// <summary>
/// Pure win logic: the player wins when every reel show the same symbol
/// wild symbols count as a match for any symbol 
/// </summary>

public static class WinEvaluator 
{
    public static SpinResult Evaluate(IReadOnlyList<SymbolData> line)
    {
        if (line == null || line.Count == 0) return SpinResult.Loss;

        //find the first non wild symbol every other non- wild must match it.

        SymbolData target = null;
        foreach (SymbolData symbol in line)
        {
            if (!symbol.IsWild)
            {
                target = symbol;
                break;
            }
        }

        //Every reel is a wild : pay using the wild's own multiplier.
        if (target == null) return new SpinResult(true, line[0], line[0].PayoutMultiplier);
        foreach (SymbolData symbol in line) {
            if (!symbol.IsWild && symbol != target) return SpinResult.Loss;
         }
        return new SpinResult(true, target, target.PayoutMultiplier);
    }
  
}
