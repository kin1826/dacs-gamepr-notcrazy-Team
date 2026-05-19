using Unity.Netcode;
using UnityEngine;

public class LevelCompleteDoorGroup : NetworkBehaviour
{
    [Header("Doors")]
    public GameObject primaryDoor;
    public GameObject secondaryDoor;

    [Header("Single Player")]
    public bool hideSecondaryDoorInSingle = true;

    private NetworkVariable<bool> secondaryDoorVisible = new NetworkVariable<bool>(false);

    private void Start()
    {
        if (!IsSpawned)
        {
            if (primaryDoor != null)
            {
                primaryDoor.SetActive(true);
            }

            bool isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
            UpdateSecondaryDoor(isMultiplayer || !hideSecondaryDoorInSingle);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (primaryDoor != null)
        {
            primaryDoor.SetActive(true);
        }

        secondaryDoorVisible.OnValueChanged += OnSecondaryDoorVisibleChanged;

        if (IsServer)
        {
            bool isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
            secondaryDoorVisible.Value = isMultiplayer || !hideSecondaryDoorInSingle;
        }

        UpdateSecondaryDoor(secondaryDoorVisible.Value);
    }

    private void OnSecondaryDoorVisibleChanged(bool oldValue, bool newValue)
    {
        UpdateSecondaryDoor(newValue);
    }

    private void UpdateSecondaryDoor(bool isVisible)
    {
        if (secondaryDoor != null)
        {
            secondaryDoor.SetActive(isVisible);
        }
    }

    public override void OnNetworkDespawn()
    {
        secondaryDoorVisible.OnValueChanged -= OnSecondaryDoorVisibleChanged;
        base.OnNetworkDespawn();
    }
}
