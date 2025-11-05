using UnityEngine;

public class BobCoordinator : MonoBehaviour
{
    [Tooltip("The first three probes should be: front right, back left, back right.")]
    [SerializeField] BobWithWater[] probes;

    float desiredYOffset;
    Vector3 combinedWaterForce;
    [SerializeField] float buoyancyForceModifier;

    [SerializeField] float rotationLerpSpeed;

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
            combinedWaterForce += probes[i].driftDirection;
        }
        desiredYOffset /= probes.Length;
        combinedWaterForce /= probes.Length;
        Vector3 desiredUpDirection = Vector3.Cross(probes[0].desiredPosition - probes[2].desiredPosition, probes[0].desiredPosition - probes[1].desiredPosition);
        Quaternion desiredRotation = Quaternion.LookRotation(transform.forward, desiredUpDirection);
        rb.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationLerpSpeed);
        //rb.AddForce(currentForceMultiplier*combinedWaterForce + Vector3.up * (desiredYOffset + baseWaterOffset - transform.position.y));
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, buoyancyForceModifier*(desiredYOffset+baseWaterOffset+ - transform.position.y), rb.linearVelocity.z);
    }

}
