using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
public class PlayerManager : NetworkBehaviour
{
    bool isRowing;
    PlayerCamera playerCamera;
    InputHandler input;

    private void Awake()
    {
        playerCamera = GetComponent<PlayerCamera>();
        input = GetComponent<InputHandler>();
        playerCamera.Initialize();
        isRowing = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            playerCamera.Initialize();
        }
        else
        {
            playerCamera.DeactivateCamera();
            input.DeactivateInput();
        }
    }

    private void LateUpdate()
    {
        //if (IsLocalPlayer)
        //{
            playerCamera.HandleCameraMovement(input.lookVector, isRowing);
        //}
    }
}
