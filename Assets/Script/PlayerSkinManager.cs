using Unity.Netcode;
using UnityEngine;

public class PlayerSkinManager : NetworkBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Skins")]
    public RuntimeAnimatorController[] dinoSkins;

    private NetworkVariable<int> skinIndex = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        skinIndex.OnValueChanged += OnSkinChanged;

        if (IsOwner)
        {
            SubmitSkinServerRpc(GameData.SelectedDino);
        }

        ApplySkin(skinIndex.Value);
    }

    private void OnSkinChanged(int oldValue, int newValue)
    {
        ApplySkin(newValue);
    }

    private void ApplySkin(int index)
    {
        if (index < 0 || index >= dinoSkins.Length)
            return;

        animator.runtimeAnimatorController = dinoSkins[index];
    }

    [ServerRpc]
    private void SubmitSkinServerRpc(int index)
    {
        skinIndex.Value = index;
    }
}