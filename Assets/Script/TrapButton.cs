using System.Collections.Generic;
using System.Collections;
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
    public string[] extraActivatorTags = { "PushBlock", "ButtonWeight" };

    [Header("Visual")]
    public Transform visual;
    public float pressedYOffset = -0.05f;
    public float pulseDuration = 0.12f;

    [Header("Actions")]
    public TrapAction[] actions;

    private readonly HashSet<int> activatorsHolding = new HashSet<int>();
    private bool pressedOnce;
    private bool toggledOn;
    private Vector3 visualStartLocalPosition;

    private void Awake()
    {
        if (visual == null)
        {
            visual = transform;
        }

        visualStartLocalPosition = visual.localPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!TryGetActivatorId(collision, out int activatorId))
        {
            return;
        }

        if (!CanRunTrapLogic())
        {
            return;
        }

        activatorsHolding.Add(activatorId);
        Press();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!TryGetActivatorId(collision, out int activatorId))
        {
            return;
        }

        if (!CanRunTrapLogic())
        {
            return;
        }

        activatorsHolding.Remove(activatorId);

        if (GetEffectiveMode() == ButtonMode.Hold && activatorsHolding.Count == 0)
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
                StartCoroutine(PulsePressedVisual());
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
        if (GetEffectiveMode() != ButtonMode.Toggle)
        {
            SetPressedVisual(true);
        }

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
        if (GetEffectiveMode() != ButtonMode.Toggle)
        {
            SetPressedVisual(false);
        }

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

    private bool TryGetActivatorId(Collider2D collision, out int activatorId)
    {
        activatorId = 0;

        PlayerMovement player = collision.GetComponentInParent<PlayerMovement>();
        if (collision.CompareTag("Player") || player != null)
        {
            NetworkObject networkObject = collision.GetComponentInParent<NetworkObject>();
            activatorId = networkObject != null
                ? networkObject.OwnerClientId.GetHashCode()
                : collision.GetInstanceID();
            return true;
        }

        foreach (string tagName in extraActivatorTags)
        {
            if (!string.IsNullOrWhiteSpace(tagName) && HasTag(collision.gameObject, tagName))
            {
                activatorId = collision.attachedRigidbody != null
                    ? collision.attachedRigidbody.gameObject.GetInstanceID()
                    : collision.gameObject.GetInstanceID();
                return true;
            }
        }

        return false;
    }

    private bool HasTag(GameObject target, string tagName)
    {
        return string.Equals(
            target.tag.Trim(),
            tagName.Trim(),
            System.StringComparison.Ordinal
        );
    }

    private void SetPressedVisual(bool isPressed)
    {
        if (visual == null)
        {
            return;
        }

        visual.localPosition = visualStartLocalPosition + (isPressed ? new Vector3(0f, pressedYOffset, 0f) : Vector3.zero);
    }

    private IEnumerator PulsePressedVisual()
    {
        SetPressedVisual(true);
        yield return new WaitForSeconds(pulseDuration);
        SetPressedVisual(false);
    }
}
