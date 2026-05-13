using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject multiplayerPanel;
    public GameObject dinoSelectPanel;

    [Header("Scene")]
    public string gameplaySceneName = "Level_1";

    [Header("Dino Sprites")]
    public Sprite[] dinoSprites;

    [Header("Room Images")]
    public Image leftDinoImage;
    public Image rightDinoImage;

    [Header("Buttons")]
    public Button createCodeButton;
    public Button joinRoomButton;
    public Button playButton;

    [Header("Dino Buttons")]
    public Button[] dinoButtons;

    [Header("Join")]
    public TMP_InputField roomCodeInput;
    public TMP_InputField codeDisplayInput;

    private void Start()
    {
        createCodeButton.interactable = false;
        joinRoomButton.interactable = true;
        playButton.interactable = false;

        leftDinoImage.gameObject.SetActive(false);
        rightDinoImage.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

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

        dinoSelectPanel.SetActive(true);
    }

    // =========================
    // JOIN ROOM
    // =========================
    // public void JoinRoom()
    // {
    //     GameData.IsHost = false;

    //     multiplayerPanel.SetActive(false);
    //     dinoSelectPanel.SetActive(true);
    // }

    public void OpenDinoSelect()
    {
        dinoSelectPanel.SetActive(true);
    }

    // =========================
    // SELECT DINO
    // =========================
    public void SelectDino(int index)
    {
        // SINGLE PLAYER
        if (!GameData.IsMultiplayer)
        {
            GameData.SelectedDino = index;

            SceneManager.LoadScene(gameplaySceneName);

            return;
        }

        // MULTIPLAYER HOST
        if (GameData.IsHost)
        {
            RoomData.HostDino = index;

            leftDinoImage.sprite = dinoSprites[index];
            leftDinoImage.gameObject.SetActive(true);

            dinoSelectPanel.SetActive(false);

            createCodeButton.interactable = true;

            Debug.Log("HOST SELECT DINO");
        }
        else
        {
            // MULTIPLAYER CLIENT
            RoomData.ClientDino = index;

            rightDinoImage.sprite = dinoSprites[index];
            rightDinoImage.gameObject.SetActive(true);

            dinoSelectPanel.SetActive(false);

            playButton.interactable = true;

            Debug.Log("CLIENT SELECT DINO");

            UpdateRoomUI();
        }
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

    public void CreateRoomCode()
    {
        RoomData.RoomCode = Random.Range(1000, 9999).ToString();

        codeDisplayInput.text = RoomData.RoomCode;

        playButton.interactable = false;

        Debug.Log("ROOM CODE: " + RoomData.RoomCode);
    }

    public void JoinRoom()
    {
        string inputCode = roomCodeInput.text;

        GameData.IsHost = false;

        dinoSelectPanel.SetActive(true);

        DisableHostDino();

        Debug.Log("JOIN SUCCESS");

        // if (inputCode == RoomData.RoomCode)
        // {
        //     GameData.IsHost = false;

        //     dinoSelectPanel.SetActive(true);

        //     DisableHostDino();

        //     Debug.Log("JOIN SUCCESS");
        // }
        // else
        // {
        //     Debug.Log("WRONG CODE");
        // }
    }

    private void DisableHostDino()
    {
        int hostIndex = RoomData.HostDino;

        dinoButtons[hostIndex].interactable = false;
    }

    public void UpdateRoomUI()
    {
        if (RoomData.HostDino != -1)
        {
            leftDinoImage.sprite =
                dinoSprites[RoomData.HostDino];

            leftDinoImage.gameObject.SetActive(true);
        }

        if (RoomData.ClientDino != -1)
        {
            rightDinoImage.sprite =
                dinoSprites[RoomData.ClientDino];

            rightDinoImage.gameObject.SetActive(true);

            playButton.interactable = true;
        }
    }
}