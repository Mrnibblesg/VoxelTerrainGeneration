using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System;
using Unity.Profiling;

public class ChunkMeshGenerator
{
    private static ProfilerMarker s_chunkFinish = new ProfilerMarker(ProfilerCategory.Render, "Finish chunk");
    //The stuff you want back when the job finishes. All else is lost
    private struct JobData
    {
        public Chunk requester;
        public long requestTime;
        public NativeList<float3> vertices;
        public NativeList<int> quads;
        public NativeList<Color32> colors;
    };
    public static void RequestNewMesh(Chunk c)
    {
        int size = c.parent.chunkSize;
        int height = c.parent.chunkHeight;
        JobData jobData = new()
        {
            requester = c,
            requestTime = DateTime.Now.Ticks,
            vertices = new(Allocator.TempJob),
            quads = new(Allocator.TempJob),
            colors = new(Allocator.TempJob)
        };

        //flatten the voxel array, make space for the first layer of neighboring chunks
        NativeArray<Voxel> flattened = new((size + 2) * (height + 2) * (size + 2), Allocator.TempJob);
        Voxel[,,] arr = VoxelRun.toArray(c.voxels, size, height);

        Voxel[,,] up = VoxelRun.toArray(c.neighbors[0]?.voxels, size, height);
        Voxel[,,] down = VoxelRun.toArray(c.neighbors[1]?.voxels, size, height);
        Voxel[,,] left = VoxelRun.toArray(c.neighbors[2]?.voxels, size, height);
        Voxel[,,] right = VoxelRun.toArray(c.neighbors[3]?.voxels, size, height);
        Voxel[,,] forward = VoxelRun.toArray(c.neighbors[4]?.voxels, size, height);
        Voxel[,,] back = VoxelRun.toArray(c.neighbors[5]?.voxels, size, height);

        //Place the original chunk data in the center of the flat array.
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    flattened[(x + 1) * (height+2) * (size+2) + (y + 1) * (size + 2) + (z + 1)] = arr[x,y,z];
                }
            }
        }

        //Copy the top/bottom neighbor chunk slices
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                //set the top of the input data as the bottom of the above chunk
                flattened[(x+1) * (height+2) * (size+2) + (height+1) * (size+2) + (z+1)] = 
                    up[x, 0, z];

                //set bottom of input as top of below chunk
                flattened[(x + 1) * (height + 2) * (size + 2) + (z + 1)] =
                    down[x, height - 1, z];
            }
        }
        //left/right
        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < size; z++)
            {
                //Set the left of the input chunk as the right slice of the left chunk
                flattened[(y + 1) * (height + 2) + (z + 1)] =
                    left[size - 1, y, z];
                //Set the right of the input as the left slice of the right chunk.
                flattened[(size + 1) * (height + 2) * (size + 2) + (y + 1) * (size + 2) + (z + 1)] =
                    right[0, y, z];
            }
        }
        //front/back
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Set the back of the input as the front of the back chunk
                flattened[(x + 1) * (height + 2) * (size + 2) + (y + 1) * (size + 2) + size + 1] =
                    forward[x, y, 0];
                //Set the front of the input as the back of the front
                flattened[(x + 1) * (height + 2) * (size + 2) + (y + 1) * (size + 2)] =
                    back[x, y, size - 1];
            }
        }

        ChunkMeshJob chunkMeshJob = new()
        {
            size = size,
            height = height,
            resolution = c.parent.resolution,
            voxels = flattened,

            vertices = jobData.vertices,
            quads = jobData.quads,
            colors = jobData.colors,
        };

        JobManager.Manager.AddJob(chunkMeshJob.Schedule(), finishNewMesh, jobData);
    }
    public static void finishNewMesh(object raw)
    {
        s_chunkFinish.Begin();
        JobData results = (JobData)raw;
        if (results.requester == null)
        {
            results.quads.Dispose();
            results.vertices.Dispose();
            results.colors.Dispose();
            return;
        }

        Mesh newMesh = new();
        newMesh.name = results.requester.name;

        //Unfortunately there is no direct conversion from NativeList to a managed array. TODO. This is currently inefficient. Death by 1000 cuts type-beat. ya know.
        NativeArray<float3> nativeVerts = results.vertices.ToArray(Allocator.TempJob);
        NativeArray<int> nativeQuads = results.quads.ToArray(Allocator.TempJob);
        NativeArray<Color32> nativeColors = results.colors.ToArray(Allocator.TempJob);

        Vector3[] vecVerts = new Vector3[nativeVerts.Length];
        for (int i = 0; i < nativeVerts.Length; i++)
        {
            vecVerts[i] = nativeVerts[i];
        }

        newMesh.vertices = vecVerts;
        newMesh.colors32 = nativeColors.ToArray();
        //newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        newMesh.SetIndices(nativeQuads.ToArray(), MeshTopology.Quads, 0);

        newMesh.RecalculateNormals();
        results.requester.ApplyNewMesh(newMesh, results.requestTime);
        
        nativeVerts.Dispose();
        nativeQuads.Dispose();
        nativeColors.Dispose();
        results.quads.Dispose();
        results.vertices.Dispose();
        results.colors.Dispose();
        s_chunkFinish.End();
    }

}
