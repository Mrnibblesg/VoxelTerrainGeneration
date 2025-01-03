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
        int size = c.world.parameters.ChunkSize;
        int height = c.world.parameters.ChunkHeight;

        //Check if this whole chunk is air. If so, don't bother creating a job
        if (c.voxels.type.type == VoxelType.AIR &&
            c.voxels.runLength == size * height * size)
        {
            c.ApplyNewMesh(new Mesh(), DateTime.Now.Ticks);
            return;
        }

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
        

        NativeList<VoxelRun.Pair<Voxel, int>> orig = VoxelRun.ToNativeList(c.voxels, size, height);

        NativeList<VoxelRun.Pair<Voxel, int>> up = VoxelRun.ToNativeList(c.neighbors[0]?.voxels, size, height);
        NativeList<VoxelRun.Pair<Voxel, int>> down = VoxelRun.ToNativeList(c.neighbors[1]?.voxels, size, height);
        NativeList<VoxelRun.Pair<Voxel, int>> left = VoxelRun.ToNativeList(c.neighbors[2]?.voxels, size, height);
        NativeList<VoxelRun.Pair<Voxel, int>> right = VoxelRun.ToNativeList(c.neighbors[3]?.voxels, size, height);
        NativeList<VoxelRun.Pair<Voxel, int>> forward = VoxelRun.ToNativeList(c.neighbors[4]?.voxels, size, height);
        NativeList<VoxelRun.Pair<Voxel, int>> back = VoxelRun.ToNativeList(c.neighbors[5]?.voxels, size, height);

        ChunkMeshJob chunkMeshJob = new()
        {
            size = size,
            height = height,
            resolution = c.world.parameters.Resolution,
            
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
        JobHandle handle = chunkMeshJob.Schedule();

        JobManager.Manager.AddJob(handle, finishNewMesh, jobData);

        //DeallocateOnJobCompletion only works on NativeArrays, not NativeLists.
        //Hopefully this doesn't go on the main thread but it probably does :(
        orig.Dispose(handle);

        up.Dispose(handle);
        down.Dispose(handle);
        left.Dispose(handle);
        right.Dispose(handle);
        forward.Dispose(handle);
        back.Dispose(handle);

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
