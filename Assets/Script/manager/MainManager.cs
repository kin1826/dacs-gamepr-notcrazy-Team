using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

/// <summary>
/// Main manager for UI flow and scene management.
/// Handles single-player vs multiplayer routing and scene loading.
/// Ensures network-synchronized gameplay for multiplayer.
/// </summary>
public class MainManager : MonoBehaviour
{
    public static MainManager Instance { get; private set; }

    [Header("User")]
    public TMP_Text playerNameText;
    public TMP_Text goldText;

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject multiplayerPanel;
    public GameObject dinoSelectPanel;
    public GameObject levelSelectPanel;
    public GameObject weaponPanel;
    public GameObject giftPanel;
    public GameObject rankingPanel;

    [Header("Scene")]
    public string gameplaySceneName = "Level_01";

    [Header("Dino Sprites")]
    public Sprite[] dinoSprites;

    [Header("Room Images")]
    public Image leftDinoImage;
    public Image rightDinoImage;

    [Header("Buttons")]
    public Button joinRoomButton;
    public Button playButton;

    [Header("Dino Buttons")]
    public Button[] dinoButtons;

    [Header("Join")]
    public TMP_InputField roomCodeInput;
    public TMP_InputField codeDisplayInput;

    [Header("Audio")]
    public int lobbyMusicIndex = 0;
    public AudioClip clickSFX;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate MainManager found! Destroying this instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SaveManager.Load();
    }

    private void Start()
    {
        NetworkConfig.EnsureInstance();
        ValidateReferences();

        joinRoomButton.interactable = true;
        playButton.interactable = false;

        leftDinoImage.gameObject.SetActive(false);
        rightDinoImage.gameObject.SetActive(false);

        AudioManager.Instance?.PlayMusic(lobbyMusicIndex);
        RefreshPlayerName();
    }

    public void RefreshPlayerName()
    {
        if (playerNameText == null) return;
        playerNameText.text = UserSession.Current != null ? UserSession.Current.name : "";
    }

    public void RefreshGold()
    {
        if (goldText == null) return;
        goldText.text = UserSession.Current != null ? $"{UserSession.Current.gold}" : "0";
    }

    /// <summary>
    /// Validates all required UI references are assigned.
    /// </summary>
    private void ValidateReferences()
    {
        // if (mainPanel == null) Debug.LogError("MainManager: mainPanel not assigned!");
        // if (multiplayerPanel == null) Debug.LogError("MainManager: multiplayerPanel not assigned!");
        // if (dinoSelectPanel == null) Debug.LogError("MainManager: dinoSelectPanel not assigned!");
        // if (dinoSprites == null || dinoSprites.Length == 0) Debug.LogError("MainManager: dinoSprites not assigned!");
        // if (leftDinoImage == null) Debug.LogError("MainManager: leftDinoImage not assigned!");
        // if (rightDinoImage == null) Debug.LogError("MainManager: rightDinoImage not assigned!");
        // if (createCodeButton == null) Debug.LogError("MainManager: createCodeButton not assigned!");
        // if (joinRoomButton == null) Debug.LogError("MainManager: joinRoomButton not assigned!");
        // if (playButton == null) Debug.LogError("MainManager: playButton not assigned!");
        // if (roomCodeInput == null) Debug.LogError("MainManager: roomCodeInput not assigned!");
        // if (codeDisplayInput == null) Debug.LogError("MainManager: codeDisplayInput not assigned!");
    }

    private void Update()
    {
        // Only update UI if multiplayer and LobbyManager is available
        if (GameData.IsMultiplayer && LobbyManager.Instance != null)
        {
            UpdateRoomUI();
        }
    }

    public void StartGame()
    {
        gameplaySceneName = $"Level_{GameData.SelectedLevel:00}";
        Debug.Log($"MainManager: StartGame with scene {gameplaySceneName}");

        if (string.IsNullOrEmpty(gameplaySceneName))
        {
            Debug.LogError("gameplaySceneName not set!");
            return;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Only the host can start the multiplayer game.");
                return;
            }

            // Multiplayer - use network-synchronized loading
            Debug.Log("Starting multiplayer game via network...");
            NetworkManager.Singleton.SceneManager.LoadScene(
                gameplaySceneName,
                LoadSceneMode.Single
            );
        }
        else
        {
            // Single player
            Debug.Log("Starting single player game...");
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    /// <summary>
    /// Load any scene by name using the same logic as StartGame (multiplayer host uses Netcode).
    /// </summary>
    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("LoadLevel: sceneName is null or empty");
            return;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Only the host can start the multiplayer game.");
                return;
            }

            Debug.Log($"MainManager: Loading multiplayer scene via network... {sceneName}");
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log($"MainManager: Loading local scene... {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }

    // =========================
    // AUDIO
    // =========================
    public void PlayClick()
    {
        if (clickSFX != null)
            AudioManager.Instance?.PlaySFX(clickSFX);
    }

    // =========================
    // SINGLE PLAYER
    // =========================
    public void OpenSinglePlayer()
    {
        GameData.IsMultiplayer = false;

        // mainPanel.SetActive(false);
        dinoSelectPanel.SetActive(true);
    }

    // =========================
    // MULTIPLAYER
    // =========================
    public void OpenMultiplayer()
    {
        GameData.IsMultiplayer = true;

        // mainPanel.SetActive(false);
        multiplayerPanel.SetActive(true);
    }

    public void OpenLevelSelect()
    {
        levelSelectPanel.SetActive(true);
        LevelSelectManager.Instance?.GenerateAgain();
    }

    // =========================
    // CREATE ROOM
    // =========================
    public async void CreateRoom()
    {
        NetworkConfig networkConfig = NetworkConfig.EnsureInstance();
        if (networkConfig == null)
        {
            Debug.LogError("NetworkConfig not found!");
            return;
        }

        GameData.IsHost = true;

        try
        {
            // Use relay for online multiplayer
            string relayCode = await networkConfig.CreateRelay();
            if (relayCode != null)
            {
                RoomData.RoomCode = relayCode;
                if (codeDisplayInput != null)
                {
                    codeDisplayInput.text = relayCode;
                }
                Debug.Log($"Share this Relay join code with the client: {relayCode}");
                dinoSelectPanel.SetActive(true);
                Debug.Log($"Room created with relay code: {relayCode}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to create room: {ex.Message}");
        }

        LobbyManager.Instance?.SetMinLevelRpc(SaveManager.Data.highestLevel);
    }

    // =========================
    // SELECT DINO
    // =========================
    public void SelectDino(int index)
    {
        // Validate dino index
        if (index < 0 || index >= dinoSprites.Length)
        {
            Debug.LogError($"Invalid dino index: {index}");
            return;
        }

        // SINGLE PLAYER
        if (!GameData.IsMultiplayer)
        {
            GameData.SelectedDino = index;

            //Save
            SaveManager.Data.selectedDino =index;
            SaveManager.Save();

            PlayerSkinManager.LocalPlayer?.SetSkin(index);
            Debug.Log($"Single player selected dino: {index}");
            OpenLevelSelect();
            // StartGame();
            return;
        }

        // MULTIPLAYER HOST
        if (GameData.IsHost)
        {
            GameData.SelectedDino = index;
            PlayerSkinManager.LocalPlayer?.SetSkin(index);

            if (LobbyManager.Instance == null)
            {
                Debug.LogError("LobbyManager not found!");
                return;
            }

            LobbyManager.Instance.SetHostDinoRpc(index);
            leftDinoImage.sprite = dinoSprites[index];
            leftDinoImage.gameObject.SetActive(true);
            dinoSelectPanel.SetActive(false);
            playButton.interactable = false;
            Debug.Log($"Host selected dino: {index}");
        }
        else
        {
            // MULTIPLAYER CLIENT
            GameData.SelectedDino = index;
            PlayerSkinManager.LocalPlayer?.SetSkin(index);

            if (LobbyManager.Instance == null)
            {
                Debug.LogError("LobbyManager not found!");
                return;
            }

            LobbyManager.Instance.SetClientDinoRpc(index);          
            rightDinoImage.sprite = dinoSprites[index];
            rightDinoImage.gameObject.SetActive(true);
            dinoSelectPanel.SetActive(false);
            playButton.interactable = false;
            Debug.Log($"Client selected dino: {index}");
            UpdateRoomUI();

            LobbyManager.Instance?.SetMinLevelRpc(SaveManager.Data.highestLevel);

            LevelSelectManager.Instance?.GenerateAgain();
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
    public void CloseLevelSelectPanel()
    {
        levelSelectPanel.SetActive(false);
    }

    // =========================
    // QUIT GAME
    // =========================
    public void QuitGame()
    {
        Debug.Log("QUIT GAME");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Creates room code via Unity Relay service.
    /// Called automatically when creating room - no manual code generation needed.
    /// </summary>
    public void CreateRoomCode()
    {
        string currentRelayCode = NetworkConfig.Instance != null ? NetworkConfig.Instance.relayCode : RoomData.RoomCode;

        if (string.IsNullOrWhiteSpace(currentRelayCode))
        {
            Debug.LogWarning("No Relay room exists yet. Press Create Room first.");
            return;
        }

        if (codeDisplayInput != null)
        {
            codeDisplayInput.text = currentRelayCode;
        }

        Debug.Log($"Current Relay join code: {currentRelayCode}");
    }

    /// <summary>
    /// Join room using relay join code.
    /// Validates code with Unity Relay service.
    /// </summary>
    public async void JoinRoom()
    {
        if (roomCodeInput == null)
        {
            Debug.LogError("roomCodeInput not assigned!");
            return;
        }

        NetworkConfig networkConfig = NetworkConfig.EnsureInstance();
        if (networkConfig == null)
        {
            Debug.LogError("NetworkConfig not found!");
            return;
        }

        string inputCode = NetworkConfig.NormalizeJoinCode(roomCodeInput.text);

        if (string.IsNullOrEmpty(inputCode))
        {
            Debug.LogWarning("Please enter a room code");
            return;
        }

        roomCodeInput.text = inputCode;
        GameData.IsHost = false;

        try
        {
            bool success = await networkConfig.JoinRelay(inputCode);
            if (success)
            {
                dinoSelectPanel.SetActive(true);
                DisableHostDino();
                Debug.Log("Successfully joined relay room!");
            }
            else
            {
                Debug.LogWarning("Failed to join relay room - invalid code or connection issue");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to join room: {ex.Message}");
        }

       
    }

    /// <summary>
    /// Disable the dino button selected by host.
    /// </summary>
    private void DisableHostDino()
    {
        if (LobbyManager.Instance == null)
        {
            Debug.LogWarning("LobbyManager not available yet");
            return;
        }

        int hostIndex = LobbyManager.Instance.HostDino.Value;

        if (hostIndex >= 0 && hostIndex < dinoButtons.Length)
        {
            dinoButtons[hostIndex].interactable = false;
        }
    }

    /// <summary>
    /// Updates multiplayer room UI with host/client dino selections.
    /// Called every frame while in multiplayer dino select screen.
    /// </summary>
    public void UpdateRoomUI()
    {
        if (LobbyManager.Instance == null)
            return;

        int hostDino = LobbyManager.Instance.HostDino.Value;
        int clientDino = LobbyManager.Instance.ClientDino.Value;

        if (hostDino != -1 && hostDino < dinoSprites.Length)
        {
            leftDinoImage.sprite = dinoSprites[hostDino];
            leftDinoImage.gameObject.SetActive(true);
        }

        if (clientDino != -1 && clientDino < dinoSprites.Length)
        {
            rightDinoImage.sprite = dinoSprites[clientDino];
            rightDinoImage.gameObject.SetActive(true);

            playButton.interactable = NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
        }
    }

    public void OpenWeaponPanel()
    {
        weaponPanel.SetActive(true);
    }
    public void CloseWeaponPanel()
    {
        weaponPanel.SetActive(false);
    }

    public void OpenGiftPanel()
    {
        giftPanel.SetActive(true);
    }
    public void CloseGiftPanel()
    {
        giftPanel.SetActive(false);
    }
    
    public void OpenRankingPanel()
    {
        rankingPanel.SetActive(true);
    }
    public void CloseRankingPanel()
    {
        rankingPanel.SetActive(false);
    }
}