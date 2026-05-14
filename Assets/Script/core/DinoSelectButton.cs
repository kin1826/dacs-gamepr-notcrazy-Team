using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages dino selection button. Routes through MainManager for proper scene loading.
/// Single-player will load Level_01, multiplayer will wait for network signal.
/// </summary>
public class DinoSelectButton : MonoBehaviour
{
    public int dinoIndex;

    public void SelectDino()
    {
        GameData.SelectedDino = dinoIndex;
        
        // Route through MainManager for proper flow control (single/multi)
        if (MainManager.Instance != null)
        {
            MainManager.Instance.SelectDino(dinoIndex);
        }
        else
        {
            Debug.LogError("MainManager not found! Cannot load scene properly.");
        }
    }
}