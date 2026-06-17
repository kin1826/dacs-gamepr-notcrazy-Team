using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TrapButton : NetworkBehaviour
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
    private bool currentPressedState;

    private NetworkVariable<bool> isPressed = new NetworkVariable<bool>(false);

    private void Awake()
    {
        if (visual == null)
        {
            visual = transform;
        }

        visualStartLocalPosition = visual.localPosition;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isPressed.OnValueChanged += OnPressedChanged;
        UpdatePressedVisual(isPressed.Value);
    }

    public override void OnNetworkDespawn()
    {
        isPressed.OnValueChanged -= OnPressedChanged;
        base.OnNetworkDespawn();
    }

    private void OnPressedChanged(bool oldValue, bool newValue)
    {
        currentPressedState = newValue;
        UpdatePressedVisual(newValue);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!TryGetActivatorId(collision, out int activatorId))
        {
            return;
        }

        activatorsHolding.Add(activatorId);

        if (IsMultiplayer() && IsSpawned)
        {
            if (!IsServer)
            {
                PressServerRpc();
            }
            else
            {
                Press();
            }
            return;
        }

        Press();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!TryGetActivatorId(collision, out int activatorId))
        {
            return;
        }

        activatorsHolding.Remove(activatorId);

        if (GetEffectiveMode() != ButtonMode.Hold || activatorsHolding.Count != 0)
        {
            return;
        }

        if (IsMultiplayer() && IsSpawned)
        {
            if (!IsServer)
            {
                DeactivateServerRpc();
            }
            else
            {
                DeactivateActions();
            }
            return;
        }

        DeactivateActions();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PressServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer)
        {
            return;
        }

        Press();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeactivateServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer)
        {
            return;
        }

        DeactivateActions();
    }

    private void Press()
    {
        if (IsMultiplayer() && !IsServer) return;

        switch (GetEffectiveMode())
        {
            case ButtonMode.PressOnce:
                if (pressedOnce)
                {
                    return;
                }

                pressedOnce = true;
                isPressed.Value = true;
                ActivateActions();
                break;

            case ButtonMode.Toggle:
                toggledOn = !toggledOn;
                isPressed.Value = toggledOn;
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
                isPressed.Value = true;
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
            isPressed.Value = true;
        }
        else
        {
            isPressed.Value = toggledOn;
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
        if (IsMultiplayer() && !IsServer) return;

        if (GetEffectiveMode() != ButtonMode.Toggle)
        {
            isPressed.Value = false;
        }

        foreach (TrapAction action in actions)
        {
            if (action != null)
            {
                action.RequestDeactivate();
            }
        }
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

    private bool IsMultiplayer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
    }

    private void SetPressedVisual(bool isPressed)
    {
        if (visual == null)
        {
            return;
        }

        visual.localPosition = visualStartLocalPosition + (isPressed ? new Vector3(0f, pressedYOffset, 0f) : Vector3.zero);
    }

    private void UpdatePressedVisual(bool pressed)
    {
        currentPressedState = pressed;
        SetPressedVisual(pressed);
    }

    private IEnumerator PulsePressedVisual()
    {
        SetPressedVisual(true);
        yield return new WaitForSeconds(pulseDuration);
        SetPressedVisual(false);
    }
}
