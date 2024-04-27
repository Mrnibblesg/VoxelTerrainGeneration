using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

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

        //col = Color.green;
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
                        transform.position + new Vector3(x, y, z),
                        Color.white,
                        true//(x + y + z) % 2 == 1
                    );
                }
            }
        }
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
                    if (voxels[x, y, z].active)
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

    //Builds the mesh from the currently stored voxels
    //if voxel face is exposed, add to mesh
    //Currently inefficient, we add multiple copies of the same vertex.
    //Tris are are either clockwise (CW) or counterclockwise (CCW).

    //Since we are only using flat colors on our meshes for now, we actually
    //don't need to apply the UVs, saving us memory.

    //Welcome to the brain scrambler
    void AddFaces(Vector3Int pos)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;

        Action<Vector3Int> addAllVerticesToMesh = (Vector3Int dir) => meshTris.Add(AddVerticesToMesh(pos + dir));

        //Sharing corner vertices will cause a cube to appear to have
        //smooth edges. This needs to be fixed. Ensure that edge/corner voxels
        //are not shared, or determine an alternative solution, like supplying
        //normals manually.
        if (!IsOpaque(pos + Vector3Int.right)) // +X (Right, CW)
        {
            List<Vector3Int> relevantDirections = new List<Vector3Int>()
            {
                Voxel.RTF,
                Voxel.RBB,
                Voxel.RBF,
                Voxel.RBB,
                Voxel.RTF,
                Voxel.RTB
            };

            relevantDirections.ForEach(addAllVerticesToMesh);
        }

        if (!IsOpaque(pos + Vector3Int.left)) // -X (Left, CCW)
        {
            List<Vector3Int> relevantDirections = new List<Vector3Int>()
            {
                Voxel.LBF,
                Voxel.LBB,
                Voxel.LTF,
                Voxel.LTB,
                Voxel.LTF,
                Voxel.LBB
            };

            relevantDirections.ForEach(addAllVerticesToMesh);
        } 

        if (!IsOpaque(pos + Vector3Int.up)) // +Y (Top, CW)
        {
            List<Vector3Int> relevantDirections = new List<Vector3Int>()
            {
                Voxel.LTF,
                Voxel.LTB,
                Voxel.RTF,
                Voxel.RTB,
                Voxel.RTF,
                Voxel.LTB
            };

            relevantDirections.ForEach(addAllVerticesToMesh);
        }

        if (!IsOpaque(pos + Vector3Int.down)) // -Y (Bottom, CCW)
        {
            List<Vector3Int> relevantDirections = new List<Vector3Int>()
            {
                Voxel.RBF,
                Voxel.LBB,
                Voxel.LBF,
                Voxel.LBB,
                Voxel.RBF,
                Voxel.RBB
            };

            relevantDirections.ForEach(addAllVerticesToMesh);
        }

        if (!IsOpaque(pos + Vector3Int.back)) // +Z (Front, CCW)
        {
            List<Vector3Int> vector3Ints = new List<Vector3Int>()
            {
                Voxel.LBF,
                Voxel.LTF,
                Voxel.RBF,
                Voxel.RTF,
                Voxel.RBF,
                Voxel.LTF
            };

            vector3Ints.ForEach(addAllVerticesToMesh);
        }

        if (!IsOpaque(pos + Vector3Int.forward)) // -Z (Back, CW)
        {
            List<Vector3Int> relevantDirections = new List<Vector3Int>()
            {
                Voxel.RBB,
                Voxel.LTB,
                Voxel.LBB,
                Voxel.LTB,
                Voxel.RBB,
                Voxel.RTB
            };

            relevantDirections.ForEach(addAllVerticesToMesh);
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

    bool IsOpaque(Vector3Int position)
    {
        int x = position.x;
        int y = position.y;
        int z = position.z;

        if (x < 0 || x >= size || //outside of chunk
            y < 0 || y >= height ||
            z < 0 || z >= size)
        {
            Vector3Int pos = new Vector3Int(x, y, z);
            return OutsideVoxelOpaque(pos);
        }
        return voxels[x, y, z].active;
    }

    //Is the voxel at pos which is outside this chunk opaque?
    bool OutsideVoxelOpaque(Vector3 pos)
    {
        Vector3 globalPos = transform.position + pos;
        Chunk neighbor = WorldGenerator.World.GetChunk(globalPos);
        if (neighbor == null)
        {
            return false;
        }
        
        //Ensure we don't somehow use an invalid index
        try
        {
            Vector3 neighborPos = neighbor.gameObject.transform.InverseTransformPoint(globalPos);
            return neighbor.voxels[(int)neighborPos.x, (int)neighborPos.y, (int)neighborPos.z].active;
        }
        catch(IndexOutOfRangeException e)
        {
            print(e);
        }
        return false;
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

        bool outside = pos.x < 0 || pos.x >= size ||
                       pos.y < 0 || pos.y >= height ||
                       pos.z < 0 || pos.z >= size;

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

        bool outside = pos.x < 0 || pos.x >= size ||
                       pos.y < 0 || pos.y >= height ||
                       pos.z < 0 || pos.z >= size;

        if (!outside)
        {
            voxels[pos.x, pos.y, pos.z] = Voxel.Clone(voxel);
        }

        // Update the mesh
        // Brute force for now
        RegenerateMesh();

        return !outside;
    }
}
