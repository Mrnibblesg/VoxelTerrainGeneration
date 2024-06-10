using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public VoxelRun voxels;

    public World world;

    //Store refs to neighbors used when you request a new mesh
    public Chunk[] neighbors;
    //up, down, left, right, forward, back

    public static ProfilerMarker s_ChunkReq = new(ProfilerCategory.Render, "Chunk request new mesh"); //Profiling

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    //Use to avoid race conditions related to mesh requests & job completion time
    private long lastMeshUpdateTime = -1;

    public void Initialize(World world)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.vertexColorMaterial;

        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

        neighbors = new Chunk[6];

        this.world = world;

        voxels = new VoxelRun(world.parameters.ChunkSize, world.parameters.ChunkHeight);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < neighbors.Length; i++)
        {
            neighbors[i] = null;
        }
        Destroy(meshFilter.sharedMesh);
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
        
        Vector3Int pos = LocalToVoxelCoord(vec);

        bool outside = VoxelOutOfBounds(pos.x, pos.y, pos.z);

        return outside ? null : GetVoxel(pos);
    }

    /// <summary>
    /// Return the voxel coordinate given a localized position
    /// </summary>
    /// <returns></returns>
    public Vector3Int LocalToVoxelCoord(Vector3 vec)
    {
        //Get from some coordinate within the chunk to the appropriate voxel coords.
        vec *= world.parameters.Resolution;
        Vector3Int pos = new Vector3Int(
            Mathf.FloorToInt(vec.x),
            Mathf.FloorToInt(vec.y),
            Mathf.FloorToInt(vec.z)
        );
        return pos;
    }

    /// <summary>
    /// Get the voxel
    /// </summary>
    /// <param name="voxCoords"></param>
    /// <returns></returns>
    public Voxel GetVoxel(Vector3Int voxCoords)
    {
        return VoxelRun.Get(voxels,
            voxCoords.x * world.parameters.ChunkSize * world.parameters.ChunkHeight +
            voxCoords.y * world.parameters.ChunkSize +
            voxCoords.z);
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
        Vector3Int pos = LocalToVoxelCoord(vec);

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
            voxCoords.x * world.parameters.ChunkSize * world.parameters.ChunkHeight + 
            voxCoords.y * world.parameters.ChunkSize + 
            voxCoords.z);
    }
    /// <summary>
    /// Set the voxels between localized p1 and p2 to the given voxel
    /// </summary>
    /// <returns></returns>
    public bool SetVoxels(Vector3 p1, Vector3 p2, Voxel voxel)
    {
        Vector3Int p1Coords = LocalToVoxelCoord(p1);
        Vector3Int p2Coords = LocalToVoxelCoord(p2);

        bool changed = false;

        //Only loop through the portion of the selection that intersects with this chunk
        for (int x = Math.Max(0, p1Coords.x); x <= Math.Min(world.parameters.ChunkSize-1, p2Coords.x); x++)
        {
            for (int y = Math.Max(0, p1Coords.y); y <= Math.Min(world.parameters.ChunkHeight-1, p2Coords.y); y++)
            {
                for (int z = Math.Max(0, p1Coords.z); z <= Math.Min(world.parameters.ChunkSize-1, p2Coords.z); z++)
                {
                    SetVoxel(new Vector3Int(x, y, z), voxel);
                    changed = true;
                }
            }
        }
        if (changed) { RegenerateMesh(); }
        
        return changed;
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

            Chunk c = world.ChunkFromGlobal(VoxelCoordToGlobal(new Vector3Int(x,y,z)));
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
        return x < 0 || x >= world.parameters.ChunkSize ||
               y < 0 || y >= world.parameters.ChunkHeight ||
               z < 0 || z >= world.parameters.ChunkSize;
    }
    /// <summary>
    /// Converts a voxel coordinate to its world-space position.
    /// </summary>
    /// <param name="coord"></param>
    /// <returns></returns>
    private Vector3 VoxelCoordToGlobal(Vector3 coord)
    {
        return transform.position + (coord / world.parameters.Resolution);
    }
    private void updateNeighborChunks()
    {
        Vector3 chunkPos = new(transform.position.x, transform.position.y, transform.position.z);

        float globalChunkHeight = world.parameters.ChunkHeight / world.parameters.Resolution;
        float globalChunkSize = world.parameters.ChunkSize / world.parameters.Resolution;

        neighbors[0] = world.ChunkFromGlobal(chunkPos + (Vector3.up * globalChunkHeight));
        neighbors[1] = world.ChunkFromGlobal(chunkPos + (Vector3.down * globalChunkHeight));
        neighbors[2] = world.ChunkFromGlobal(chunkPos + (Vector3.left * globalChunkSize));
        neighbors[3] = world.ChunkFromGlobal(chunkPos + (Vector3.right * globalChunkSize));
        neighbors[4] = world.ChunkFromGlobal(chunkPos + (Vector3.forward * globalChunkSize));
        neighbors[5] = world.ChunkFromGlobal(chunkPos + (Vector3.back * globalChunkSize));
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
