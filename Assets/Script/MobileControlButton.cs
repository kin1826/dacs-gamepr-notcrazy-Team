using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;

public class MobileControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum ControlAction
    {
        MoveLeft,
        MoveRight,
        Jump
    }

    [SerializeField] private ControlAction action;
    [SerializeField] private bool debugLogs = false;
    private int activePointerId = int.MinValue;
    private bool isPressed = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (debugLogs) Debug.Log($"[MobileControlButton] OnPointerDown action={action} pointerId={eventData.pointerId} pos={eventData.position}");
        // record active pointer so Up/Exit only respond to the same touch
        activePointerId = eventData.pointerId;
        isPressed = true;

        PlayerMovement player = PlayerMovement.LocalPlayer;
        if (player == null) return;
        bool hasLocalControl = player.IsOwner || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient;
        if (!hasLocalControl) return;

        switch (action)
        {
            case ControlAction.MoveLeft:
                player.MoveLeftDown();
                break;
            case ControlAction.MoveRight:
                player.MoveRightDown();
                break;
            case ControlAction.Jump:
                player.JumpPressed();
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (debugLogs) Debug.Log($"[MobileControlButton] OnPointerUp action={action} pointerId={eventData.pointerId} pos={eventData.position} activePointer={activePointerId}");
        if (eventData.pointerId != activePointerId)
        {
            if (debugLogs) Debug.Log($"[MobileControlButton] Ignored OnPointerUp from different pointerId={eventData.pointerId}");
            return;
        }

        StopMoveIfNeeded();
        ResetPointerState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (debugLogs) Debug.Log($"[MobileControlButton] OnPointerExit action={action} pointerId={eventData.pointerId} pos={eventData.position} activePointer={activePointerId}");
        if (eventData.pointerId != activePointerId)
        {
            if (debugLogs) Debug.Log($"[MobileControlButton] Ignored OnPointerExit from different pointerId={eventData.pointerId}");
            return;
        }

        StopMoveIfNeeded();
        ResetPointerState();
    }

    private void StopMoveIfNeeded()
    {
        if (action == ControlAction.Jump) return;

        PlayerMovement player = PlayerMovement.LocalPlayer;
        if (player == null) return;
        bool hasLocalControl = player.IsOwner || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient;
        if (!hasLocalControl) return;

        player.MoveStop();
    }

    private void ResetPointerState()
    {
        isPressed = false;
        activePointerId = int.MinValue;
    }

    private void OnDisable()
    {
        // ensure movement stops when object disabled
        if (isPressed)
        {
            StopMoveIfNeeded();
            ResetPointerState();
        }
    }
}
