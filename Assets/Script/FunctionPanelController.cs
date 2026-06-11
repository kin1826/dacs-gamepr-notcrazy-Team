using UnityEngine;
using UnityEngine.SceneManagement;

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
        bool isPausing = !pausePanel.activeSelf;
        pausePanel.SetActive(isPausing);
        Time.timeScale = isPausing ? 0f : 1f;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        LevelManager.Instance.LoadLevel(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        LevelManager.Instance.LoadLevel("Menu");
    }
}