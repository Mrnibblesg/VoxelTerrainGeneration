using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static int size; //X*Z
    public static int height = 128;
    Voxel[,,] voxels;
    
    private Color col;

    private List<Vector3> meshVertices;
    private List<Vector2> meshUVs;
    private List<int> meshTris;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    public void Initialize(int size)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        col = Color.green;
        meshRenderer.material.SetColor("_Color", col);

        voxels = new Voxel[size, height, size];
        InitializeVoxels();
        CreateMesh();
        
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
                        Color.white
                    );
                }
            }
        }
    }

    void CreateMesh()
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
        if (!isOpaque(x+1, y, z)) // +X (Right)
        {
            meshVertices.Add(new Vector3(x + 1,y + 1,z + 1));
            meshVertices.Add(new Vector3(x + 1,y + 1,z    ));
            meshVertices.Add(new Vector3(x + 1,y    ,z + 1));
            meshVertices.Add(new Vector3(x + 1,y    ,z    ));
            meshUVs.Add(new Vector2(1,1));
            meshUVs.Add(new Vector2(0,1));
            meshUVs.Add(new Vector2(1,0));
            meshUVs.Add(new Vector2(0,0));
            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 1);

            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 4);
        }
        if (!isOpaque(x-1, y, z)) // -X (Left)
        {
            meshVertices.Add(new Vector3(x, y + 1, z + 1));
            meshVertices.Add(new Vector3(x, y + 1, z    ));
            meshVertices.Add(new Vector3(x, y    , z + 1));
            meshVertices.Add(new Vector3(x, y    , z    ));
            meshUVs.Add(new Vector2(1, 1));
            meshUVs.Add(new Vector2(0, 1));
            meshUVs.Add(new Vector2(1, 0));
            meshUVs.Add(new Vector2(0, 0));
            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 1);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);

            meshTris.Add(vertexAmt - 4);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
        }
        if (!isOpaque(x, y+1, z)) // +Y (Top)
        {
            meshVertices.Add(new Vector3(x + 1, y + 1, z + 1));
            meshVertices.Add(new Vector3(x + 1, y + 1, z    ));
            meshVertices.Add(new Vector3(x    , y + 1, z + 1));
            meshVertices.Add(new Vector3(x    , y + 1, z    ));
            meshUVs.Add(new Vector2(1, 1));
            meshUVs.Add(new Vector2(0, 1));
            meshUVs.Add(new Vector2(1, 0));
            meshUVs.Add(new Vector2(0, 0));
            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 1);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);

            meshTris.Add(vertexAmt - 4);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
        }
        if (!isOpaque(x, y-1, z)) // -Y (Bottom)
        {
            meshVertices.Add(new Vector3(x + 1, y    , z + 1));
            meshVertices.Add(new Vector3(x + 1, y    , z    ));
            meshVertices.Add(new Vector3(x    , y    , z + 1));
            meshVertices.Add(new Vector3(x    , y    , z    ));
            meshUVs.Add(new Vector2(1, 1));
            meshUVs.Add(new Vector2(0, 1));
            meshUVs.Add(new Vector2(1, 0));
            meshUVs.Add(new Vector2(0, 0));
            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 1);

            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 4);
        }
        if (!isOpaque(x, y, z+1)) // +Z (Front)
        {
            meshVertices.Add(new Vector3(x + 1, y + 1, z + 1));
            meshVertices.Add(new Vector3(x + 1, y    , z + 1));
            meshVertices.Add(new Vector3(x    , y + 1, z + 1));
            meshVertices.Add(new Vector3(x    , y    , z + 1));
            meshUVs.Add(new Vector2(1, 1));
            meshUVs.Add(new Vector2(0, 1));
            meshUVs.Add(new Vector2(1, 0));
            meshUVs.Add(new Vector2(0, 0));
            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 1);

            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 4);
        }
        if (!isOpaque(x, y, z-1)) // -Z (Back)
        {
            meshVertices.Add(new Vector3(x + 1, y + 1, z    ));
            meshVertices.Add(new Vector3(x + 1, y    , z    ));
            meshVertices.Add(new Vector3(x    , y + 1, z    ));
            meshVertices.Add(new Vector3(x    , y    , z    ));
            meshUVs.Add(new Vector2(1, 1));
            meshUVs.Add(new Vector2(0, 1));
            meshUVs.Add(new Vector2(1, 0));
            meshUVs.Add(new Vector2(0, 0));
            int vertexAmt = meshVertices.Count;

            meshTris.Add(vertexAmt - 1);
            meshTris.Add(vertexAmt - 2);
            meshTris.Add(vertexAmt - 3);

            meshTris.Add(vertexAmt - 4);
            meshTris.Add(vertexAmt - 3);
            meshTris.Add(vertexAmt - 2);
        }
    }

    bool isOpaque(int x, int y, int z)
    {
        if (x < 0 || x >= size || 
            y < 0 || y >= height ||
            z < 0 || z >= size)
        {
            return false;
        }
        return voxels[x, y, z].active;
    }

    void OnDrawGizmos()
    {
        if (voxels == null || meshFilter.mesh == null) return;
        //Outline the whole chunk
        Gizmos.color = Color.black;
        //Gizmos.DrawCube(transform.position + new Vector3(size / 2, height / 2, size / 2), new Vector3(size, height, size));
        Gizmos.DrawWireMesh(meshFilter.mesh);
    }
}
