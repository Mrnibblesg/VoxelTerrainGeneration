using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static int size; //X*Z
    public static int height;
    Voxel[,,] voxels;
    private Vector3Int chunkCoords;
    
    private Color col;

    private List<Vector3> meshVertices;
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
                        Color.white//,
                        //(x + y + z) % 2 == 1
                    );
                }
            }
        }
    }

    public void Render()
    {
        meshVertices = new List<Vector3>();
        meshUVs = new List<Vector2>();
        meshTris = new List<int>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (voxels[x, y, z].active)
                    {
                        AddFaces(x, y, z);
                    }
                }
            }
        }
        Mesh newMesh = new Mesh();

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
    //TRI POINTS SHOULD BE EITHER CLOCKWISE OR COUNTERCLOCKWISE, IT MATTERS
    void AddFaces(int x, int y, int z)
    {
        if (!IsOpaque(x+1, y, z)) // +X (Right)
        {
            meshVertices.Add(new Vector3(x + 1,y + 1,z + 1));
            meshVertices.Add(new Vector3(x + 1,y + 1,z    ));
            meshVertices.Add(new Vector3(x + 1,y    ,z + 1));
            meshVertices.Add(new Vector3(x + 1,y    ,z    ));
            addUVs();

            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 1);

            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 4);
        }
        if (!IsOpaque(x-1, y, z)) // -X (Left)
        {
            meshVertices.Add(new Vector3(x, y + 1, z + 1));
            meshVertices.Add(new Vector3(x, y + 1, z    ));
            meshVertices.Add(new Vector3(x, y    , z + 1));
            meshVertices.Add(new Vector3(x, y    , z    ));
            addUVs();

            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 1);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);

            meshTris.Add(vertexAmt - 4);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
        }
        if (!IsOpaque(x, y+1, z)) // +Y (Top)
        {
            meshVertices.Add(new Vector3(x + 1, y + 1, z + 1));
            meshVertices.Add(new Vector3(x + 1, y + 1, z    ));
            meshVertices.Add(new Vector3(x    , y + 1, z + 1));
            meshVertices.Add(new Vector3(x    , y + 1, z    ));
            addUVs();

            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 1);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);

            meshTris.Add(vertexAmt - 4);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
        }
        if (!IsOpaque(x, y-1, z)) // -Y (Bottom)
        {
            meshVertices.Add(new Vector3(x + 1, y    , z + 1));
            meshVertices.Add(new Vector3(x + 1, y    , z    ));
            meshVertices.Add(new Vector3(x    , y    , z + 1));
            meshVertices.Add(new Vector3(x    , y    , z    ));
            addUVs();

            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 1);

            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 4);
        }
        if (!IsOpaque(x, y, z+1)) // +Z (Front)
        {
            meshVertices.Add(new Vector3(x + 1, y + 1, z + 1));
            meshVertices.Add(new Vector3(x + 1, y    , z + 1));
            meshVertices.Add(new Vector3(x    , y + 1, z + 1));
            meshVertices.Add(new Vector3(x    , y    , z + 1));
            addUVs();

            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 1);

            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 4);
        }
        if (!IsOpaque(x, y, z-1)) // -Z (Back)
        {
            meshVertices.Add(new Vector3(x + 1, y + 1, z    ));
            meshVertices.Add(new Vector3(x + 1, y    , z    ));
            meshVertices.Add(new Vector3(x    , y + 1, z    ));
            meshVertices.Add(new Vector3(x    , y    , z    ));
            addUVs();

            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 1);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);

            meshTris.Add(vertexAmt - 4);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
        }
    }
    void addUVs()
    {
        meshUVs.Add(new Vector2(1, 1));
        meshUVs.Add(new Vector2(0, 1));
        meshUVs.Add(new Vector2(1, 0));
        meshUVs.Add(new Vector2(0, 0));
    }

    bool IsOpaque(int x, int y, int z)
    {
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
        Render();

        return !outside;
    }
}
