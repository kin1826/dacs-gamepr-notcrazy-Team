using UnityEngine;
using UnityEngine.InputSystem;

public class ClickSFX : MonoBehaviour
{
    public int sfxIndex = 0;

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            AudioManager.Instance?.PlaySFX(sfxIndex);

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            AudioManager.Instance?.PlaySFX(sfxIndex);
    }
}
