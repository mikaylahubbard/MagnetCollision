using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource musicSource;


    public AudioClip bgMusic;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        musicSource.volume = savedVolume;

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scene == "GameScreen" || scene == "GameScreen2")
            PlayMusic(bgMusic);
    }

    public void SetVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "GameScreen" || scene.name == "GameScreen2")
            musicSource.Play();
        else
            musicSource.Stop();
    }


    // public void PlaySoundEffect(AudioClip clip)
    // {
    //     sfxSource.PlayOneShot(clip);
    // }



    // void OnEnable()
    // {
    //     GameManager.Instance.onScoreChanged += OnScoreChanged;

    //     GameManager.Instance.onHealthChanged += OnHealthChanged;
    // }

    // void OnDisable()
    // {
    //     GameManager.Instance.onScoreChanged -= OnScoreChanged;
    //     GameManager.Instance.onHealthChanged -= OnHealthChanged;
    // }

    // void OnScoreChanged(int newScore)
    // {
    //     PlaySoundEffect(coinSound);
    // }

    // void OnHealthChanged(int newHealth)
    // {
    //     PlaySoundEffect(damageSound);
    // }


}