using UnityEngine;

public struct FishBaitStats
{
    // Lightweight data table entry for one bait type. We use a struct and a
    // switch instead of ScriptableObjects so the jam prototype stays portable.
    public readonly string displayName;
    public readonly string shortLabel;
    public readonly int baseScore;
    public readonly float alertIncrease;
    public readonly Color popupColor;

    public bool IsScoring => baseScore > 0;

    public FishBaitStats(string displayName, string shortLabel, int baseScore, float alertIncrease, Color popupColor)
    {
        this.displayName = displayName;
        this.shortLabel = shortLabel;
        this.baseScore = baseScore;
        this.alertIncrease = alertIncrease;
        this.popupColor = popupColor;
    }
}
