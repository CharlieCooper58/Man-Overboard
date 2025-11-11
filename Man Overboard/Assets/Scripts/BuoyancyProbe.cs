using UnityEngine;

public class BuoyancyProbe : MonoBehaviour
{
    public float offsetHeight;
    public Vector3 driftDirection;

    public Vector3 desiredPosition;

    private void Start()
    {
        desiredPosition = transform.position;
    }

    public void UpdateBuoyancyParams()
    {
        Vector3 waveparams = Water.StaticGetWaterBehaviorAtPoint(transform.position);
        offsetHeight = waveparams.x;
        driftDirection.x = waveparams.y;
        driftDirection.z = waveparams.z;
        desiredPosition = new Vector3(transform.position.x, offsetHeight, transform.position.z);
    }
}
