using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.Profiling;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Chunk : MonoBehaviour
{
    public Voxel[,,] voxels;

    public World parent;

    public static ProfilerMarker s_ChunkGen = new(ProfilerCategory.Render, "Chunk.RegenerateMesh"); //Profiling

    public static ProfilerMarker s_VoxelUpdate = new(ProfilerCategory.Render, "Chunk.SetVoxels"); //Profiling

    //more useful for chunks with many voxels
    Chunk[] neighbors;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

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

        if (voxel.type == VoxelType.AIR)
        {
            if (voxels[pos.x, pos.y, pos.z].type != VoxelType.AIR)
            {
                voxels[pos.x, pos.y, pos.z] = Voxel.Clone(voxel);
                if (pos.x == 0 || pos.x == parent.chunkSize-1 || pos.y == 0 || pos.y == parent.chunkSize-1 || pos.z == 0 || pos.z == parent.chunkSize-1)
                {
                    UpdateNeighbors(pos.x, pos.y, pos.z);
                }
            }
        }
        else
        {
            if (voxels[pos.x, pos.y, pos.z].type == VoxelType.AIR)
            {
                voxels[pos.x, pos.y, pos.z] = Voxel.Clone(voxel);
                if (pos.x == 0 || pos.x == parent.chunkSize-1 || pos.y == 0 || pos.y == parent.chunkSize-1 || pos.z == 0 || pos.z == parent.chunkSize-1)
                {
                    UpdateNeighbors(pos.x, pos.y, pos.z);
                }
            }
        }

        // Update the mesh
        // Brute force for now
        RegenerateMesh();

        return true;
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
                    vectors[i,j,k] *= parent.resolution;
                    Vector3Int pos = new Vector3Int(
                        Mathf.FloorToInt(vectors[i,j,k].x),
                        Mathf.FloorToInt(vectors[i,j,k].y),
                        Mathf.FloorToInt(vectors[i,j,k].z)
                    );

                    bool outside = VoxelOutOfBounds(pos.x, pos.y, pos.z);
                    if (outside) { continue; }

                    if (newVoxels[i,j,k].type == VoxelType.AIR)
                    {
                        if (voxels[pos.x, pos.y, pos.z].type != VoxelType.AIR)
                        {
                            voxels[pos.x, pos.y, pos.z] = Voxel.Clone(newVoxels[i,j,k]);
                        }
                    }
                    else
                    {
                        if (voxels[pos.x, pos.y, pos.z].type == VoxelType.AIR)
                        {
                            voxels[pos.x, pos.y, pos.z] = Voxel.Clone(newVoxels[i,j,k]);
                        }
                    }
                }
            }
            }

        Vector3Int chunkPos = new Vector3Int(
            (int)transform.position.x, 
            (int)transform.position.y, 
            (int)transform.position.z);

        if ((openFaces & POSXFACE) == POSXFACE) { UpdateNeighbors(chunkPos.x + parent.chunkSize, chunkPos.y + 1, chunkPos.z + 1); }
        if ((openFaces & POSYFACE) == POSYFACE) { UpdateNeighbors(chunkPos.x + 1, chunkPos.y + parent.chunkSize, chunkPos.z + 1); }
        if ((openFaces & POSZFACE) == POSZFACE) { UpdateNeighbors(chunkPos.x + 1, chunkPos.y + 1, chunkPos.z + parent.chunkSize); }
        if ((openFaces & NEGXFACE) == NEGXFACE) { UpdateNeighbors(chunkPos.x, chunkPos.y + 1, chunkPos.z + 1); }
        if ((openFaces & NEGYFACE) == NEGYFACE) { UpdateNeighbors(chunkPos.x + 1, chunkPos.y, chunkPos.z + 1); }
        if ((openFaces & NEGZFACE) == NEGZFACE) { UpdateNeighbors(chunkPos.x + 1, chunkPos.y + 1, chunkPos.z); }

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

        if (x == 0)
        {
            UseAppropriateChunk(x - 1, y, z);
        }
        if (x == parent.chunkSize-1)
        {
            UseAppropriateChunk(x + 1, y, z);
        }
        if (y == 0)
        {
            UseAppropriateChunk(x, y - 1, z);
        }
        if (y == parent.chunkSize - 1)
        {
            UseAppropriateChunk(x, y + 1, z);
        }
        if (z == 0)
        {
            UseAppropriateChunk(x, y, z - 1);
        }
        if (z == parent.chunkSize - 1)
        {
            UseAppropriateChunk(x, y, z + 1);
        }
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
