using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public TextMeshProUGUI winnerText;
    public Button playBoard1;

    public Button playBoard2;

    void Start()
    {
        int winner = PlayerPrefs.GetInt("Winner", 0);
        string winnerName = winner == 0 ? "Player 1" : "Player 2";

        if (winnerText != null)
            winnerText.text = $"{winnerName} Wins!";

        if (playBoard1 != null)
            playBoard1.onClick.AddListener(() => PlayAgain(1));

        if (playBoard2 != null)
            playBoard2.onClick.AddListener(() => PlayAgain(2));


    }

    public void PlayAgain(int boardNumber)
    {
        if (Unity.Netcode.NetworkManager.Singleton != null)
            Unity.Netcode.NetworkManager.Singleton.Shutdown();

        if (GameManager.Instance != null)
            Destroy(GameManager.Instance.gameObject);

        PlayerPrefs.DeleteKey("Winner");

        switch (boardNumber)
        {
            case 1:
                SceneManager.LoadScene("GameScreen");
                break;
            case 2:
                SceneManager.LoadScene("GameScreen2");
                break;
        }

    }
}