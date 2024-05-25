using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
public struct ChunkMeshJob : IJob
{

    [ReadOnly]
    public int size;
    public int height;
    public float resolution;
    public NativeArray<Voxel> voxels;

    [WriteOnly]
    public NativeList<float3> vertices;
    public NativeList<int> quads;
    public NativeList<Color32> colors;

    //We aren't allowed to read from vertices, so we keep track of its length on our own
    private int verticesLength;

    public void Execute()
    {
        verticesLength = 0;
        GreedyMeshing();
    }

    //index into the given voxels. Voxels are currently given in a flat array,
    //and if you imagine the 3d representation, the current chunk's voxels are
    //spaced out from the edges by 1 space. The first layer of the neighboring
    //chunks fill the space between the actual chunk and the edge of the array.
    private Voxel voxel(int x, int y, int z)
    {
        return voxels[(x + 1) * (height+2) * (size+2) + (y + 1) * (size + 2) + (z+1)];
    }
    private bool outsideChunk(int x, int y, int z)
    {
        return (x < 0 || x >= size ||
                y < 0 || y >= height ||
                z < 0 || z >= size);
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
        //Create arrays once & reuse
        //stores distinct types of vertices to
        //turn into quads on current slice
        NativeArray<int> mask = new(Mathf.Max(size*size, size*height), Allocator.Temp);

        //dimensions
        NativeArray<int> dims = new(3, Allocator.Temp);
        dims[0] = size;
        dims[1] = height;
        dims[2] = size;

        //stores progress through the volume.
        //each element is basically used as an iterator for that dimension.
        NativeArray<int> progress = new(3, Allocator.Temp);

        //Add each element to your position to move up 1 slice
        NativeArray<int> normOff = new(3, Allocator.Temp);

        //We apply the w and h in the correct dimension
        //according to our current axes so we can properly
        //calculate our quad coordinates.
        NativeArray<int> uOff = new(3, Allocator.Temp);
        NativeArray<int> vOff = new(3, Allocator.Temp);

        //Iterate over the 3 axes.
        //a variable representing an axis will change each iteration.

        for (int normal = 0; normal < 3; normal++)
        {
            //The other 2 axes of the current slice direction.
            //Not the same as texture mapping UVs.
            int u = (normal + 1) % 3;
            int v = (normal + 2) % 3;


            progress[0] = 0;
            progress[1] = 0;
            progress[2] = 0;

            
            normOff[normal] = 1;
            normOff[(normal+1)%3] = 0;
            normOff[(normal+2)%3] = 0;


            //Compute mask for this slice
            for (progress[normal] = -1; progress[normal] < dims[normal];)
            {

                int n = 0;
                for (progress[u] = 0; progress[u] < dims[u]; progress[u]++)
                {
                    for (progress[v] = 0; progress[v] < dims[v]; progress[v]++, n++)
                    {
                        //Bounds checking/voxel type checking
                        //Make sure to check the adjacent chunk if our needed voxel is outside this one
                        //voxels below and above the current slice
                        VoxelType below = voxel(
                            progress[0],
                            progress[1],
                            progress[2]).type;

                        VoxelType above = voxel(
                            progress[0] + normOff[0],
                            progress[1] + normOff[1],
                            progress[2] + normOff[2]).type;

                        //no face if they're both a voxel or if the're both air
                        //no face if the solid block is in the neighbor chunk
                        //We need the last 2 checks to avoid overdraw.
                        if ((above == VoxelType.AIR) ==
                            (below == VoxelType.AIR) || 
                            (below != VoxelType.AIR && outsideChunk(progress[0], progress[1], progress[2])) ||
                            (above != VoxelType.AIR && outsideChunk(progress[0] + normOff[0], progress[1] + normOff[1], progress[2] + normOff[2])))
                        {
                            mask[progress[u] * dims[v] + progress[v]] = 0;
                        }
                        else if (below != VoxelType.AIR)
                        {
                            mask[progress[u] * dims[v] + progress[v]] = (int)below;
                        }
                        else
                        {
                            //A negative value means that the face is facing the
                            //opposite direction.
                            mask[progress[u] * dims[v] + progress[v]] = -(int)above;
                        }
                    }
                }
                progress[normal]++;

                //Create quads from the mask
                for (int j = 0; j < dims[v]; j++)
                {
                    for (int i = 0; i < dims[u];)
                    {
                        int current = mask[i * dims[v] + j];

                        if (current == 0)
                        {
                            i++;
                            continue;
                        }

                        //Calculate width of quad
                        int w;
                        for (w = 1; i + w < dims[u] && current == mask[(i + w)*dims[v] + j]; w++) { }

                        int h;
                        //Calculate height of quad
                        for (h = 1; j + h < dims[v]; h++)
                        {
                            for (int k = 0; k < w; k++)
                            {
                                if (current != mask[(i + k)*dims[v] + j + h])
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
                        
                        uOff[u] = w;
                        uOff[(u + 1) % 3] = 0;
                        uOff[(u + 2) % 3] = 0;

                        vOff[v] = h;
                        vOff[(v + 1) % 3] = 0;
                        vOff[(v + 2) % 3] = 0;

                        AddQuad(current, progress, uOff, vOff);

                        //Mark the section of mask that the quad occupied as done.
                        for (int k = 0; k < w; k++)
                        {
                            for (int l = 0; l < h; l++)
                            {
                                mask[(i + k) * dims[v] + j + l] = 0;
                            }
                        }
                        i += w;
                    }
                }
            }
        }
        mask.Dispose();
        progress.Dispose();
        dims.Dispose();
        normOff.Dispose();
        uOff.Dispose();
        vOff.Dispose();
    }

    /// <summary>
    /// Add a quad to the current list of
    /// mesh vertices, colors, and quad indices.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="pos"></param>
    /// <param name="uOff"></param>
    /// <param name="vOff"></param>
    private void AddQuad(int type, NativeArray<int> pos, NativeArray<int> uOff, NativeArray<int> vOff)
    {
        bool CCW = type < 0;
        if (type < 0) type *= -1;
        Color32 col = ((VoxelType)type).getVoxelAttributes().color;

        vertices.Add( //Bottom left corner
            new float3(
            pos[0],
            pos[1],
            pos[2]) / resolution);

        vertices.Add( //Bottom right corner
            new float3(
            pos[0] + uOff[0],
            pos[1] + uOff[1],
            pos[2] + uOff[2]) / resolution);

        vertices.Add( // Top right corner
            new float3(
            pos[0] + uOff[0] + vOff[0],
            pos[1] + uOff[1] + vOff[1],
            pos[2] + uOff[2] + vOff[2]) / resolution);

        vertices.Add( // Top left corner
            new float3(
            pos[0] + vOff[0],
            pos[1] + vOff[1],
            pos[2] + vOff[2]) / resolution);

        colors.Add(col);
        colors.Add(col);
        colors.Add(col);
        colors.Add(col);
        if (CCW)
        {
            quads.Add(verticesLength + 3);
            quads.Add(verticesLength + 2);
            quads.Add(verticesLength + 1);
            quads.Add(verticesLength);
        }
        else
        {
            quads.Add(verticesLength);
            quads.Add(verticesLength + 1);
            quads.Add(verticesLength + 2);
            quads.Add(verticesLength + 3);
        }
        verticesLength += 4;
    }
}
