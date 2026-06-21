using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardPanel : MonoBehaviour
{
    [Header("Layout")]
    public Transform    content;
    public GameObject   rowPrefab;

    [Header("Colors")]
    public Color colorNormal = Color.white;
    public Color colorSelf   = new Color(1f, 0.85f, 0.2f);

    [Header("Status")]
    public TMP_Text statusText;

    private void OnEnable() => Load();

    private void Load()
    {
        SetStatus("Đang tải...");
        ClearRows();

        ApiManager.Instance?.GetLeaderboard(
            entries =>
            {
                SetStatus("");
                ClearRows();
                for (int i = 0; i < entries.Length; i++)
                    AddRow(i + 1, entries[i]);
            },
            err => SetStatus("Không tải được: " + err));
    }

    private void AddRow(int rank, LeaderboardEntry entry)
    {
        GameObject obj = Instantiate(rowPrefab, content);

        bool isSelf = UserSession.Current != null && entry.id == UserSession.Current.id;

        // Tìm các TMP_Text theo index: 0=rank, 1=name, 2=level
        var texts = obj.GetComponentsInChildren<TMP_Text>();
        if (texts.Length >= 3)
        {
            texts[0].text = $"#{rank}";
            texts[1].text = entry.name;
            texts[2].text = $"Level {entry.highestLevel}";
        }

        // Highlight dòng của chính mình
        foreach (var img in obj.GetComponentsInChildren<Image>())
            img.color = isSelf ? colorSelf : colorNormal;
    }

    private void ClearRows()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
    }

    private void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
    }

    public void OnCloseClick() => gameObject.SetActive(false);
}
