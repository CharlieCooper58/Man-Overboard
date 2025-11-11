using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class VoxelizedMesh : MonoBehaviour
{
    private void Start()
    {
        VoxelizeMesh(GetComponent<MeshFilter>());
    }
    public List<Vector3Int> gridPoints;
    Dictionary<Vector2Int, float> coordinateHeights;
    public static Vector3 halfsize = new Vector3(0.25f, .1f, 0.25f);
    Vector3 cellSize { get
        {
            return halfsize * 2;
        } 
    }

    public Vector3 localOrigin;
    public Vector3 PointToPosition(Vector3Int point)
    {
        return transform.TransformPoint(localOrigin + halfsize + new Vector3(point.x * cellSize.x, point.y * cellSize.y, point.z * cellSize.z));
    }

    // Extremely basic function for voxelizing mesh.  Should correctly work on a mesh with a concave meshcollider as a quick and dirty editor solution
    // The object must have ONLY the mesh collider attached and should be in a void away from any other colliders
    // Like I said this is very quick and dirty
    public static void VoxelizeMesh(MeshFilter meshFilter)
    {
        if(!meshFilter.TryGetComponent(out MeshCollider meshCollider))
        {
            meshCollider = meshFilter.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.mesh;
        }
        if(!meshFilter.TryGetComponent(out VoxelizedMesh voxelizedMesh))
        {
            voxelizedMesh = meshFilter.AddComponent<VoxelizedMesh>();
        }

        Bounds bounds = meshCollider.bounds;
        Vector3 minExtents = bounds.center - bounds.extents;
        Vector3Int cellCount = new Vector3Int(Mathf.CeilToInt(bounds.extents.x / halfsize.x), Mathf.CeilToInt(bounds.extents.y / halfsize.y), Mathf.CeilToInt(bounds.extents.z / halfsize.z));
        voxelizedMesh.gridPoints.Clear();
        voxelizedMesh.localOrigin = voxelizedMesh.transform.InverseTransformPoint(minExtents);

        for(int x = 0; x < cellCount.x; x++)
        {
            for(int z = 0; z < cellCount.z; z++)
            {
                for(int y = 0; y < cellCount.y; y++)
                {
                    Vector3 pos = voxelizedMesh.PointToPosition(new Vector3Int(x, y, z));
                    if(Physics.CheckBox(pos, halfsize))
                    {
                        voxelizedMesh.gridPoints.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }
}

[CustomEditor(typeof(VoxelizedMesh))]
public class VoxelizedMeshEditor : Editor
{
    void OnSceneGUI()
    {
        VoxelizedMesh voxelizedMesh = target as VoxelizedMesh;

        Handles.color = Color.green;

        foreach (Vector3Int gridPoint in voxelizedMesh.gridPoints)
        {
            Vector3 worldPos = voxelizedMesh.PointToPosition(gridPoint);
            Handles.DrawWireCube(worldPos, VoxelizedMesh.halfsize * 2f);
        }

        Handles.color = Color.red;
        if (voxelizedMesh.TryGetComponent(out MeshCollider meshCollider))
        {
            Bounds bounds = meshCollider.bounds;
            Handles.DrawWireCube(bounds.center, bounds.extents * 2);
        }
    }
}
