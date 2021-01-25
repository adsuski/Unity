using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

public class GameOverWindow : MonoBehaviour
{
    private Text scoreText;

    private void Awake()
    {
        scoreText = transform.Find("scoreText").GetComponent<Text>();

        transform.Find("retryBtn").GetComponent<Button_UI>().ClickFunc = () =>
        {
            Loader.Load(Loader.Scene.GameScene);
        };
        transform.Find("mainMenuBtn").GetComponent<Button_UI>().ClickFunc = () =>
        {
            Loader.Load(Loader.Scene.MainMenu);
        };
    }

    
    private void Start()
    {
        Bird.GetInstance().Smierc += Bird_Smierc;
        Hide();
    }

    private void Bird_Smierc(object sender,System.EventArgs e)
    {
        scoreText.text = Level.GetInstance().pobierzLiczbePokonanychRur().ToString();
        Show();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
}
