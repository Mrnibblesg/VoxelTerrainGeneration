using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Voxel[,,] voxels;

    public World parent;

    //Store refs to neighbors used when you request a new mesh
    public Chunk[] neighbors;
    //up, down, left, right, forward, back

    public static ProfilerMarker s_ChunkGen = new(ProfilerCategory.Render, "Chunk.RegenerateMesh"); //Profiling

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    //Use to avoid race conditions related to mesh requests & job completion time
    private long lastMeshUpdateTime;

    public void Initialize(World parent)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = parent.vertexColorMaterial;

        neighbors = new Chunk[6];

        this.parent = parent;

        voxels = new Voxel[parent.chunkSize, parent.chunkHeight, parent.chunkSize];
    }

    /// <summary>
    /// Generates a mesh for this chunk
    /// </summary>
    public void RegenerateMesh()
    {
        updateNeighborChunks();
        ChunkMeshGenerator.RequestNewMesh(this);
    }
    public void ApplyNewMesh(Mesh m, long requestTime)
    {
        //Ignore if the mesh was requested earlier than the current one was (outdated mesh)
        if (requestTime < lastMeshUpdateTime)
        {
            return;
        }
        meshFilter.mesh = m;
        meshCollider.sharedMesh = m;
        lastMeshUpdateTime = requestTime;
    }

    /// <summary>
    /// Copies the voxel at the global position in this chunk.
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public Voxel? GetVoxel(Vector3 vec)
    {
        //Get from some coordinate within the chunk to the appropriate voxel coords.
        vec *= parent.resolution;
        Vector3Int pos = new Vector3Int(
            Mathf.FloorToInt(vec.x),
            Mathf.FloorToInt(vec.y),
            Mathf.FloorToInt(vec.z)
        );

        bool outside = VoxelOutOfBounds(pos.x, pos.y, pos.z);

        return outside ? null : Voxel.Clone(voxels[pos.x, pos.y, pos.z]);
    }

    /// <summary>
    /// Sets the voxel at the world-space position in this chunk.
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="voxel"></param>
    /// <returns> Whether the voxel was set. </returns>
    public bool SetVoxel(Vector3 vec, Voxel voxel)
    {
        //Scale the world-space coordinate to voxel-coordinate space
        vec *= parent.resolution;
        Vector3Int pos = new Vector3Int(
            Mathf.FloorToInt(vec.x),
            Mathf.FloorToInt(vec.y),
            Mathf.FloorToInt(vec.z)
        );

        bool outside = VoxelOutOfBounds(pos.x, pos.y, pos.z);
        if (outside) { return false; }

        voxels[pos.x, pos.y, pos.z] = Voxel.Clone(voxel);

        // Update the mesh
        // Brute force for now
        UpdateNeighbors(pos.x, pos.y, pos.z);
        RegenerateMesh();

        return true;
    }
    public bool SetVoxels(List<Vector3> vec, List<Voxel> voxel)
    {
        if (vec.Count != voxel.Count)
        {
            return false;
        }

        for (int i = 0; i < vec.Count; i++)
        {
            //Scale the world-space coordinate to voxel-coordinate space
            vec[i] *= parent.resolution;
            Vector3Int pos = new Vector3Int(
                Mathf.FloorToInt(vec[i].x),
                Mathf.FloorToInt(vec[i].y),
                Mathf.FloorToInt(vec[i].z)
            );

            bool outside = VoxelOutOfBounds(pos.x, pos.y, pos.z);
            if (outside) { continue; }

            if (voxel[i].type == VoxelType.AIR)
            {
                if (voxels[pos.x, pos.y, pos.z].type != VoxelType.AIR)
                {
                    voxels[pos.x, pos.y, pos.z] = Voxel.Clone(voxel[i]);
                }
            }
            else
            {
                if (voxels[pos.x, pos.y, pos.z].type == VoxelType.AIR)
                {
                    voxels[pos.x, pos.y, pos.z] = Voxel.Clone(voxel[i]);
                }
            }
        }

        // Update the mesh
        // Brute force for now
        RegenerateMesh();

        return true;
    }

    /// <summary>
    /// Update the state of voxels adjacent to the given position.
    /// Regenerates mesh.
    /// </summary>
    /// <remarks>
    /// The origin of the updates must be from within the chunk.
    /// </remarks>
    private void UpdateNeighbors(int x, int y, int z)
    {
        if (VoxelOutOfBounds(x,y,z)) { return; }

        //Make sure when adding to this function that the things you add DO NOT trigger
        //more updates, or the updates could cascade forever.
        void updateList(Chunk c, int x, int y, int z)
        {

            if (c != this)
            {
                c.RegenerateMesh();
            }
        }

        //Use the proper chunk to update the neighbor voxel from.
        void UseAppropriateChunk(int x, int y, int z)
        {
            if (!VoxelOutOfBounds(x, y, z))
            {
                updateList(this, x, y, z);
            }

            Chunk c = parent.ChunkFromGlobal(VoxelCoordToGlobal(new Vector3Int(x,y,z)));
            if (c != null)
            {
                Vector3 neighborPos = c.transform.InverseTransformPoint(
                    transform.position + new Vector3Int(x, y, z)
                );

                updateList(c, (int)neighborPos.x, (int)neighborPos.y, (int)neighborPos.z);
            }
        }


        UseAppropriateChunk(x-1, y, z);
        UseAppropriateChunk(x+1, y, z);
        UseAppropriateChunk(x, y-1, z);
        UseAppropriateChunk(x, y+1, z);
        UseAppropriateChunk(x, y, z-1);
        UseAppropriateChunk(x, y, z+1);
    }

    /// <summary>
    /// Returns whether the given voxel coordinates are within the chunk.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private bool VoxelOutOfBounds(int x, int y, int z)
    {
        return x < 0 || x >= parent.chunkSize ||
               y < 0 || y >= parent.chunkHeight ||
               z < 0 || z >= parent.chunkSize;
    }
    /// <summary>
    /// Converts a voxel coordinate to its world-space position.
    /// </summary>
    /// <param name="coord"></param>
    /// <returns></returns>
    private Vector3 VoxelCoordToGlobal(Vector3 coord)
    {
        return transform.position + (coord / parent.resolution);
    }
    private void updateNeighborChunks()
    {
        Vector3 chunkPos = new(transform.position.x, transform.position.y, transform.position.z);

        neighbors[0] = parent.ChunkFromGlobal(chunkPos + (Vector3.up * parent.chunkHeight));
        neighbors[1] = parent.ChunkFromGlobal(chunkPos + (Vector3.down * parent.chunkHeight));
        neighbors[2] = parent.ChunkFromGlobal(chunkPos + (Vector3.left * parent.chunkSize));
        neighbors[3] = parent.ChunkFromGlobal(chunkPos + (Vector3.right * parent.chunkSize));
        neighbors[4] = parent.ChunkFromGlobal(chunkPos + (Vector3.forward * parent.chunkSize));
        neighbors[5] = parent.ChunkFromGlobal(chunkPos + (Vector3.back * parent.chunkSize));
    }

    //Debug only
    void OnDrawGizmos()
    {
        if (voxels == null || meshFilter.mesh == null) return;
        //Outline the whole chunk
        Gizmos.color = Color.black;
        if (meshFilter.mesh.GetIndices(0).Length != 0)
        {
            Gizmos.DrawWireMesh(meshFilter.mesh, transform.position);
        }
    }
}
