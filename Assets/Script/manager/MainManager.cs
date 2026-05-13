using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject multiplayerPanel;
    public GameObject dinoSelectPanel;

    [Header("Scene")]
    public string gameplaySceneName = "Level_1";

    // =========================
    // SINGLE PLAYER
    // =========================
    public void OpenSinglePlayer()
    {
        GameData.IsMultiplayer = false;

        mainPanel.SetActive(false);
        dinoSelectPanel.SetActive(true);
    }

    // =========================
    // MULTIPLAYER
    // =========================
    public void OpenMultiplayer()
    {
        GameData.IsMultiplayer = true;

        mainPanel.SetActive(false);
        multiplayerPanel.SetActive(true);
    }

    // =========================
    // CREATE ROOM
    // =========================
    public void CreateRoom()
    {
        GameData.IsHost = true;

        multiplayerPanel.SetActive(false);
        dinoSelectPanel.SetActive(true);
    }

    // =========================
    // JOIN ROOM
    // =========================
    public void JoinRoom()
    {
        GameData.IsHost = false;

        multiplayerPanel.SetActive(false);
        dinoSelectPanel.SetActive(true);
    }

    // =========================
    // SELECT DINO
    // =========================
    public void SelectDino(int dinoIndex)
    {
        GameData.SelectedDino = dinoIndex;

        SceneManager.LoadScene(gameplaySceneName);
    }

    // =========================
    // BACK BUTTON
    // =========================
    public void BackToMainMenu()
    {
        multiplayerPanel.SetActive(false);
        dinoSelectPanel.SetActive(false);

        mainPanel.SetActive(true);
    }

    // =========================
    // CLOSE CURRENT PANEL
    // =========================
    public void CloseAllPanels()
    {
        multiplayerPanel.SetActive(false);
        dinoSelectPanel.SetActive(false);

        mainPanel.SetActive(true);
    }

    // =========================
    // CLOSE DINO PANEL
    // =========================
    public void CloseDinoPanel()
    {
        dinoSelectPanel.SetActive(false);

        if (GameData.IsMultiplayer)
        {
            multiplayerPanel.SetActive(true);
        }
        else
        {
            mainPanel.SetActive(true);
        }
    }

    // =========================
    // CLOSE MULTIPLAYER PANEL
    // =========================
    public void CloseMultiplayerPanel()
    {
        multiplayerPanel.SetActive(false);
        Debug.Log("Close Multi");

        mainPanel.SetActive(true);
    }

    // =========================
    // QUIT GAME
    // =========================
    public void QuitGame()
    {
        Debug.Log("QUIT GAME");

        Application.Quit();
    }
}