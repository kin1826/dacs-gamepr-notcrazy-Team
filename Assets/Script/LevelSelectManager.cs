using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectManager :
    MonoBehaviour
{
    public static LevelSelectManager Instance;

    public GameObject
        levelButtonPrefab;

    public Transform content;

    public int totalLevel = 20;

    void Awake()
    {
        Instance = this;
    }

    public void GenerateAgain()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            DestroyImmediate(content.GetChild(i).gameObject);

        GenerateLevels();
    }

    void GenerateLevels()
    {
        if (GameData.IsMultiplayer)
        {
            totalLevel = LobbyManager.Instance.MinLevelLobby.Value;
            Debug.Log("Multiplayer mode: Limiting levels to " + totalLevel);
        }

        for(
            int i=1;
            i<=totalLevel;
            i++
        )
        {
            GameObject obj =
                Instantiate(
                    levelButtonPrefab,
                    content
                );

            int level = i;

            TMP_Text txt =
                obj.GetComponentInChildren
                <TMP_Text>();

            txt.text =
                level.ToString();

            Button btn =
                obj.GetComponent<Button>();

            btn.interactable =
                level <=
                SaveManager
                .Data
                .highestLevel;

            btn.onClick
            .AddListener(
                ()=>SelectLevel(
                    level
                )
            );
        }
    }

    public void SelectLevel(
        int level
    )
    {
        GameData.SelectedLevel =
            level;

        MainManager.Instance.StartGame();
    }
}