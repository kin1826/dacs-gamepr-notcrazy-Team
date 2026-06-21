using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyRewardPanel : MonoBehaviour
{
    public static DailyRewardPanel Instance { get; private set; }

    private const string LastClaimKey = "DailyLastClaimDate";
    private const string StreakDayKey  = "DailyStreakDay";

    private static readonly int[] Rewards = { 1, 1, 1, 1, 2, 2, 5 };

    [Header("Cards (7 cái, theo thứ tự ngày 1→7)")]
    public Image[] dayCards;
    public Color colorToday    = Color.white;
    public Color colorFuture   = Color.gray;
    public Color colorClaimed  = Color.green;

    [Header("Slider")]
    public Slider progressSlider;

    [Header("Buttons")]
    public Button receiveButton;

    private int  currentDay;
    private bool canClaim;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    private void OnEnable() => Refresh();

    // ── PUBLIC API ────────────────────────────────────────

    // Gọi sau login — tự hiện nếu chưa nhận hôm nay
    public void TryAutoShow()
    {
        Evaluate();
        if (canClaim)
            gameObject.SetActive(true);
    }

    // Gọi từ openButton ngoài menu
    public void Open()
    {
        Evaluate();
        gameObject.SetActive(true);
    }

    // ── ACTIONS ───────────────────────────────────────────

    public void OnReceiveClick()
    {
        if (!canClaim) return;

        // Cộng gold local + sync lên API
        if (UserSession.Current != null)
        {
            int reward = Rewards[currentDay - 1];
            UserSession.Current.gold   += reward;
            SaveManager.Data.savedGold  = UserSession.Current.gold;
            SaveManager.Save();
            ApiManager.Instance?.UpdateGold(reward);
        }

        // Lưu streak
        PlayerPrefs.SetString(LastClaimKey, Today());
        PlayerPrefs.SetInt(StreakDayKey, currentDay);
        PlayerPrefs.Save();

        canClaim = false;
        RefreshReceiveButton();
        MainManager.Instance?.RefreshGold();
        gameObject.SetActive(false);
    }

    public void OnCloseClick()
    {
        gameObject.SetActive(false);
    }

    // ── INTERNAL ──────────────────────────────────────────

    private void Evaluate()
    {
        string lastClaim = PlayerPrefs.GetString(LastClaimKey, "");
        int    savedDay  = PlayerPrefs.GetInt(StreakDayKey, 0);

        if (lastClaim == Today())
        {
            currentDay = savedDay;
            canClaim   = false;
        }
        else if (lastClaim == Yesterday())
        {
            currentDay = savedDay >= 7 ? 1 : savedDay + 1;
            canClaim   = true;
        }
        else
        {
            // Lần đầu hoặc bỏ ngày → reset
            currentDay = 1;
            canClaim   = true;
        }
    }

    private void Refresh()
    {
        RefreshCards();
        RefreshSlider();
        RefreshReceiveButton();
    }

    private void RefreshCards()
    {
        for (int i = 0; i < dayCards.Length; i++)
        {
            if (dayCards[i] == null) continue;
            int day = i + 1;
            if (day < currentDay)       dayCards[i].color = colorClaimed;
            else if (day == currentDay) dayCards[i].color = colorToday;
            else                        dayCards[i].color = colorFuture;
        }
    }

    private void RefreshSlider()
    {
        if (progressSlider == null) return;
        progressSlider.value = (currentDay - 1) / 6f;
    }

    private void RefreshReceiveButton()
    {
        if (receiveButton == null) return;
        receiveButton.interactable = canClaim;
    }

    private static string Today()     => DateTime.Now.ToString("yyyy-MM-dd");
    private static string Yesterday() => DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
}
