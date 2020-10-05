using System.Collections;
using System.Collections.Generic;
using Doozy.Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public UIView view;
    public UIButton menuButton;
    public UIButton restartButton;

    public TMP_Text achievement;

    public void GameOver(int level)
    {
        achievement.text = $"YOU MADE IT TO LEVEL {level}";
        view.Show();
    }

    public void Restart()
    {
        // TODO: Stefan
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
