using UnityEngine;
using UnityEngine.EventSystems;

public class MobileControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum ControlAction
    {
        MoveLeft,
        MoveRight,
        Jump
    }

    [SerializeField] private ControlAction action;

    public void OnPointerDown(PointerEventData eventData)
    {
        PlayerMovement player = PlayerMovement.LocalPlayer;
        if (player == null || !player.IsOwner) return;

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
        StopMoveIfNeeded();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopMoveIfNeeded();
    }

    private void StopMoveIfNeeded()
    {
        if (action == ControlAction.Jump) return;

        PlayerMovement player = PlayerMovement.LocalPlayer;
        if (player == null || !player.IsOwner) return;

        player.MoveStop();
    }
}
