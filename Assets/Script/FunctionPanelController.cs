using UnityEngine;

public class FunctionPanelController : MonoBehaviour
{
    public static FunctionPanelController Instance;

    public GameObject pausePanel;

    private void Awake()
    {
        Instance = this;
    }

    public void Pause()
    {
        pausePanel.SetActive(!pausePanel.activeSelf);
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
    }

    public void MainMenu()
    {
        LevelManager.Instance.LoadLevel("Menu");
    }
}