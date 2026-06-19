using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopUpPanel : MonoBehaviour
{
    [Header("Package Buttons")]
    public Button btn10Gold;
    public Button btn100Gold;
    public Button btn500Gold;

    [Header("QR Sub-panel")]
    public GameObject qrSubPanel;
    public TMP_Text   codeText;
    public TMP_Text   priceText;
    public TMP_Text   goldAmountText;

    [Header("Status")]
    public TMP_Text statusText;
    public GameObject loadingIndicator;

    private void Awake()
    {
        btn10Gold.onClick.AddListener(()  => RequestTopUp(10));
        btn100Gold.onClick.AddListener(() => RequestTopUp(100));
        btn500Gold.onClick.AddListener(() => RequestTopUp(500));
    }

    private void OnEnable()
    {
        if (qrSubPanel) qrSubPanel.SetActive(false);
        SetStatus("");
    }

    private void RequestTopUp(int goldAmount)
    {
        if (!UserSession.IsLoggedIn)
        {
            SetStatus("Bạn cần đăng nhập để nạp xu.");
            return;
        }

        SetLoading(true);
        SetStatus("");

        ApiManager.Instance.CreateTopUp(
            new CreateTopUpRequest { goldAmount = goldAmount },
            OnSuccess,
            OnError
        );
    }

    private void OnSuccess(TopUpResponse res)
    {
        SetLoading(false);

        if (codeText)        codeText.text       = res.code;
        if (priceText)       priceText.text       = $"{res.price:N0} đ";
        if (goldAmountText)  goldAmountText.text  = $"+{res.goldAmount} xu";

        if (qrSubPanel) qrSubPanel.SetActive(true);
    }

    private void OnError(string err)
    {
        SetLoading(false);
        SetStatus("Lỗi: " + err);
    }

    public void OnCloseQrClick()
    {
        if (qrSubPanel) qrSubPanel.SetActive(false);
    }

    public void OnClosePanel()
    {
        gameObject.SetActive(false);
    }

    private void SetLoading(bool active)
    {
        if (loadingIndicator) loadingIndicator.SetActive(active);
        btn10Gold.interactable  = !active;
        btn100Gold.interactable = !active;
        btn500Gold.interactable = !active;
    }

    private void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
    }
}
