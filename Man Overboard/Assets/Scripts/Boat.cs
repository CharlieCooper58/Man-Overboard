using Unity.Netcode;
using UnityEngine;

public class Boat : NetworkBehaviour
{
    Rigidbody rb;
    float forcePerRow;
    [ServerRpc]
    public void RowBoatServerRPC(Vector3 position)
    {

    }
}
