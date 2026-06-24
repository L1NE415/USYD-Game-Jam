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

// Fisherman variants define both the warning appearance and the capture tool
// used when Alert reaches 100.
public enum FishFishermanType
{
    Net,
    Claw,
    Electric,
    Boss
}

// High-level game states owned by FishGameManager. Most scripts read this
// instead of keeping their own copies of the current mode.
public enum FishGameState
{
    Normal,
    FishingHazard,
    Recovering,
    Caught,
    BigFishAttack,
    Victory
}
