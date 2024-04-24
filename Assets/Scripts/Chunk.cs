using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Chunk : MonoBehaviour
{
    public static int size; //X*Z
    public static int height = 128;
    Voxel[,,] voxels;
    private Vector3Int chunkCoords;
    
    private Color col;

    private List<Vector3> meshVertices;
    private List<Vector2> meshUVs;
    private List<int> meshTris;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    public void Initialize(int size, Vector3Int position)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        chunkCoords = position;

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
        if (!IsOpaque(x+1, y, z)) // +X (Right)
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
        if (!IsOpaque(x-1, y, z)) // -X (Left)
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
        if (!IsOpaque(x, y+1, z)) // +Y (Top)
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
        if (!IsOpaque(x, y-1, z)) // -Y (Bottom)
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
        if (!IsOpaque(x, y, z+1)) // +Z (Front)
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
        if (!IsOpaque(x, y, z-1)) // -Z (Back)
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

    bool IsOpaque(int x, int y, int z)
    {
        if (y < 0 || y >= height) //above or below
        {
            return false;
        }
        if (x < 0 || x >= size || //outside of chunk
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
        Chunk neighbor = WorldGenerator.world.GetChunk(globalPos);
        if (neighbor == null)
        {
            return false;
        }
        Vector3 neighborPos = neighbor.gameObject.transform.InverseTransformPoint(globalPos);
        return neighbor.voxels[(int)neighborPos.x, (int)neighborPos.y, (int)neighborPos.z].active;
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
