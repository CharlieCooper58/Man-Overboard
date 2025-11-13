using Unity.VisualScripting;
using UnityEngine;

public class BuoyancyComponent : MonoBehaviour
{
    [Tooltip("The first three probes should be: front right, back left, back right.")]
    Vector3 combinedWaterForce;
    [SerializeField] float buoyancyForceModifier;

    [SerializeField] float rotationLerpSpeed;

    [SerializeField] float baseWaterOffset;

    [SerializeField] float futurePredictionSteps;

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
        rb.centerOfMass -= 0.5f * Vector3.up;
    }

    private void FixedUpdate()
    {
        for(int i = 0; i<voxels.keysArray.Length; i++)
        {
            Vector3 waterParams = Water.StaticGetWaterBehaviorAtPoint(voxels.PositionFromKey(voxels.keysArray[i]));
            int voxelsBelowWaterline = 0 ;
            Vector2Int key = voxels.keysArray[i];
            Vector3 forcePos = voxels.PointToPosition(new Vector3Int(key.x, voxels.coordinateHeights[key][0], key.y));
            //forcePos.y = transform.position.y;
            for (int j = 0; j < voxels.coordinateHeights[voxels.keysArray[i]].Count; j++)
            {
                // To do: this breaks down given high ship rotations, as the column of voxels no longer accurately shares an xz coordinate
                float worldSpaceVoxelHeight = (voxels.PointToPosition(new Vector3Int(key.x, voxels.coordinateHeights[key][j], key.y))+rb.linearVelocity*futurePredictionSteps*Time.deltaTime).y;

                if (worldSpaceVoxelHeight <= waterParams.x)
                {
                    voxelsBelowWaterline++;
                }
                else
                {
                    // To do: add handling for if a voxel is half submerged?
                    break;
                }
            }
            // Density of water is 1000 kg/m^3
            rb.AddForceAtPosition(1000*voxelVol * voxelsBelowWaterline * Vector3.up, forcePos);
        }
    }

}
