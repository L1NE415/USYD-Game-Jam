using UnityEngine;

public enum FishDifficulty
{
    Easy = 0,
    Normal = 1,
    Hard = 2
}

// Central place for player-facing settings. UI writes these values, while
// gameplay scripts read the multipliers so the menu actually changes the run.
public static class FishGameSettings
{
    private const string LegacyVolumePrefKey = "FishBalatro.Settings.Volume";
    private const string MusicVolumePrefKey = "FishBalatro.Settings.MusicVolume";
    private const string SfxVolumePrefKey = "FishBalatro.Settings.SfxVolume";
    private const string UiScalePrefKey = "FishBalatro.Settings.UiScale";
    private const string DifficultyPrefKey = "FishBalatro.Settings.Difficulty";

    private static bool loaded;

    public static float MusicVolume { get; private set; } = 1f;
    public static float SfxVolume { get; private set; } = 1f;
    public static float UiScale { get; private set; } = 1f;
    public static FishDifficulty Difficulty { get; private set; } = FishDifficulty.Normal;

    public static float AlertMultiplier
    {
        get
        {
            EnsureLoaded();
            switch (Difficulty)
            {
                case FishDifficulty.Easy:
                    return 0.78f;
                case FishDifficulty.Hard:
                    return 1.22f;
                default:
                    return 1f;
            }
        }
    }

    public static float ToolDurationMultiplier
    {
        get
        {
            EnsureLoaded();
            switch (Difficulty)
            {
                case FishDifficulty.Easy:
                    return 1.18f;
                case FishDifficulty.Hard:
                    return 0.84f;
                default:
                    return 1f;
            }
        }
    }

    public static int BaitTargetBonus
    {
        get
        {
            EnsureLoaded();
            switch (Difficulty)
            {
                case FishDifficulty.Easy:
                    return 1;
                case FishDifficulty.Hard:
                    return -1;
                default:
                    return 0;
            }
        }
    }

    public static int BaitBudgetBonus
    {
        get
        {
            EnsureLoaded();
            switch (Difficulty)
            {
                case FishDifficulty.Easy:
                    return 4;
                case FishDifficulty.Hard:
                    return -3;
                default:
                    return 0;
            }
        }
    }

    public static void EnsureLoaded()
    {
        if (loaded)
        {
            return;
        }

        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumePrefKey, PlayerPrefs.GetFloat(LegacyVolumePrefKey, 1f)));
        SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumePrefKey, 1f));
        UiScale = Mathf.Clamp(PlayerPrefs.GetFloat(UiScalePrefKey, 1f), 0.85f, 1.25f);
        Difficulty = (FishDifficulty)Mathf.Clamp(PlayerPrefs.GetInt(DifficultyPrefKey, (int)FishDifficulty.Normal), 0, 2);
        loaded = true;
        ApplyAudioFallback();
    }

    public static void SetMusicVolume(float value)
    {
        EnsureLoaded();
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumePrefKey, MusicVolume);
        PlayerPrefs.Save();
        ApplyAudioFallback();
    }

    public static void SetSfxVolume(float value)
    {
        EnsureLoaded();
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumePrefKey, SfxVolume);
        PlayerPrefs.Save();
        ApplyAudioFallback();
    }

    public static void SetUiScale(float value)
    {
        EnsureLoaded();
        UiScale = Mathf.Clamp(value, 0.85f, 1.25f);
        PlayerPrefs.SetFloat(UiScalePrefKey, UiScale);
        PlayerPrefs.Save();
    }

    public static void SetDifficulty(FishDifficulty difficulty)
    {
        EnsureLoaded();
        Difficulty = difficulty;
        PlayerPrefs.SetInt(DifficultyPrefKey, (int)Difficulty);
        PlayerPrefs.Save();
    }

    public static string GetDifficultyLabel()
    {
        EnsureLoaded();
        switch (Difficulty)
        {
            case FishDifficulty.Easy:
                return "EASY";
            case FishDifficulty.Hard:
                return "HARD";
            default:
                return "NORMAL";
        }
    }

    private static void ApplyAudioFallback()
    {
        // Until the project has separate AudioMixer groups, keep both sliders
        // meaningful by using the loudest channel as the global listener volume.
        AudioListener.volume = Mathf.Max(MusicVolume, SfxVolume);
    }
}
