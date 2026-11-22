
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music")]
    public AudioSource musicSource;
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;

    [Header("Sound Effects")]
    public AudioSource sfxSource;
    public AudioClip diceRollSound;
    public AudioClip powerUpSound1;
    public AudioClip powerUpSound2;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 1f;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string gameplaySceneName = "GameScene";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // Load saved volume settings
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        ApplyVolumes();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForScene(
            SceneManager.GetActiveScene().name
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        if (sceneName == mainMenuSceneName)
        {
            PlayMusic(mainMenuMusic);
        }
        else if (sceneName == gameplaySceneName)
        {
            PlayMusic(gameplayMusic);
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // MUSIC VOLUME
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (musicSource != null)
            musicSource.volume = musicVolume;

        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    // SOUND EFFECT VOLUME
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    // SOUND EFFECTS
    public void PlayDiceRoll()
    {
        PlaySFX(diceRollSound);
    }

    public void PlayPowerUp1()
    {
        PlaySFX(powerUpSound1);
    }

    public void PlayPowerUp2()
    {
        PlaySFX(powerUpSound2);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
