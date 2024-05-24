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
    /// Mark the given voxel as exposed if it's transparent or
    /// adjacent to something transparent.
    /// </summary>
    public void MarkExposed(int x, int y, int z)
    {
        if (VoxelOutOfBounds(x, y, z)) { return; }

        voxels[x, y, z].exposed =
            voxels[x,y,z].hasTransparency ||
            VoxelHasTransparency(x + 1, y, z) ||
            VoxelHasTransparency(x - 1, y, z) ||
            VoxelHasTransparency(x, y + 1, z) ||
            VoxelHasTransparency(x, y - 1, z) ||
            VoxelHasTransparency(x, y, z + 1) ||
            VoxelHasTransparency(x, y, z - 1);
    }

    /// <summary>
    /// Generates a mesh for this chunk
    /// </summary>
    public void RegenerateMesh()
    {
        updateNeighborChunks();
        ChunkMeshGenerator.RequestNewMesh(this);
    }
    public void ApplyNewMesh(Mesh m)
    {
        meshFilter.mesh = m;
        meshCollider.sharedMesh = m;
    }

    bool VoxelHasTransparency(int x, int y, int z)
    {
        if (VoxelOutOfBounds(x,y,z))
        {
            Vector3Int pos = new Vector3Int(x, y, z);
            return OutsideVoxelHasTransparency(pos);
        }
        return voxels[x, y, z].hasTransparency;
    }

    bool OutsideVoxelHasTransparency(Vector3 pos)
    {
        Vector3 globalPos = transform.position + pos;
        Chunk neighbor = parent.ChunkFromGlobal(globalPos);
        if (neighbor == null)
        {
            return true;
        }
        
        //Ensure we don't somehow use an invalid index
        try
        {
            Vector3 neighborPos = neighbor.gameObject.transform.InverseTransformPoint(globalPos);
            return neighbor.voxels[(int)neighborPos.x, (int)neighborPos.y, (int)neighborPos.z].hasTransparency;
        }
        catch(IndexOutOfRangeException e)
        {
            print(e);
        }
        return true;
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
        UpdateNeighbors(pos.x, pos.y, pos.z);
        
        // Update the mesh
        // Brute force for now
        RegenerateMesh();

        return true;
    }
    public bool SetVoxel(List<Vector3> vec, List<Voxel> voxel)
    {
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

            voxels[pos.x, pos.y, pos.z] = Voxel.Clone(voxel[i]);
            UpdateNeighbors(pos.x, pos.y, pos.z);
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
            c.MarkExposed(x, y, z);

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
        Gizmos.color = Color.black;
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
