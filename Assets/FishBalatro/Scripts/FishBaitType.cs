// Bait types are the "cards" of this prototype. Add new bait behavior in
// BaitPickup.GetStats and FishGameManager.ApplyBaitEffect.
public enum FishBaitType
{
    Worm,
    Shrimp,
    GlowBug,
    SmallFish,
    GoldenShrimp,
    FakeBait
}

// High-level game states owned by FishGameManager. Most scripts read this
// instead of keeping their own copies of the current mode.
public enum FishGameState
{
    Normal,
    Hooked,
    Recovering,
    Caught,
    BigFishAttack
}
