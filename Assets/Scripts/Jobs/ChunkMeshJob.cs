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

    //Passed in
    [ReadOnly]
    public int size;
    public int height;
    public float resolution;
    
    [ReadOnly]
    public NativeList<VoxelRun.Pair<Voxel, int>> orig;

    [ReadOnly]
    public NativeList<VoxelRun.Pair<Voxel, int>> up;

    [ReadOnly]
    public NativeList<VoxelRun.Pair<Voxel, int>> down;

    [ReadOnly]
    public NativeList<VoxelRun.Pair<Voxel, int>> left;

    [ReadOnly]
    public NativeList<VoxelRun.Pair<Voxel, int>> right;

    [ReadOnly]
    public NativeList<VoxelRun.Pair<Voxel, int>> forward;

    [ReadOnly]
    public NativeList<VoxelRun.Pair<Voxel, int>> back;

    [WriteOnly]
    public NativeList<float3> vertices;

    [WriteOnly]
    public NativeList<int> quads;

    [WriteOnly]
    public NativeList<Color32> colors;

    //This object only
    private int verticesLength;


    public void Execute()
    {
        //allocate voxels here, pass them to the other functions
        NativeArray<Voxel> voxels = new((size + 2) * (height + 2) * (size + 2), Allocator.Temp);
        verticesLength = 0;
        //convert nativelists to decompressed native arrays

        BuildExpandedChunk(voxels);
        
        GreedyMeshing(voxels);
        voxels.Dispose();
    }
    public void BuildExpandedChunk(NativeArray<Voxel> voxels)
    {
        void DecompressToArray(NativeList<VoxelRun.Pair<Voxel, int>> list, NativeArray<Voxel> array)
        {
            int i = 0;
            for (int node = 0; node < list.Length; node++)
            {
                VoxelRun.Pair<Voxel, int> current = list[node];
                for (int run = 0; run < current.value; run++, i++)
                {
                    array[i] = current.key;
                }
            }
        }
        //Convert native lists to arrays
        NativeArray<Voxel> origArr = new(size * height * size, Allocator.Temp);

        NativeArray<Voxel> upArr = new(size * height * size, Allocator.Temp);
        NativeArray<Voxel> downArr = new(size * height * size, Allocator.Temp);
        NativeArray<Voxel> leftArr = new(size * height * size, Allocator.Temp);
        NativeArray<Voxel> rightArr = new(size * height * size, Allocator.Temp);
        NativeArray<Voxel> forwardArr = new(size * height * size, Allocator.Temp);
        NativeArray<Voxel> backArr = new(size * height * size, Allocator.Temp);
        
        DecompressToArray(orig, origArr);
        DecompressToArray(up, upArr);
        DecompressToArray(down, downArr);
        DecompressToArray(left, leftArr);
        DecompressToArray(right, rightArr);
        DecompressToArray(forward, forwardArr);
        DecompressToArray(back, backArr);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    voxels[(x + 1) * (height + 2) * (size + 2) + (y + 1) * (size + 2) + (z + 1)] = origArr[x * height * size + y * size + z];
                }
            }
        }

        //Copy the top/bottom neighbor chunk slices
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                //set the top of the input data as the bottom of the above chunk
                voxels[(x + 1) * (height + 2) * (size + 2) + (height + 1) * (size + 2) + (z + 1)] =
                    upArr[x * height * size + z];

                //set bottom of input as top of below chunk
                voxels[(x + 1) * (height + 2) * (size + 2) + (z + 1)] =
                    downArr[x * height * size + (height-1) * size + z];
            }
        }
        //left/right
        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < size; z++)
            {
                //Set the left of the input chunk as the right slice of the left chunk
                voxels[(y + 1) * (size + 2) + (z + 1)] =
                    leftArr[(size-1) * height * size + y * size + z];
                //Set the right of the input as the left slice of the right chunk.
                voxels[(size + 1) * (height + 2) * (size + 2) + (y + 1) * (size + 2) + (z + 1)] =
                    rightArr[y * size + z];
            }
        }
        //forward/back
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Set the back of the input as the forward of the back chunk
                voxels[(x + 1) * (height + 2) * (size + 2) + (y + 1) * (size + 2) + size + 1] =
                    forwardArr[x * height * size + y * size];
                //Set the forward of the input as the back of the forward
                voxels[(x + 1) * (height + 2) * (size + 2) + (y + 1) * (size + 2)] =
                    backArr[x * height * size + y * size + (size-1)];
            }
        }

        origArr.Dispose();
        upArr.Dispose();
        downArr.Dispose();
        leftArr.Dispose();
        rightArr.Dispose();
        forwardArr.Dispose();
        backArr.Dispose();
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
    private void GreedyMeshing(NativeArray<Voxel> voxels)
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
                        //orig below and above the current slice

                        //Voxels are currently given in a flat array,
                        //and if you imagine the 3d representation, the current chunk's orig are
                        //spaced out from the edges by 1 space. The first layer of the neighboring
                        //chunks fill the space between the actual chunk and the edge of the array.
                        //With the given formula, the indices of the first position can be imagined as (-1,-1,-1).
                        //Formula to properly access the array:
                        //voxels[(x + 1) * (height + 2) * (size + 2) + (y + 1) * (size + 2) + (z + 1)];

                        VoxelType below = voxels[
                            (progress[0] + 1) * (height + 2) * (size + 2) +
                            (progress[1] + 1) * (size + 2) +
                            (progress[2] + 1)].type;

                        VoxelType above = voxels[
                            (progress[0] + normOff[0] + 1) * (height + 2) * (size + 2) +
                            (progress[1] + normOff[1] + 1) * (size + 2) +
                            (progress[2] + normOff[2] + 1)].type;

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
