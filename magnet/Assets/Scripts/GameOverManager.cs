using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public TextMeshProUGUI winnerText;
    public Button playAgainButton;

    void Start()
    {
        int winner = PlayerPrefs.GetInt("Winner", 0);
        string winnerName = winner == 0 ? "Player 1" : "Player 2";

        if (winnerText != null)
            winnerText.text = $"{winnerName} Wins!";

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(PlayAgain);
    }

    public void PlayAgain()
    {
        if (Unity.Netcode.NetworkManager.Singleton != null)
            Unity.Netcode.NetworkManager.Singleton.Shutdown();

        if (GameManager.Instance != null)
            Destroy(GameManager.Instance.gameObject);

        PlayerPrefs.DeleteKey("Winner");
        SceneManager.LoadScene("GameScreen");
    }
}