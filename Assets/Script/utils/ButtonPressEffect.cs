using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Shrinks a child frame RectTransform's padding to 0 on press, restores on release.
/// Attach to the Button. Assign the inner frame's RectTransform to "frameRect".
/// </summary>
public class ButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Tooltip("RectTransform của frame bên trong button")]
    public RectTransform frameRect;

    private Vector2 defaultOffsetMin;
    private Vector2 defaultOffsetMax;

    private void Start()
    {
        if (frameRect == null)
        {
            Debug.LogError("ButtonPressEffect: frameRect chưa được gán!", this);
            return;
        }

        defaultOffsetMin = frameRect.offsetMin;
        defaultOffsetMax = frameRect.offsetMax;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (frameRect == null) return;

        frameRect.offsetMin = Vector2.zero;
        frameRect.offsetMax = Vector2.zero;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Restore();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Restore();
    }

    private void Restore()
    {
        if (frameRect == null) return;

        frameRect.offsetMin = defaultOffsetMin;
        frameRect.offsetMax = defaultOffsetMax;
    }
}
