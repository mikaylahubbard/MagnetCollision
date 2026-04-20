using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class MenuManager : MonoBehaviour
{
    public Button hostButton;
    public Button joinButton;
    public Button howToPlayButton;

    void Start()
    {
        if (howToPlayButton != null)
            howToPlayButton.onClick.AddListener(() => SceneManager.LoadScene("HowToPlay"));

        if (hostButton != null)
            hostButton.onClick.AddListener(StartHost);

        if (joinButton != null)
            joinButton.onClick.AddListener(StartClient);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("GameScreen", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}