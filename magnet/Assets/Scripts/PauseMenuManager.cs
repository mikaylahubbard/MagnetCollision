using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using SQLite;
using System.Collections.Generic;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pausePanel;
    public Slider volumeSlider;
    public Toggle musicToggle;

    public Button resume;

    public Button mainMenu;

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);

        // Initialize slider and toggle to match current audio state
        if (AudioManager.Instance != null)
        {
            volumeSlider.value = AudioManager.Instance.musicSource.volume;
            musicToggle.isOn = AudioManager.Instance.musicSource.isPlaying;
        }

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        musicToggle.onValueChanged.AddListener(OnMusicToggled);


        if (resume != null)
            resume.onClick.AddListener(TogglePause);

        if (mainMenu != null)
            mainMenu.onClick.AddListener(GoToMainMenu);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
    }

    void OnVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume(value);
    }

    void OnMusicToggled(bool isOn)
    {
        if (AudioManager.Instance == null) return;

        if (isOn)
            AudioManager.Instance.musicSource.Play();
        else
            AudioManager.Instance.musicSource.Pause();
    }

    public void GoToMainMenu()
    {
        isPaused = false;
        pausePanel.SetActive(false);

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        if (GameManager.Instance != null)
            Destroy(GameManager.Instance.gameObject);

        SceneManager.LoadScene("MainMenu");
    }
}