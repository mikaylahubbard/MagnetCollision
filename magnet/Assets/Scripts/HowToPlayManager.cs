using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SQLite;
using System.Collections.Generic;

public class HowToPlayManager : MonoBehaviour
{

    public Button back;

    void Start()
    {


        if (back != null)
            back.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));


    }

}