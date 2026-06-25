using UnityEngine;

public enum FishAudioCue
{
    EatBait,
    FishDie,
    FishRodLevel1,
    LaserLevel2,
    CageLevel3,
    SharkRoar
}

// Lightweight runtime audio hub. Clips live under Resources/Audio so artists can
// replace same-named files without touching scene references.
public class FishAudioManager : MonoBehaviour
{
    private const string AudioPath = "Audio/";
    private const float MusicOutputScale = 0.62f;
    private const float SfxOutputScale = 0.68f;
    private const float SwimOutputScale = 0.26f;

    private static FishAudioManager instance;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource swimSource;

    private AudioClip backgroundMusic;
    private AudioClip eatBait;
    private AudioClip fishDie;
    private AudioClip fishRodLevel1;
    private AudioClip fishSwim;
    private AudioClip laserLevel2;
    private AudioClip cageLevel3;
    private AudioClip sharkRoar;

    public static FishAudioManager Instance => EnsureExists();
    public static bool HasInstance => instance != null;

    public static FishAudioManager EnsureExists()
    {
        if (instance != null)
        {
            return instance;
        }

        instance = FindFirstObjectByType<FishAudioManager>();
        if (instance != null)
        {
            return instance;
        }

        GameObject audioObject = new GameObject("Fish Audio Manager");
        instance = audioObject.AddComponent<FishAudioManager>();
        return instance;
    }

    public static void PlayCue(FishAudioCue cue)
    {
        EnsureExists().Play(cue);
    }

    public static void PlayCaptureToolCue(FishFishermanType type)
    {
        switch (type)
        {
            case FishFishermanType.Claw:
                PlayCue(FishAudioCue.FishRodLevel1);
                break;
            case FishFishermanType.Electric:
                PlayCue(FishAudioCue.LaserLevel2);
                break;
            case FishFishermanType.Net:
                PlayCue(FishAudioCue.CageLevel3);
                break;
        }
    }

    public static void SetPlayerSwimming(bool swimming)
    {
        EnsureExists().SetSwimming(swimming);
    }

    public void PlayBackgroundMusic()
    {
        EnsureAudioSources();

        backgroundMusic = backgroundMusic != null ? backgroundMusic : LoadClip("Background");
        if (backgroundMusic == null)
        {
            return;
        }

        if (musicSource.clip != backgroundMusic)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
        }

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }

        SyncVolumes();
    }

    public void SyncVolumes()
    {
        FishGameSettings.EnsureLoaded();
        EnsureAudioSources();

        musicSource.volume = FishGameSettings.MusicVolume * MusicOutputScale;
        sfxSource.volume = FishGameSettings.SfxVolume * SfxOutputScale;
        swimSource.volume = FishGameSettings.SfxVolume * SwimOutputScale;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureAudioSources();
        EnsureAudioListener();
        SyncVolumes();
    }

    private void Update()
    {
        SyncVolumes();
    }

    private void OnDisable()
    {
        if (swimSource != null)
        {
            swimSource.Stop();
        }
    }

    private void Play(FishAudioCue cue)
    {
        EnsureAudioSources();

        AudioClip clip = GetCueClip(cue);
        if (clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, GetCueVolume(cue));
    }

    private void SetSwimming(bool swimming)
    {
        EnsureAudioSources();

        fishSwim = fishSwim != null ? fishSwim : LoadClip("FishSwim");
        if (fishSwim == null)
        {
            return;
        }

        if (swimSource.clip != fishSwim)
        {
            swimSource.clip = fishSwim;
            swimSource.loop = true;
        }

        if (swimming)
        {
            if (!swimSource.isPlaying)
            {
                swimSource.Play();
            }
        }
        else if (swimSource.isPlaying)
        {
            swimSource.Stop();
        }
    }

    private AudioClip GetCueClip(FishAudioCue cue)
    {
        switch (cue)
        {
            case FishAudioCue.EatBait:
                eatBait = eatBait != null ? eatBait : LoadClip("EatBait");
                return eatBait;
            case FishAudioCue.FishDie:
                fishDie = fishDie != null ? fishDie : LoadClip("FishDie");
                return fishDie;
            case FishAudioCue.FishRodLevel1:
                fishRodLevel1 = fishRodLevel1 != null ? fishRodLevel1 : LoadClip("FishRod_Level1");
                return fishRodLevel1;
            case FishAudioCue.LaserLevel2:
                laserLevel2 = laserLevel2 != null ? laserLevel2 : LoadClip("Laser_Level2");
                return laserLevel2;
            case FishAudioCue.CageLevel3:
                cageLevel3 = cageLevel3 != null ? cageLevel3 : LoadClip("Cage_Level3");
                return cageLevel3;
            case FishAudioCue.SharkRoar:
                sharkRoar = sharkRoar != null ? sharkRoar : LoadClip("SharkRoar");
                return sharkRoar;
            default:
                return null;
        }
    }

    private static float GetCueVolume(FishAudioCue cue)
    {
        switch (cue)
        {
            case FishAudioCue.EatBait:
                return 0.85f;
            case FishAudioCue.FishDie:
                return 0.9f;
            case FishAudioCue.SharkRoar:
                return 0.95f;
            case FishAudioCue.FishRodLevel1:
            case FishAudioCue.CageLevel3:
                return 0.78f;
            case FishAudioCue.LaserLevel2:
                return 0.46f;
            default:
                return 1f;
        }
    }

    private static AudioClip LoadClip(string clipName)
    {
        return Resources.Load<AudioClip>(AudioPath + clipName);
    }

    private void EnsureAudioSources()
    {
        musicSource = musicSource != null ? musicSource : CreateSource("Music Source");
        sfxSource = sfxSource != null ? sfxSource : CreateSource("SFX Source");
        swimSource = swimSource != null ? swimSource : CreateSource("Swim Source");
    }

    private AudioSource CreateSource(string sourceName)
    {
        Transform existing = transform.Find(sourceName);
        GameObject sourceObject = existing != null ? existing.gameObject : new GameObject(sourceName);
        sourceObject.transform.SetParent(transform, false);

        AudioSource source = sourceObject.GetComponent<AudioSource>();
        if (source == null)
        {
            source = sourceObject.AddComponent<AudioSource>();
        }

        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }

    private void EnsureAudioListener()
    {
        if (FindFirstObjectByType<AudioListener>() == null)
        {
            gameObject.AddComponent<AudioListener>();
        }
    }
}
