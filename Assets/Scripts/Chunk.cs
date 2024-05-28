using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.Profiling;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public VoxelRun voxels;

    public World parent;

    //Store refs to neighbors used when you request a new mesh
    public Chunk[] neighbors;
    //up, down, left, right, forward, back

    public static ProfilerMarker s_VoxelUpdate = new(ProfilerCategory.Render, "Chunk.SetVoxels"); //Profiling

    public static ProfilerMarker s_ChunkReq = new(ProfilerCategory.Render, "Chunk request new mesh"); //Profiling

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    //Use to avoid race conditions related to mesh requests & job completion time
    private long lastMeshUpdateTime = -1;

    //directional face bit flags
    public static int POSXFACE = 0b00000001;
    public static int POSYFACE = 0b00000010;
    public static int POSZFACE = 0b00000100;
    public static int NEGXFACE = 0b00001000;
    public static int NEGYFACE = 0b00010000;
    public static int NEGZFACE = 0b00100000;

    public void Initialize(World parent)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = parent.vertexColorMaterial;

        neighbors = new Chunk[6];

        this.parent = parent;

        voxels = new VoxelRun(parent.chunkSize, parent.chunkHeight);
    }

    /// <summary>
    /// Generates a mesh for this chunk
    /// </summary>
    public void RegenerateMesh()
    {
        s_ChunkReq.Begin();
        updateNeighborChunks();
        ChunkMeshGenerator.RequestNewMesh(this);
        s_ChunkReq.End();
    }
    public void ApplyNewMesh(Mesh m, long requestTime)
    {
        //Ignore if the mesh was requested earlier than the current one was (outdated mesh)
        if (requestTime < lastMeshUpdateTime)
        {
            Destroy(m);
            return;
        }
        Destroy(meshFilter.sharedMesh);
        meshFilter.sharedMesh = m;
        meshCollider.sharedMesh = m;
        lastMeshUpdateTime = requestTime;
    }

    /// <summary>
    /// Copies the voxel at the global position in this chunk.
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public Voxel? VoxelFromLocal(Vector3 vec)
    {
        //Get from some coordinate within the chunk to the appropriate voxel coords.
        vec *= parent.resolution;
        Vector3Int pos = new Vector3Int(
            Mathf.FloorToInt(vec.x),
            Mathf.FloorToInt(vec.y),
            Mathf.FloorToInt(vec.z)
        );

        bool outside = VoxelOutOfBounds(pos.x, pos.y, pos.z);

        return outside ? null : GetVoxel(pos);
    }
    /// <summary>
    /// Get the voxel
    /// </summary>
    /// <param name="voxCoords"></param>
    /// <returns></returns>
    public Voxel GetVoxel(Vector3Int voxCoords)
    {
        return VoxelRun.Get(voxels,
            voxCoords.x * parent.chunkSize * parent.chunkHeight +
            voxCoords.y * parent.chunkSize +
            voxCoords.z);
    }
public Voxel GetVoxel(int x, int y, int z)
    {
        return VoxelRun.Get(voxels,
            x * parent.chunkSize * parent.chunkHeight +
            y * parent.chunkSize +
            z);
    }

    /// <summary>
    /// Sets the voxel at the local world-space position in this chunk.
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="voxel"></param>
    /// <returns> Whether the voxel was set. </returns>
    public bool SetVoxelFromLocal(Vector3 vec, Voxel voxel)
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

        if (SetVoxel(pos, Voxel.Clone(voxel)))
        {
            UpdateNeighbors(pos.x, pos.y, pos.z);
            RegenerateMesh();
            return true;
        }
        return false;
    }
    private bool SetVoxel(Vector3Int voxCoords, Voxel voxel)
    {
        return VoxelRun.Set(voxels, voxel,
            voxCoords.x * parent.chunkSize * parent.chunkHeight + 
            voxCoords.y * parent.chunkSize + 
            voxCoords.z);
    }
    public bool SetVoxels(Vector3[,,] vectors, Voxel[,,] newVoxels, int openFaces)
    {
        s_VoxelUpdate.Begin();

        if (vectors.Length != newVoxels.Length)
        {
            s_VoxelUpdate.End();
            return false;
        }

        for (int i = 0; i < parent.chunkSize; i++)
        {
            for (int j = 0; j < parent.chunkSize; j++)
            {
                for (int k = 0; k < parent.chunkSize; k++)
                {
                    //Scale the world-space coordinate to voxel-coordinate space
                    vectors[i, j, k] *= parent.resolution;
                    Vector3Int pos = new Vector3Int(
                        Mathf.FloorToInt(vectors[i, j, k].x),
                        Mathf.FloorToInt(vectors[i, j, k].y),
                        Mathf.FloorToInt(vectors[i, j, k].z)
                    );

                    bool outside = VoxelOutOfBounds(pos.x, pos.y, pos.z);
                    if (outside) { continue; }

                    if (newVoxels[i,j,k].type == VoxelType.AIR)
                    {
                        if (GetVoxel(pos).type != VoxelType.AIR)
                        {
                            SetVoxel(pos, Voxel.Clone(newVoxels[i,j,k]));
                        }
                    }
                    else
                    {
                        if (GetVoxel(pos).type == VoxelType.AIR)
                        {
                            SetVoxel(pos, Voxel.Clone(newVoxels[i,j,k]));
                        }
                    }
                }
            }
        }

        Vector3Int chunkPos = new Vector3Int(
            (int)transform.position.x,
            (int)transform.position.y,
            (int)transform.position.z);

        UpdateNeighbors(chunkPos.x, chunkPos.y + 1, chunkPos.z + 1);
        UpdateNeighbors(chunkPos.x + 1, chunkPos.y, chunkPos.z + 1);
        UpdateNeighbors(chunkPos.x + 1, chunkPos.y + 1, chunkPos.z);
        UpdateNeighbors(chunkPos.x + parent.chunkSize, chunkPos.y + 1, chunkPos.z + 1);
        UpdateNeighbors(chunkPos.x + 1, chunkPos.y + parent.chunkSize, chunkPos.z + 1);
        UpdateNeighbors(chunkPos.x + 1, chunkPos.y + 1, chunkPos.z + parent.chunkSize);

        // Update the mesh
        // Brute force for now
        //RegenerateMesh();

        s_VoxelUpdate.End();
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
        if (VoxelOutOfBounds(x, y, z)) { return; }

        //Make sure when adding to this function that the things you add DO NOT trigger
        //more updates, or the updates could cascade forever.

        //Use the proper chunk to update the neighbor voxel from.
        void UseAppropriateChunk(int x, int y, int z)
        {
            if (VoxelOutOfBounds(x, y, z))
            {
                Chunk c = parent.ChunkFromGlobal(VoxelCoordToGlobal(new Vector3Int(x, y, z)));
                if (c != null)
                {
                    c.RegenerateMesh();
                }
            }
        }

        if (x == 0) { UseAppropriateChunk(x - 1, y, z); }
        if (y == 0) { UseAppropriateChunk(x, y - 1, z); }
        if (z == 0) { UseAppropriateChunk(x, y, z - 1); }
        if (x == parent.chunkSize - 1) { UseAppropriateChunk(x + 1, y, z); }
        if (y == parent.chunkSize - 1) { UseAppropriateChunk(x, y + 1, z); }
        if (z == parent.chunkSize - 1) { UseAppropriateChunk(x, y, z + 1); }
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

        neighbors[0] = parent.ChunkFromGlobal(chunkPos + (Vector3.up * parent.chunkHeight / parent.resolution));
        neighbors[1] = parent.ChunkFromGlobal(chunkPos + (Vector3.down * parent.chunkHeight / parent.resolution));
        neighbors[2] = parent.ChunkFromGlobal(chunkPos + (Vector3.left * parent.chunkSize / parent.resolution));
        neighbors[3] = parent.ChunkFromGlobal(chunkPos + (Vector3.right * parent.chunkSize / parent.resolution));
        neighbors[4] = parent.ChunkFromGlobal(chunkPos + (Vector3.forward * parent.chunkSize / parent.resolution));
        neighbors[5] = parent.ChunkFromGlobal(chunkPos + (Vector3.back * parent.chunkSize / parent.resolution));
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
