using UnityEngine;

/// <summary>
/// Shows or hides a GameObject according to the current game mode.
/// Attach this to any scene object, then choose the required mode in the Inspector.
/// </summary>
public class GameModeVisibility : MonoBehaviour
{
    public enum VisibilityMode
    {
        Always,
        SinglePlayerOnly,
        MultiplayerOnly
    }

    [Header("Visibility")]
    [SerializeField] private VisibilityMode visibilityMode = VisibilityMode.Always;

    [Tooltip("Leave empty to show/hide the GameObject this component is attached to.")]
    [SerializeField] private GameObject targetObject;

    private GameObject Target => targetObject != null ? targetObject : gameObject;

    private void Awake()
    {
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    /// <summary>
    /// Re-check the current mode. Call this after changing GameData.IsMultiplayer
    /// while remaining in the same scene.
    /// </summary>
    public void Refresh()
    {
        bool shouldBeVisible = visibilityMode switch
        {
            VisibilityMode.SinglePlayerOnly => !GameData.IsMultiplayer,
            VisibilityMode.MultiplayerOnly => GameData.IsMultiplayer,
            _ => true
        };

        Target.SetActive(shouldBeVisible);
    }
}
