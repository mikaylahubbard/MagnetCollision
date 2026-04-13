using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int magnetsPickedUp = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        Magnet.OnMagnetPickedUp += HandleMagnetPickedUp;
        Magnet.OnMagnetDropped += HandleMagnetDropped;
    }

    void OnDisable()
    {
        Magnet.OnMagnetPickedUp -= HandleMagnetPickedUp;
        Magnet.OnMagnetDropped -= HandleMagnetDropped;
    }

    void HandleMagnetPickedUp(Magnet magnet)
    {
        magnetsPickedUp++;
        Debug.Log("Magnet picked up! Total: " + magnetsPickedUp);
    }

    void HandleMagnetDropped(Magnet magnet)
    {
        Debug.Log("Magnet dropped!");
    }
}