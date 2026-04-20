using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using SQLite;
using System.Collections.Generic;
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    // 0 = player 1's turn, 1 = player 2's turn
    private NetworkVariable<ulong> currentTurnOwner = new NetworkVariable<ulong>(0);
    private NetworkVariable<bool> isResolvingTurn = new NetworkVariable<bool>(false);

    public ulong GetCurrentTurnOwner() => currentTurnOwner.Value;

    public TextMeshProUGUI turnText;
    public TextMeshProUGUI p1MagnetCount;
    public TextMeshProUGUI p2MagnetCount;

    private bool isGameOver = false;
    public bool IsGameOver => isGameOver;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        Magnet.OnMagnetDropped += HandleMagnetDropped;
    }

    void OnDisable()
    {
        Magnet.OnMagnetDropped -= HandleMagnetDropped;
    }

    public override void OnNetworkSpawn()
    {
        currentTurnOwner.OnValueChanged += (_, _) => UpdateUI();
        isResolvingTurn.OnValueChanged += (_, _) => UpdateUI();
        UpdateUI();
    }

    void HandleMagnetDropped(Magnet magnet)
    {
        // UI will update when turn resolves
    }

    public bool IsMyTurn(ulong clientId)
    {
        return !isResolvingTurn.Value && currentTurnOwner.Value == clientId;
    }

    public void OnTurnResolved()
    {

        Debug.Log($"[GameManager] OnTurnResolved called, IsServer={IsServer}");
        if (!IsServer) return;

        isResolvingTurn.Value = false;
        // Swap turn
        currentTurnOwner.Value = currentTurnOwner.Value == 0 ? 1UL : 0UL;
        UpdateUI();
    }

    public void OnMagnetDropped()
    {
        Debug.Log($"[GameManager] OnMagnetDropped called, IsServer={IsServer}");
        if (!IsServer) return;
        isResolvingTurn.Value = true;
    }

    void UpdateUI()
    {
        if (turnText == null)
        {
            Debug.LogWarning("[GameManager] turnText is null");
            return;
        }

        Debug.Log($"[GameManager] UpdateUI — resolving={isResolvingTurn.Value}, turn={currentTurnOwner.Value}");

        if (isResolvingTurn.Value)
            turnText.text = "Resolving...";
        else
            turnText.text = currentTurnOwner.Value == 0 ? "Player 1's Turn" : "Player 2's Turn";
    }
    public void UpdateMagnetCounts(int p1Count, int p2Count)
    {
        if (!IsServer) return;
        UpdateMagnetCountsClientRpc(p1Count, p2Count);
    }

    [ClientRpc]
    private void UpdateMagnetCountsClientRpc(int p1Count, int p2Count)
    {
        if (p1MagnetCount != null) p1MagnetCount.text = $"{p1Count}";
        if (p2MagnetCount != null) p2MagnetCount.text = $"{p2Count}";

        CheckWinCondition(p1Count, p2Count);
    }

    private void CheckWinCondition(int p1Count, int p2Count)
    {
        // Only server triggers the actual game over
        if (!IsServer) return;

        if (p1Count == 0)
            TriggerGameOverClientRpc(0);
        else if (p2Count == 0)
            TriggerGameOverClientRpc(1);
    }

    [ClientRpc]
    private void TriggerGameOverClientRpc(ulong winnerClientId)
    {
        isGameOver = true;

        // Store winner so Game Over scene can read it
        PlayerPrefs.SetInt("Winner", (int)winnerClientId);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameOver");
    }
}