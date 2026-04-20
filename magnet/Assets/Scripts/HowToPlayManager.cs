using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HowToPlayManager : MonoBehaviour
{

    public Button back;

    void Start()
    {


        if (back != null)
            back.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));


    }

}