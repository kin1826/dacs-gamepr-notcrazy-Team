using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkipChoicePanel : MonoBehaviour
{
    private const int GoldCost    = 20;
    private const int MaxAdViews  = 3;
    private const string PrefKey  = "AdViewCount";
    private const string DateKey  = "AdLastResetDate";

    [Header("Video button")]
    public Button   videoButton;
    public TMP_Text videoTurnsText;

    [Header("Coin button")]
    public Button   coinButton;
    public TMP_Text coinStatusText;

    [Header("Refs")]
    public GameObject adsPanel;
    public GameObject respawnPanel;

    private void OnEnable()
    {
        RefreshDailyReset();
        RefreshUI();
    }

    // ── UI ────────────────────────────────────────────────

    private void RefreshUI()
    {
        int views = GetAdViewsRemaining();
        videoButton.interactable = views > 0;
        videoTurnsText.text      = $"{views} turns";

        int gold    = UserSession.Current?.gold ?? 0;
        bool enough = gold >= GoldCost;
        coinButton.interactable = enough;
        coinStatusText.text     = enough ? $"{GoldCost} gold" : "Not Enaught";
    }

    // ── ACTIONS ───────────────────────────────────────────

    public void OnVideoClick()
    {
        UseAdView();
        gameObject.SetActive(false);
        adsPanel.SetActive(true);
    }

    public void OnCoinClick()
    {
        if (!UserSession.SpendGold(GoldCost)) return;
        // TODO: sync gold lên API khi về menu (gọi ApiManager.UpdateGold với delta = -GoldCost)
        MainManager.Instance?.RefreshGold();
        gameObject.SetActive(false);
        LevelManager.Instance?.OnAdsFinished();
    }

    public void OnCloseClick()
    {
        gameObject.SetActive(false);
        // respawnPanel?.SetActive(true);
    }

    // ── AD VIEW TRACKING (local) ──────────────────────────

    private void RefreshDailyReset()
    {
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        if (PlayerPrefs.GetString(DateKey, "") != today)
        {
            PlayerPrefs.SetInt(PrefKey, 0);
            PlayerPrefs.SetString(DateKey, today);
            PlayerPrefs.Save();
        }
    }

    private int GetAdViewsRemaining()
    {
        return MaxAdViews - PlayerPrefs.GetInt(PrefKey, 0);
    }

    private void UseAdView()
    {
        int used = PlayerPrefs.GetInt(PrefKey, 0) + 1;
        PlayerPrefs.SetInt(PrefKey, used);
        PlayerPrefs.Save();
    }
}
