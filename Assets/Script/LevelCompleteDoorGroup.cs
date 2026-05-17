using Unity.Netcode;
using UnityEngine;

public class LevelCompleteDoorGroup : MonoBehaviour
{
    [Header("Doors")]
    public GameObject primaryDoor;
    public GameObject secondaryDoor;

    [Header("Single Player")]
    public bool hideSecondaryDoorInSingle = true;

    private void Start()
    {
        bool isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;

        if (primaryDoor != null)
        {
            primaryDoor.SetActive(true);
        }

        if (secondaryDoor != null)
        {
            secondaryDoor.SetActive(isMultiplayer || !hideSecondaryDoorInSingle);
        }
    }
}
