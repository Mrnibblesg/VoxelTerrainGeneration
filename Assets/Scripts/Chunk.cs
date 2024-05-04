using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static int size; //X*Z
    public static int height;
    private static Material vertexColorMaterial;
    Voxel[,,] voxels;
    Chunk[] neighbors;
    private Vector3Int chunkCoords;

    private List<Vector3> meshVertices;
    private List<Color32> meshColors;
    private List<int> meshQuads;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    static Chunk()
    {
        vertexColorMaterial = (Material)Resources.Load("Textures/Vertex Colors");
    }
    public void Initialize(Vector3Int position)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = vertexColorMaterial;

        chunkCoords = position;

        voxels = new Voxel[size, height, size];
        InitializeVoxels();
        
    }

    void InitializeVoxels()
    {
        Array types = Enum.GetValues(typeof(VoxelType));
        System.Random r = new System.Random();
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    VoxelType type= (VoxelType)types.GetValue(r.Next(1, types.Length-1));
                    voxels[x, y, z] = new Voxel(
                         type,//(x + z) % 2 == 0 ? VoxelType.Type.GRASS : VoxelType.Type.DIRT, 
                         false, false
                         //(type == VoxelType.Type.GLASS ? true : false)
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
    /// Mark the given voxel as exposed if it's transparent or
    /// adjacent to something transparent.
    /// </summary>
    public void MarkExposed(int x, int y, int z)
    {
        if (IsOutOfBounds(x, y, z)) { return; }

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
        meshVertices = new List<Vector3>();
        meshColors = new List<Color32>();
        meshQuads = new List<int>();

        Mesh newMesh = new Mesh();
        newMesh.name = gameObject.name;
        //newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GreedyMeshing();
        newMesh.vertices = meshVertices.ToArray();
        newMesh.colors32 = meshColors.ToArray();
        newMesh.SetIndices(meshQuads.ToArray(), MeshTopology.Quads,0);

        meshFilter.mesh = newMesh;
        meshCollider.sharedMesh = newMesh;

        newMesh.RecalculateNormals();
    }

    /// <summary>
    /// Uses greedy meshing to merge voxel faces producing a memory efficient
    /// mesh.
    /// </summary>
    /// <remarks>Complicated.</remarks>
    //Algorithm borrowed from:
    //https://0fps.net/2012/07/07/meshing-minecraft-part-2/
    private void GreedyMeshing()
    {
        int[] dimensions = new int[] { size, height, size };
        //Iterate over the 3 axes.
        //a variable representing an axis will change each iteration.

        for (int normal = 0; normal < 3; normal++)
        {
            //The other 2 axes of the current slice direction.
            //Not the same as texture mapping UVs.
            int u = (normal + 1) % 3;
            int v = (normal + 2) % 3;

            //stores distinct types of vertices to
            //turn into quads on current slice
            int[,] mask = new int[dimensions[u],dimensions[v]];

            //stores progress through the volume.
            //each element is basically used as an iterator for that dimension.
            int[] progress = new int[] { 0, 0, 0 };

            //Add each element to your position to move up 1 slice
            int[] normOff = new int[] { 0, 0, 0 };
            normOff[normal] = 1;

            //Compute mask for this slice
            for (progress[normal] = -1; progress[normal] < dimensions[normal];)
            {

                int n = 0;
                for (progress[u] = 0; progress[u] < dimensions[u]; progress[u]++)
                {
                    for (progress[v] = 0; progress[v] < dimensions[v]; progress[v]++, n++)
                    {
                        //Bounds checking/voxel type checking
                        //Even if one bound isn't actually air and it's a voxel in another chunk,
                        //we say it's air anyways because we aren't meshing across chunks.
                        //voxels below and above the current slice
                        VoxelType below = 
                            (progress[normal] >= 0 ?
                            voxels[progress[0],
                                   progress[1],
                                   progress[2]].type :
                            VoxelType.AIR);

                        VoxelType above =
                            (progress[normal] < dimensions[normal] - 1 ?
                            voxels[progress[0]+normOff[0],
                                   progress[1]+normOff[1],
                                   progress[2]+normOff[2]].type :
                            VoxelType.AIR);

                        //no face if they're both a voxel or if the're both air
                        if ((above == VoxelType.AIR) ==
                            (below == VoxelType.AIR))
                        {
                            mask[progress[u], progress[v]] = 0;
                        }
                        else if (below != VoxelType.AIR)
                        {
                            mask[progress[u], progress[v]] = (int)below;
                        }
                        else
                        {
                            //A negative value means that the face is facing the
                            //opposite direction.
                            mask[progress[u], progress[v]] = -(int)above;
                        }
                    }
                }
                progress[normal]++;

                //Create quads from the mask
                for (int j = 0; j < dimensions[v]; j++)
                {
                    for (int i = 0; i < dimensions[u];)
                    {
                        int current = mask[i, j];
                        
                        if (current == 0)
                        {
                            i++;
                            continue;
                        }

                        //Calculate width of quad
                        int w;
                        for (w = 1; i + w < dimensions[u] && current == mask[i + w, j]; w++) { }

                        int h;
                        //Calculate height of quad
                        for (h = 1; j + h < dimensions[v]; h++)
                        {
                            for (int k = 0; k < w; k++)
                            {
                                if (current != mask[i+k,j+h])
                                {
                                    goto BreakHeight; //Break from 2 loops cleanly
                                }
                            }
                        }
                    BreakHeight:
                        //we now have our quad: i and j are its starting pos,
                        //w and h the width and height.
                        progress[u] = i;
                        progress[v] = j;
                        //We apply the w and h in the correct dimension
                        //according to our current axes so we can properly
                        //calculate our quad coordinates.
                        int[] uOff = new int[] { 0, 0, 0 };
                        uOff[u] = w;
                        int[] vOff = new int[] { 0, 0, 0 };
                        vOff[v] = h;

                        AddQuad(current, progress, uOff, vOff);

                        //Mark the section of mask that the quad occupied as done.
                        for (int k = 0; k < w; k++)
                        {
                            for (int l = 0; l < h; l++)
                            {
                                mask[i + k, j + l] = 0;
                            }
                        }
                        i += w;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Add a quad to the current list of
    /// mesh vertices, colors, and quad indices.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="pos"></param>
    /// <param name="uOff"></param>
    /// <param name="vOff"></param>
    private void AddQuad(int type, int[] pos, int[] uOff, int[] vOff)
    {
        bool CCW = type < 0;
        if (type < 0) type *= -1;
        Color32 col = ((VoxelType) type).getVoxelAttributes().color;

        int verts = meshVertices.Count;

        meshVertices.Add( //Bottom left corner
            new(
            pos[0],
            pos[1],
            pos[2]));

        meshVertices.Add( //Bottom right corner
            new(
            pos[0] + uOff[0],
            pos[1] + uOff[1],
            pos[2] + uOff[2]));

        meshVertices.Add( // Top right corner
            new(
            pos[0] + uOff[0] + vOff[0],
            pos[1] + uOff[1] + vOff[1],
            pos[2] + uOff[2] + vOff[2]));

        meshVertices.Add( // Top left corner
            new(
            pos[0] + vOff[0],
            pos[1] + vOff[1],
            pos[2] + vOff[2]));

        meshColors.Add(col);
        meshColors.Add(col);
        meshColors.Add(col);
        meshColors.Add(col);
        if (CCW)
        {
            meshQuads.Add(verts + 3);
            meshQuads.Add(verts + 2);
            meshQuads.Add(verts + 1);
            meshQuads.Add(verts);
        }
        else
        {
            meshQuads.Add(verts);
            meshQuads.Add(verts + 1);
            meshQuads.Add(verts + 2);
            meshQuads.Add(verts + 3);
        }
    }

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
    /// Regenerates mesh.
    /// </summary>
    /// <remarks>
    /// The origin of the updates must be from within the chunk.
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

    //Debug only
    void OnDrawGizmos()
    {
        if (voxels == null || meshFilter.mesh == null) return;
        //Outline the whole chunk
        Gizmos.color = Color.black;
        //Gizmos.DrawCube(transform.position + new Vector3(size / 2, height / 2, size / 2), new Vector3(size, height, size));
        Gizmos.DrawWireMesh(meshFilter.mesh, transform.position);
    }
}
