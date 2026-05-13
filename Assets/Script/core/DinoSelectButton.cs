using UnityEngine;
using UnityEngine.SceneManagement;

public class DinoSelectButton : MonoBehaviour
{
    public int dinoIndex;

    public void SelectDino()
    {
        GameData.SelectedDino = dinoIndex;

        SceneManager.LoadScene("Level_01");
    }
}