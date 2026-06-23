using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip victoryMusic;

    [Header("Weapons")]
    public AudioClip projectileLaunchClip;
    public AudioClip projectileHitClip;
    public AudioClip grenadeThrowClip;
    public AudioClip grenadeExplosionClip;

    [Header("Echoes")]
    public AudioClip echoSpawnClip;
    public AudioClip echoCollectClip;
    public AudioClip upgradeChosenClip;

    [Header("Player")]
    public AudioClip dashClip;
    public AudioClip playerHitClip;
    public AudioClip playerDeathClip;

    [Header("Countdown")]
    public AudioClip countdownTickClip;
    public AudioClip countdownGoClip;

    [Header("UI")]
    public AudioClip buttonHoverClip;
    public AudioClip buttonClickClip;

    [Header("Volumes")]
    [SerializeField]
    float masterMusicVolume = 0.7f;

    [SerializeField]
    float masterSfxVolume = 1f;

    AudioSource musicSource;

    const int SFX_POOL_SIZE = 10;
    AudioSource[] sfxPool;
    int sfxPoolIndex;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = masterMusicVolume;

        sfxPool = new AudioSource[SFX_POOL_SIZE];

        for (int i = 0; i < SFX_POOL_SIZE; i++)
        {
            sfxPool[i] = gameObject.AddComponent<AudioSource>();
            sfxPool[i].playOnAwake = false;
            sfxPool[i].loop = false;
            sfxPool[i].spatialBlend = 0f;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        GameEvents.OnMatchStarted += HandleMatchStarted;
        GameEvents.OnMatchEnded += HandleMatchEnded;

        GameEvents.OnEchoSpawned += HandleEchoSpawned;
        GameEvents.OnEchoCollected += HandleEchoCollected;
        GameEvents.OnUpgradeChosen += HandleUpgradeChosen;

        GameEvents.OnCountdownTick += HandleCountdownTick;
        GameEvents.OnCountdownEnd += HandleCountdownEnd;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        GameEvents.OnMatchStarted -= HandleMatchStarted;
        GameEvents.OnMatchEnded -= HandleMatchEnded;

        GameEvents.OnEchoSpawned -= HandleEchoSpawned;
        GameEvents.OnEchoCollected -= HandleEchoCollected;
        GameEvents.OnUpgradeChosen -= HandleUpgradeChosen;

        GameEvents.OnCountdownTick -= HandleCountdownTick;
        GameEvents.OnCountdownEnd -= HandleCountdownEnd;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            PlayMusic(menuMusic);
    }

    void HandleMatchStarted()
    {
        PlayMusic(gameplayMusic);
    }

    void HandleMatchEnded()
    {
        PlayMusic(victoryMusic);
    }

    void HandleEchoSpawned(int playerIndex)
    {
        PlaySfx(echoSpawnClip);
    }

    void HandleEchoCollected(int playerIndex)
    {
        PlaySfx(echoCollectClip);
    }

    void HandleUpgradeChosen(int playerIndex)
    {
        PlaySfx(upgradeChosenClip);
    }

    void HandleCountdownTick()
    {
        PlaySfx(countdownTickClip);
    }

    void HandleCountdownEnd()
    {
        PlaySfx(countdownGoClip);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
            return;

        AudioSource src = null;

        for (int i = 0; i < SFX_POOL_SIZE; i++)
        {
            int idx = (sfxPoolIndex + i) % SFX_POOL_SIZE;

            if (!sfxPool[idx].isPlaying)
            {
                src = sfxPool[idx];
                sfxPoolIndex = (idx + 1) % SFX_POOL_SIZE;
                break;
            }
        }

        if (src == null)
        {
            src = sfxPool[sfxPoolIndex];
            sfxPoolIndex = (sfxPoolIndex + 1) % SFX_POOL_SIZE;
        }

        src.clip = clip;
        src.volume = masterSfxVolume * volumeScale;
        src.Play();
    }

    public void PlaySfxAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(clip, position, masterSfxVolume * volumeScale);
    }

    public void SetMusicVolume(float volume)
    {
        masterMusicVolume = Mathf.Clamp01(volume);
        musicSource.volume = masterMusicVolume;
    }

    public void SetSfxVolume(float volume)
    {
        masterSfxVolume = Mathf.Clamp01(volume);
    }
}
