using UnityEngine;

/// <summary>
/// Scene-level modifier that swaps left and right movement for every local player.
/// Add this component to a manager object only in scenes that need the effect.
/// </summary>
public class ReverseHorizontalControls : MonoBehaviour
{
    [Tooltip("Enable to swap left and right movement while this object is active.")]
    [SerializeField] private bool reverseControls = true;

    private void OnEnable()
    {
        PlayerMovement.SetHorizontalInputInverted(reverseControls);
    }

    private void OnDisable()
    {
        // This component owns the scene-wide modifier, so leaving its scene
        // always restores normal controls for the next scene.
        PlayerMovement.SetHorizontalInputInverted(false);
    }
}
