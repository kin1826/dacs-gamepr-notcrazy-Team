using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrapButton : MonoBehaviour
{
    public enum ButtonMode
    {
        PressOnce,
        Toggle,
        Hold
    }

    [Header("Button")]
    public ButtonMode buttonMode = ButtonMode.Hold;
    public bool autoConvertHoldInSingle = true;

    [Header("Actions")]
    public TrapAction[] actions;

    private readonly HashSet<ulong> playersHolding = new HashSet<ulong>();
    private bool pressedOnce;
    private bool toggledOn;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsPlayer(collision, out ulong clientId))
        {
            return;
        }

        if (!CanRunTrapLogic())
        {
            return;
        }

        playersHolding.Add(clientId);
        Press();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsPlayer(collision, out ulong clientId))
        {
            return;
        }

        if (!CanRunTrapLogic())
        {
            return;
        }

        playersHolding.Remove(clientId);

        if (GetEffectiveMode() == ButtonMode.Hold && playersHolding.Count == 0)
        {
            DeactivateActions();
        }
    }

    private void Press()
    {
        switch (GetEffectiveMode())
        {
            case ButtonMode.PressOnce:
                if (pressedOnce)
                {
                    return;
                }

                pressedOnce = true;
                ActivateActions();
                break;

            case ButtonMode.Toggle:
                toggledOn = !toggledOn;
                if (toggledOn)
                {
                    ActivateActions();
                }
                else
                {
                    DeactivateActions();
                }
                break;

            case ButtonMode.Hold:
                ActivateActions();
                break;
        }
    }

    private ButtonMode GetEffectiveMode()
    {
        bool isSinglePlayer = NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient;
        if (isSinglePlayer && autoConvertHoldInSingle && buttonMode == ButtonMode.Hold)
        {
            return ButtonMode.PressOnce;
        }

        return buttonMode;
    }

    private void ActivateActions()
    {
        foreach (TrapAction action in actions)
        {
            if (action != null)
            {
                action.RequestActivate();
            }
        }
    }

    private void DeactivateActions()
    {
        foreach (TrapAction action in actions)
        {
            if (action != null)
            {
                action.RequestDeactivate();
            }
        }
    }

    private bool CanRunTrapLogic()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            return NetworkManager.Singleton.IsServer;
        }

        return true;
    }

    private bool IsPlayer(Collider2D collision, out ulong clientId)
    {
        clientId = 0;

        if (!collision.CompareTag("Player") && collision.GetComponentInParent<PlayerMovement>() == null)
        {
            return false;
        }

        NetworkObject networkObject = collision.GetComponentInParent<NetworkObject>();
        if (networkObject != null)
        {
            clientId = networkObject.OwnerClientId;
        }

        return true;
    }
}
