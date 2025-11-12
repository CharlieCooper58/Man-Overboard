using Unity.VisualScripting;
using UnityEngine;

public class BuoyancyComponent : MonoBehaviour
{
    [Tooltip("The first three probes should be: front right, back left, back right.")]
    Vector3 combinedWaterForce;
    [SerializeField] float buoyancyForceModifier;

    [SerializeField] float rotationLerpSpeed;

    [SerializeField] float baseWaterOffset;

    VoxelizedMesh voxels;

    Rigidbody rb;
    float voxelVol;

    private void Awake()
    {
        rb = GetComponent<Rigidbody> ();
        voxels = GetComponent<VoxelizedMesh> ();
    }

    private void Start()
    {
        voxelVol = voxels.vol;
    }

    private void FixedUpdate()
    {
        for(int i = 0; i<voxels.keysArray.Length; i++)
        {
            Vector3 waterParams = Water.StaticGetWaterBehaviorAtPoint(voxels.PositionFromKey(voxels.keysArray[i]));
            int voxelsBelowWaterline = 0 ;
            Vector2Int key = voxels.keysArray[i];
            Vector3 forcePos = voxels.PointToPosition(new Vector3Int(key.x, voxels.coordinateHeights[key][0], key.y));
            for (int j = 0; j < voxels.coordinateHeights[voxels.keysArray[i]].Count; j++)
            {
                // To do: this breaks down given high ship rotations, as the column of voxels no longer accurately shares an xz coordinate
                float worldSpaceVoxelHeight = voxels.PointToPosition(new Vector3Int(key.x, voxels.coordinateHeights[key][j], key.y)).y;

                if (worldSpaceVoxelHeight <= waterParams.x)
                {
                    voxelsBelowWaterline++;
                }
                else
                {
                    // To do: add handling for if a voxel is half submerged?
                    break;
                }
                rb.AddForceAtPosition(voxelVol * voxelsBelowWaterline * Vector3.up, forcePos);
            }
        }

        //Vector3 desiredUpDirection = Vector3.Cross(probes[0].desiredPosition - probes[2].desiredPosition, probes[0].desiredPosition - probes[1].desiredPosition);
        //Quaternion desiredRotation = Quaternion.LookRotation(transform.forward, desiredUpDirection);
        //rb.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationLerpSpeed);
        //rb.AddForce(Vector3.up * (desiredYOffset + baseWaterOffset - transform.position.y));
        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, buoyancyForceModifier*(desiredYOffset+baseWaterOffset+ - transform.position.y), rb.linearVelocity.z);
    }

    private float CalculateForceFromProbe(BuoyancyProbe probe)
    {
        return 0;
    }

}
