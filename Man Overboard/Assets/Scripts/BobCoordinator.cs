using UnityEngine;

public class BobCoordinator : MonoBehaviour
{
    [Tooltip("The first three probes should be: front right, back left, back right.")]
    [SerializeField] BobWithWater[] probes;

    float desiredYOffset;
    Vector3 combinedWaterForce;

    [SerializeField] float baseWaterOffset;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody> ();
    }

    private void FixedUpdate()
    {
        desiredYOffset = 0f;
        combinedWaterForce = Vector3.zero;
        for(int i = 0; i<probes.Length; i++)
        {
            probes[i].UpdateBuoyancyParams();
            desiredYOffset += probes[i].offsetHeight;
            //combinedWaterForce += probes[i]
        }
        desiredYOffset /= probes.Length;
        Vector3 desiredUpDirection = Vector3.Cross(probes[0].desiredPosition - probes[2].desiredPosition, probes[0].desiredPosition - probes[1].desiredPosition);
        transform.rotation = Quaternion.LookRotation(transform.forward, desiredUpDirection);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, desiredYOffset+baseWaterOffset+ - transform.position.y, rb.linearVelocity.z);
    }

}
