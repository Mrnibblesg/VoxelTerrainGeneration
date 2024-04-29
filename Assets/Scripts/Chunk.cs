using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static int size; //X*Z
    public static int height;
    Voxel[,,] voxels;
    Chunk[] neighbors;
    private Vector3Int chunkCoords;
    
    private Color col;

    private List<Vector3> meshVertices;
    private Dictionary<Vector3Int, int> vertexDict;
    private List<Vector2> meshUVs;
    private List<int> meshTris;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    public void Initialize(Vector3Int position)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        vertexDict = new Dictionary<Vector3Int, int>();

        chunkCoords = position;

        col = new Color(0, 0.7f, 0);
        meshRenderer.material.SetColor("_Color", col);

        voxels = new Voxel[size, height, size];
        InitializeVoxels();
        
    }

    void InitializeVoxels()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    voxels[x, y, z] = new Voxel(
                        Color.white
                        //,(x + y + z) % 2 == 1
                    );
                }
            }
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    MarkExposed(x,y,z);
                }
            }
        }

        
    }
    /// <summary>
    /// Mark the given voxel as exposed if it's adjacent to something transparent.
    /// </summary>
    public void MarkExposed(int x, int y, int z)
    {
        if (IsOutOfBounds(x, y, z)) { return; }

        voxels[x, y, z].exposed =
            VoxelHasTransparency(x + 1, y, z) ||
            VoxelHasTransparency(x - 1, y, z) ||
            VoxelHasTransparency(x, y + 1, z) ||
            VoxelHasTransparency(x, y - 1, z) ||
            VoxelHasTransparency(x, y, z + 1) ||
            VoxelHasTransparency(x, y, z - 1);
    }

    public void RegenerateMesh()
    {
        meshVertices = new List<Vector3>();
        meshUVs = new List<Vector2>();
        meshTris = new List<int>();
        vertexDict = new Dictionary<Vector3Int, int>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {   
                    if (!voxels[x, y, z].isAir && voxels[x,y,z].exposed)
                    {
                        AddFaces(new Vector3Int(x, y, z));
                    }
                }
            }
        }
        Mesh newMesh = new Mesh();
        newMesh.name = gameObject.name;

        //GreedyMeshing();
        newMesh.vertices = meshVertices.ToArray();
        newMesh.uv = meshUVs.ToArray();
        newMesh.triangles = meshTris.ToArray();
        meshFilter.mesh = newMesh;
        meshCollider.sharedMesh = newMesh;

        newMesh.RecalculateNormals();
    }

    public static readonly Vector3Int[] RightCorners = new Vector3Int[]
    {
        Voxel.RTF,
        Voxel.RBB,
        Voxel.RBF,
        Voxel.RBB,
        Voxel.RTF,
        Voxel.RTB
    };

    public static readonly Vector3Int[] LeftCorners = new Vector3Int[]
    {
        Voxel.LBF,
        Voxel.LBB,
        Voxel.LTF,
        Voxel.LTB,
        Voxel.LTF,
        Voxel.LBB
    };

    public static readonly Vector3Int[] TopCorners = new Vector3Int[]
    {
        Voxel.LTF,
        Voxel.LTB,
        Voxel.RTF,
        Voxel.RTB,
        Voxel.RTF,
        Voxel.LTB
    };

    public static readonly Vector3Int[] BottomCorners = new Vector3Int[]
    {
        Voxel.RBF,
        Voxel.LBB,
        Voxel.LBF,
        Voxel.LBB,
        Voxel.RBF,
        Voxel.RBB
    };

    public static readonly Vector3Int[] BackCorners = new Vector3Int[]
    {
        Voxel.LBF,
        Voxel.LTF,
        Voxel.RBF,
        Voxel.RTF,
        Voxel.RBF,
        Voxel.LTF
    };

    public static readonly Vector3Int[] FrontCorners = new Vector3Int[]
    {
        Voxel.RBB,
        Voxel.LTB,
        Voxel.LBB,
        Voxel.LTB,
        Voxel.RBB,
        Voxel.RTB
    };

    //Builds the mesh from the currently stored voxels
    //if voxel face is exposed, add to mesh
    //Tris are are either clockwise (CW) or counterclockwise (CCW).

    //Welcome to the brain scrambler
    void AddFaces(Vector3Int pos)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;

        // Action<Vector3Int> addAllVerticesToMesh = (Vector3Int dir) => meshTris.Add(AddVerticesToMesh(pos + dir));

        //Sharing corner vertices will cause a cube to appear to have
        //smooth edges. This needs to be fixed. Ensure that edge/corner voxels
        //are not shared, or determine an alternative solution, like supplying
        //normals manually.
        if (VoxelHasTransparency(x+1,y,z)) // +X (Right, CW)
        {
            meshTris.Add(AddVerticesToMesh(pos + RightCorners[0]));
            meshTris.Add(AddVerticesToMesh(pos + RightCorners[1]));
            meshTris.Add(AddVerticesToMesh(pos + RightCorners[2]));
            meshTris.Add(AddVerticesToMesh(pos + RightCorners[3]));
            meshTris.Add(AddVerticesToMesh(pos + RightCorners[4]));
            meshTris.Add(AddVerticesToMesh(pos + RightCorners[5]));
        }

        if (VoxelHasTransparency(x-1,y,z)) // -X (Left, CCW)
        {
            meshTris.Add(AddVerticesToMesh(pos + LeftCorners[0]));
            meshTris.Add(AddVerticesToMesh(pos + LeftCorners[1]));
            meshTris.Add(AddVerticesToMesh(pos + LeftCorners[2]));
            meshTris.Add(AddVerticesToMesh(pos + LeftCorners[3]));
            meshTris.Add(AddVerticesToMesh(pos + LeftCorners[4]));
            meshTris.Add(AddVerticesToMesh(pos + LeftCorners[5]));
        } 

        if (VoxelHasTransparency(x, y+1, z)) // +Y (Top, CW)
        {
            meshTris.Add(AddVerticesToMesh(pos + TopCorners[0]));
            meshTris.Add(AddVerticesToMesh(pos + TopCorners[1]));
            meshTris.Add(AddVerticesToMesh(pos + TopCorners[2]));
            meshTris.Add(AddVerticesToMesh(pos + TopCorners[3]));
            meshTris.Add(AddVerticesToMesh(pos + TopCorners[4]));
            meshTris.Add(AddVerticesToMesh(pos + TopCorners[5]));
        }

        if (VoxelHasTransparency(x, y-1, z)) // -Y (Bottom, CCW)
        {
            meshTris.Add(AddVerticesToMesh(pos + BottomCorners[0]));
            meshTris.Add(AddVerticesToMesh(pos + BottomCorners[1]));
            meshTris.Add(AddVerticesToMesh(pos + BottomCorners[2]));
            meshTris.Add(AddVerticesToMesh(pos + BottomCorners[3]));
            meshTris.Add(AddVerticesToMesh(pos + BottomCorners[4]));
            meshTris.Add(AddVerticesToMesh(pos + BottomCorners[5]));
        }

        if (VoxelHasTransparency(x, y, z+1)) // +Z (Front, CW)
        {
            meshTris.Add(AddVerticesToMesh(pos + FrontCorners[0]));
            meshTris.Add(AddVerticesToMesh(pos + FrontCorners[1]));
            meshTris.Add(AddVerticesToMesh(pos + FrontCorners[2]));
            meshTris.Add(AddVerticesToMesh(pos + FrontCorners[3]));
            meshTris.Add(AddVerticesToMesh(pos + FrontCorners[4]));
            meshTris.Add(AddVerticesToMesh(pos + FrontCorners[5]));
        }

        if (VoxelHasTransparency(x, y, z - 1)) // -Z (Back, CCW)
        {
            meshTris.Add(AddVerticesToMesh(pos + BackCorners[0]));
            meshTris.Add(AddVerticesToMesh(pos + BackCorners[1]));
            meshTris.Add(AddVerticesToMesh(pos + BackCorners[2]));
            meshTris.Add(AddVerticesToMesh(pos + BackCorners[3]));
            meshTris.Add(AddVerticesToMesh(pos + BackCorners[4]));
            meshTris.Add(AddVerticesToMesh(pos + BackCorners[5]));
        }
    }


    /// <summary>
    /// Input the position of the vertex you want to use on the mesh,
    /// this function will either add it, or return its index if it already
    ///  exists. Always returns a valid index.
    /// </summary>
    /// <remarks>
    ///  The purpose of this function is to deduplicate the vertices on meshes.
    /// </remarks>
    /// <param name="pos"></param>
    /// <returns>The index of the vertex that it added (or the one that already existed).</returns>
    int AddVerticesToMesh(Vector3Int pos)
    {
        int index;

        if (vertexDict.TryGetValue(pos, out index))
        {
            return index;
        }

        meshVertices.Add(pos);
        index = meshVertices.Count - 1;
        vertexDict.Add(pos, index);

        return index;
    }
/*    void addUVs()
    {
        meshUVs.Add(new Vector2(1, 1));
        meshUVs.Add(new Vector2(0, 1));
        meshUVs.Add(new Vector2(1, 0));
        meshUVs.Add(new Vector2(0, 0));
    }*/

    bool VoxelHasTransparency(int x, int y, int z)
    {
        if (IsOutOfBounds(x,y,z))
        {
            Vector3Int pos = new Vector3Int(x, y, z);
            return OutsideVoxelHasTransparency(pos);
        }
        return voxels[x, y, z].hasTransparency;
    }

    bool OutsideVoxelHasTransparency(Vector3 pos)
    {
        Vector3 globalPos = transform.position + pos;
        Chunk neighbor = WorldGenerator.World.GetChunk(globalPos);
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
    void OnDrawGizmos()
    {
        if (voxels == null || meshFilter.mesh == null) return;
        //Outline the whole chunk
        Gizmos.color = Color.black;
        //Gizmos.DrawCube(transform.position + new Vector3(size / 2, height / 2, size / 2), new Vector3(size, height, size));
        Gizmos.DrawWireMesh(meshFilter.mesh, transform.position);
    }

    /// <summary>
    /// Copies the voxel at the given position in this chunk.
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public Voxel? GetVoxel(Vector3 vec)
    {
        Vector3Int pos = new Vector3Int(
            Mathf.FloorToInt(vec.x),
            Mathf.FloorToInt(vec.y),
            Mathf.FloorToInt(vec.z)
        );

        bool outside = IsOutOfBounds(pos.x, pos.y, pos.z);

        return outside ? null : Voxel.Clone(voxels[pos.x, pos.y, pos.z]);
    }

    /// <summary>
    /// Sets the voxel at the given position in this chunk.
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="voxel"></param>
    /// <returns> Whether the voxel was set. </returns>
    public bool SetVoxel(Vector3 vec, Voxel voxel)
    {
        Vector3Int pos = new Vector3Int(
                       Mathf.FloorToInt(vec.x),
                       Mathf.FloorToInt(vec.y),
                       Mathf.FloorToInt(vec.z)
                    );

        bool outside = IsOutOfBounds(pos.x, pos.y, pos.z);
        if (outside) { return false; }

        voxels[pos.x, pos.y, pos.z] = Voxel.Clone(voxel);
        UpdateNeighbors(pos.x, pos.y, pos.z);
        
        // Update the mesh
        // Brute force for now
        RegenerateMesh();

        return true;
    }

    /// <summary>
    /// Update the state of voxels adjacent to the given position.
    /// </summary>
    /// <remarks>
    /// The origin of the updates must be from within the chunk. Neighbors may
    /// be outside.
    /// </remarks>
    private void UpdateNeighbors(int x, int y, int z)
    {
        if (IsOutOfBounds(x,y,z)) { return; }

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

        void UseAppropriateChunk(int x, int y, int z)
        {
            if (!IsOutOfBounds(x, y, z))
            {
                updateList(this, x, y, z);
            }

            Chunk c = WorldGenerator.World.GetChunk(transform.position + new Vector3(x, y, z));
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
    private bool IsOutOfBounds(int x, int y, int z)
    {
        return x < 0 || x >= size ||
               y < 0 || y >= height ||
               z < 0 || z >= size;
    }
}
