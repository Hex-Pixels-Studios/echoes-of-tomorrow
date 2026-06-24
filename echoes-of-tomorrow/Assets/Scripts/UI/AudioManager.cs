using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("music")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip victoryMusic;

    [Header("combat - projectiles")]
    public AudioClip projectileLaunchClip;
    public AudioClip projectileHitClip;
    public AudioClip grenadeThrowClip;
    public AudioClip grenadeExplosionClip;
    public AudioClip missileLaunchClip;
    public AudioClip missileHitClip;

    [Header("combat ")]
    public AudioClip iceFreezeClip;
    public AudioClip iceUnfreezeClip;
    public AudioClip teleportClip;

    [Header("echoes")]
    public AudioClip echoSpawnClip;
    public AudioClip echoCollectClip;
    public AudioClip echoRejectClip; // wrong player bounced away
    public AudioClip upgradeChosenClip;

    [Header("player")]
    public AudioClip playerHitClip;
    public AudioClip playerDeathClip;
    public AudioClip playerJumpClip;
    public AudioClip playerLandClip;
    public AudioClip playerKnockbackClip;
    public AudioClip playerHealClip;
    public AudioClip staminaRestoreClip;

    [Header(" ui")]
    public AudioClip countdownTickClip;
    public AudioClip countdownGoClip;
    public AudioClip winStingerClip; // short hit when win screen appears
    public AudioClip buttonHoverClip;
    public AudioClip buttonClickClip;

    [Header("volumes")]
    [SerializeField]
    float masterMusicVolume = 0.7f;

    [SerializeField]
    float masterSfxVolume = 1f;

    AudioSource musicSource;

    const int SFX_POOL_SIZE = 16;
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
        GameEvents.OnWinScreenOpened += HandleWinScreenOpened;

        GameEvents.OnPlayerHit += _ => PlaySfx(playerHitClip);
        GameEvents.OnPlayerKilled += _ => PlaySfx(playerDeathClip);
        GameEvents.OnPlayerJump += _ => PlaySfx(playerJumpClip);
        GameEvents.OnPlayerLand += _ => PlaySfx(playerLandClip, 0.6f);
        GameEvents.OnPlayerKnockback += _ => PlaySfx(playerKnockbackClip, 0.7f);
        GameEvents.OnPlayerHealed += _ => PlaySfx(playerHealClip);
        GameEvents.OnStaminaRestored += _ => PlaySfx(staminaRestoreClip, 0.8f);

        GameEvents.OnEchoSpawned += _ => PlaySfx(echoSpawnClip);
        GameEvents.OnEchoCollected += _ => PlaySfx(echoCollectClip);
        GameEvents.OnEchoRejected += _ => PlaySfx(echoRejectClip);
        GameEvents.OnUpgradeChosen += _ => PlaySfx(upgradeChosenClip);

        GameEvents.OnProjectileFired += _ => PlaySfx(projectileLaunchClip);
        GameEvents.OnProjectileHit += _ => PlaySfx(projectileHitClip);
        GameEvents.OnGrenadeThrown += _ => PlaySfx(grenadeThrowClip);
        GameEvents.OnGrenadeExploded += _ => PlaySfx(grenadeExplosionClip);
        GameEvents.OnIceFreeze += _ => PlaySfx(iceFreezeClip);
        GameEvents.OnTeleport += _ => PlaySfx(teleportClip);

        GameEvents.OnCountdownTick += HandleCountdownTick;
        GameEvents.OnCountdownEnd += HandleCountdownEnd;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        GameEvents.OnMatchStarted -= HandleMatchStarted;
        GameEvents.OnMatchEnded -= HandleMatchEnded;
        GameEvents.OnWinScreenOpened -= HandleWinScreenOpened;

        GameEvents.OnPlayerHit -= _ => PlaySfx(playerHitClip);
        GameEvents.OnPlayerKilled -= _ => PlaySfx(playerDeathClip);
        GameEvents.OnPlayerJump -= _ => PlaySfx(playerJumpClip);
        GameEvents.OnPlayerLand -= _ => PlaySfx(playerLandClip, 0.6f);
        GameEvents.OnPlayerKnockback -= _ => PlaySfx(playerKnockbackClip, 0.7f);
        GameEvents.OnPlayerHealed -= _ => PlaySfx(playerHealClip);
        GameEvents.OnStaminaRestored -= _ => PlaySfx(staminaRestoreClip, 0.8f);

        GameEvents.OnEchoSpawned -= _ => PlaySfx(echoSpawnClip);
        GameEvents.OnEchoCollected -= _ => PlaySfx(echoCollectClip);
        GameEvents.OnEchoRejected -= _ => PlaySfx(echoRejectClip);
        GameEvents.OnUpgradeChosen -= _ => PlaySfx(upgradeChosenClip);

        GameEvents.OnProjectileFired -= _ => PlaySfx(projectileLaunchClip);
        GameEvents.OnProjectileHit -= _ => PlaySfx(projectileHitClip);
        GameEvents.OnGrenadeThrown -= _ => PlaySfx(grenadeThrowClip);
        GameEvents.OnGrenadeExploded -= _ => PlaySfx(grenadeExplosionClip);
        GameEvents.OnIceFreeze -= _ => PlaySfx(iceFreezeClip);
        GameEvents.OnTeleport -= _ => PlaySfx(teleportClip);

        GameEvents.OnCountdownTick -= HandleCountdownTick;
        GameEvents.OnCountdownEnd -= HandleCountdownEnd;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            PlayMusic(menuMusic);
    }

    void HandleMatchStarted() => PlayMusic(gameplayMusic);

    void HandleMatchEnded() => PlayMusic(victoryMusic);

    void HandleWinScreenOpened() => PlaySfx(winStingerClip);

    void HandleCountdownTick() => PlaySfx(countdownTickClip);

    void HandleCountdownEnd() => PlaySfx(countdownGoClip);

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

    public void StopMusic() => musicSource.Stop();

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

    public void SetMusicVolume(float v)
    {
        masterMusicVolume = Mathf.Clamp01(v);
        musicSource.volume = masterMusicVolume;
    }

    public void SetSfxVolume(float v) => masterSfxVolume = Mathf.Clamp01(v);
}
