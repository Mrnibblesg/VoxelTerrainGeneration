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
        //All deallocated by job when finished
        NativeArray<Voxel> voxels = new((size + 2) * (height + 2) * (size + 2), Allocator.TempJob);

        NativeArray<Voxel> orig = VoxelRun.ToFlatNativeArray(c.voxels, size, height);

        NativeArray<Voxel> up = VoxelRun.ToFlatNativeArray(c.neighbors[0]?.voxels, size, height);
        NativeArray<Voxel> down = VoxelRun.ToFlatNativeArray(c.neighbors[1]?.voxels, size, height);
        NativeArray<Voxel> left = VoxelRun.ToFlatNativeArray(c.neighbors[2]?.voxels, size, height);
        NativeArray<Voxel> right = VoxelRun.ToFlatNativeArray(c.neighbors[3]?.voxels, size, height);
        NativeArray<Voxel> forward = VoxelRun.ToFlatNativeArray(c.neighbors[4]?.voxels, size, height);
        NativeArray<Voxel> back = VoxelRun.ToFlatNativeArray(c.neighbors[5]?.voxels, size, height);

        ChunkMeshJob chunkMeshJob = new()
        {
            size = size,
            height = height,
            resolution = c.parent.resolution,

            voxels = voxels,
            
            orig = orig,
            
            up = up,
            down = down,
            left = left,
            right = right,
            forward = forward,
            back = back,

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

        NativeArray<float3> nativeVerts = results.vertices.ToArray(Allocator.Temp);
        NativeArray<int> nativeQuads = results.quads.ToArray(Allocator.Temp);
        NativeArray<Color32> nativeColors = results.colors.ToArray(Allocator.Temp);

        newMesh.SetVertices(nativeVerts);
        newMesh.SetIndices(nativeQuads, MeshTopology.Quads, 0);
        newMesh.SetColors(nativeColors);

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
