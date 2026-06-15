using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrapButton : NetworkBehaviour
{
    public enum ButtonMode { PressOnce, Toggle, Hold }

    [Header("Button")]
    public ButtonMode buttonMode = ButtonMode.Hold;
    public bool autoConvertHoldInSingle = true;
    public string[] extraActivatorTags = { "PushBlock", "ButtonWeight" };

    [Header("Visual")]
    public Transform visual;
    public float pressedYOffset = -0.05f;
    public float pulseDuration   = 0.12f;

    [Header("Actions")]
    public TrapAction[] actions;

    private readonly HashSet<int> activatorsHolding = new HashSet<int>();
    private bool pressedOnce;
    private bool toggledOn;
    private Vector3 visualStartLocalPosition;

    private NetworkVariable<bool> isPressed = new NetworkVariable<bool>(false);

    // ── LIFECYCLE ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (visual == null) visual = transform;
        visualStartLocalPosition = visual.localPosition;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isPressed.OnValueChanged += OnIsPressedChanged;
        SetPressedVisual(isPressed.Value);
    }

    public override void OnNetworkDespawn()
    {
        isPressed.OnValueChanged -= OnIsPressedChanged;
        base.OnNetworkDespawn();
    }

    private void OnIsPressedChanged(bool _, bool newVal) => SetPressedVisual(newVal);

    // ── TRIGGER ─────────────────────────── pattern giống TrapTrigger ─────────

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!TryGetActivatorId(collision, out int activatorId)) return;

        if (IsMultiplayer())
        {
            if (!IsServer)
            {
                // Client: báo server xử lý, visual ngay lập tức local
                SetPressedVisual(true);
                if (IsSpawned)
                    PlayerPressServerRpc(activatorId);
                else
                    foreach (var a in actions) a?.RequestActivate();
                return;
            }
            // Server: bỏ qua ghost của client player (đã xử lý qua RPC)
            if (IsClientGhost(collision)) return;
        }

        // Single player hoặc host player trên server
        // Add trả về false nếu đã có → tránh double-fire khi player có nhiều collider
        if (activatorsHolding.Add(activatorId))
            Press();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!TryGetActivatorId(collision, out int activatorId)) return;

        if (IsMultiplayer())
        {
            if (!IsServer)
            {
                // Always notify server so it can remove activatorId from tracking set.
                // Server decides whether to Deactivate based on mode.
                if (IsSpawned) PlayerExitServerRpc(activatorId);
                return;
            }
            if (IsClientGhost(collision)) return;
        }

        activatorsHolding.Remove(activatorId);
        if (GetEffectiveMode() == ButtonMode.Hold && activatorsHolding.Count == 0)
            DeactivateActions();
    }

    // ── SERVER RPCs ───────────────────────────────────────────────────────────

    [ServerRpc(RequireOwnership = false)]
    private void PlayerPressServerRpc(int activatorId)
    {
        // Only press if this activator wasn't already counted — prevents double-fire
        // when a client player has multiple colliders sending multiple RPCs.
        if (activatorsHolding.Add(activatorId))
            Press();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerExitServerRpc(int activatorId)
    {
        activatorsHolding.Remove(activatorId);
        // Only deactivate immediately on Hold — Toggle and PressOnce keep state until next press.
        if (GetEffectiveMode() == ButtonMode.Hold && activatorsHolding.Count == 0)
            DeactivateActions();
    }

    // ── PRESS LOGIC ───────────────────────────────────────────────────────────

    private void Press()
    {
        switch (GetEffectiveMode())
        {
            case ButtonMode.PressOnce:
                if (pressedOnce) return;
                pressedOnce = true;
                SetIsPressed(true);
                ActivateActions();
                break;

            case ButtonMode.Toggle:
                toggledOn = !toggledOn;
                SetIsPressed(toggledOn);
                if (toggledOn) { ActivateActions(); StartCoroutine(PulsePressedVisual()); }
                else             DeactivateActions();
                break;

            case ButtonMode.Hold:
                SetIsPressed(true);
                ActivateActions();
                break;
        }
    }

    private void SetIsPressed(bool value)
    {
        if (IsSpawned && IsServer) isPressed.Value = value;
        else SetPressedVisual(value);
    }

    private void ActivateActions()
    {
        foreach (var action in actions) action?.RequestActivate();
    }

    private void DeactivateActions()
    {
        SetIsPressed(false);
        foreach (var action in actions) action?.RequestDeactivate();
    }

    // ── HELPERS ───────────────────────────────────────────────────────────────

    private ButtonMode GetEffectiveMode()
    {
        bool single = NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient;
        if (single && autoConvertHoldInSingle && buttonMode == ButtonMode.Hold)
            return ButtonMode.PressOnce;
        return buttonMode;
    }

    private bool IsMultiplayer()
        => NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;

    private bool IsClientGhost(Collider2D collision)
    {
        var netObj = collision.GetComponentInParent<NetworkObject>();
        return netObj != null && netObj.OwnerClientId != NetworkManager.Singleton.LocalClientId;
    }

    private bool TryGetActivatorId(Collider2D collision, out int activatorId)
    {
        activatorId = 0;

        PlayerMovement player = collision.GetComponentInParent<PlayerMovement>();
        if (collision.CompareTag("Player") || player != null)
        {
            var netObj = collision.GetComponentInParent<NetworkObject>();
            activatorId = netObj != null ? netObj.OwnerClientId.GetHashCode() : collision.GetInstanceID();
            return true;
        }

        foreach (string tag in extraActivatorTags)
        {
            if (!string.IsNullOrWhiteSpace(tag) && string.Equals(
                    collision.gameObject.tag.Trim(), tag.Trim(), System.StringComparison.Ordinal))
            {
                activatorId = collision.attachedRigidbody != null
                    ? collision.attachedRigidbody.gameObject.GetInstanceID()
                    : collision.gameObject.GetInstanceID();
                return true;
            }
        }

        return false;
    }

    // ── VISUAL ────────────────────────────────────────────────────────────────

    private void SetPressedVisual(bool pressed)
    {
        if (visual == null) return;
        visual.localPosition = visualStartLocalPosition + (pressed ? new Vector3(0f, pressedYOffset, 0f) : Vector3.zero);
    }

    private IEnumerator PulsePressedVisual()
    {
        SetPressedVisual(true);
        yield return new WaitForSeconds(pulseDuration);
        SetPressedVisual(false);
    }
}
