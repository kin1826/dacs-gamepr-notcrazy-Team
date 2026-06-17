using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages player skin/animator synchronization across network.
/// Syncs dino selection to all players using ServerRpc.
/// </summary>
public class PlayerSkinManager : NetworkBehaviour
{
    public static PlayerSkinManager LocalPlayer { get; private set; }

    [Header("References")]
    public Animator animator;

    [Header("Skins")]
    public RuntimeAnimatorController[] dinoSkins;

    private NetworkVariable<int> skinIndex = new NetworkVariable<int>(-1);
    private bool referencesReady;

    private void Start()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient)
        {
            LocalPlayer = this;
            SetSkin(GameData.SelectedDino);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!InitializeReferences())
        {
            return;
        }

        // Register callback for network updates
        skinIndex.OnValueChanged += OnSkinChanged;

        // Owner submits their skin selection
        if (IsOwner)
        {
            LocalPlayer = this;
            SetSkin(GameData.SelectedDino);
        }

        // Apply current skin
        ApplySkin(skinIndex.Value);
    }

    private bool InitializeReferences()
    {
        if (referencesReady)
        {
            return true;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("PlayerSkinManager: No Animator found! Assign one in inspector or attach to object with Animator.");
                return false;
            }
        }

        if (dinoSkins == null || dinoSkins.Length == 0)
        {
            Debug.LogError("PlayerSkinManager: No dino skins assigned in inspector!");
            return false;
        }

        referencesReady = true;
        return true;
    }

    public void SetSkin(int index)
    {
        if (!InitializeReferences())
        {
            return;
        }

        ApplySkin(index);

        if (!IsOwner)
        {
            return;
        }

        if (IsSpawned)
        {
            SubmitSkinServerRpc(index);
        }
    }

    /// <summary>
    /// Called when skin index changes on network.
    /// </summary>
    private void OnSkinChanged(int oldValue, int newValue)
    {
        ApplySkin(newValue);
    }

    /// <summary>
    /// Applies animator controller for given dino skin.
    /// Validates index is within bounds before applying.
    /// </summary>
    private void ApplySkin(int index)
    {
        if (index < 0 || index >= dinoSkins.Length)
        {
            if (index >= 0)  // Only log error if invalid (not initial -1 value)
            {
                Debug.LogError($"PlayerSkinManager: Invalid skin index {index}. Only {dinoSkins.Length} skins available. Using default skin.");
            }
            
            // Apply default skin (first one)
            if (dinoSkins.Length > 0)
            {
                animator.runtimeAnimatorController = dinoSkins[0];
            }
            return;
        }

        animator.runtimeAnimatorController = dinoSkins[index];
        Debug.Log($"Applied skin: {index}");
    }

    /// <summary>
    /// Server RPC to submit player's skin selection.
    /// Validates and applies to all clients.
    /// </summary>
    [ServerRpc]
    private void SubmitSkinServerRpc(int index)
    {
        if (index < 0 || index >= dinoSkins.Length)
        {
            Debug.LogError($"Invalid skin index submitted: {index}. Defaulting to 0.");
            index = 0;
        }

        skinIndex.Value = index;
    }

    private void OnDestroy()
    {
        if (LocalPlayer == this)
            LocalPlayer = null;
    }

    public override void OnNetworkDespawn()
    {
        if (LocalPlayer == this)
            LocalPlayer = null;

        if (skinIndex != null)
            skinIndex.OnValueChanged -= OnSkinChanged;

        base.OnNetworkDespawn();
    }
}
