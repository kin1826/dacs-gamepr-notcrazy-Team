using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    [Tooltip("Ảnh hiển thị khi bật âm thanh")]
    public GameObject iconSound;
    [Tooltip("Ảnh hiển thị khi tắt âm thanh")]
    public GameObject iconMute;

    private void Start()
    {
        Refresh();
    }

    public void Toggle()
    {
        AudioManager.Instance?.ToggleMuteAll();
        Refresh();
    }

    private void Refresh()
    {
        bool muted = AudioManager.Instance != null && AudioManager.Instance.IsMuted;
        iconSound.SetActive(!muted);
        iconMute.SetActive(muted);

        
    }
}
