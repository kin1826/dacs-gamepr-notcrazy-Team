using UnityEngine;

public class ClickSFX : MonoBehaviour
{
    public int sfxIndex = 0;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AudioManager.Instance?.PlaySFX(sfxIndex);
        }
    }
}
