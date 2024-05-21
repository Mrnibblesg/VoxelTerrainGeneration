using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ChunkMeshGenerator
{
    //The stuff you want back when the job finishes. All else is lost
    private struct JobData
    {
        public Chunk requester;
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
            vertices = new(Allocator.Persistent),
            quads = new(Allocator.Persistent),
            colors = new(Allocator.Persistent)
        };

        //flatten the voxel array
        NativeArray<Voxel> flattened = new(size * height * size, Allocator.Persistent);
        for (int i = 0; i < flattened.Length; i++)
        {
            flattened[i] = c.voxels[
                i / (height * size),
                (i / size) % height,
                i % size];
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

        JobManager.Manager.addJob(chunkMeshJob.Schedule(), finishNewMesh, jobData);
    }
    public static void finishNewMesh(object raw)
    {
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
        NativeArray<float3> nativeVerts = results.vertices.ToArray(Allocator.Persistent);
        NativeArray<int> nativeQuads = results.quads.ToArray(Allocator.Persistent);
        NativeArray<Color32> nativeColors = results.colors.ToArray(Allocator.Persistent);

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
        results.requester.ApplyNewMesh(newMesh);
        
        nativeVerts.Dispose();
        nativeQuads.Dispose();
        nativeColors.Dispose();
        results.quads.Dispose();
        results.vertices.Dispose();
        results.colors.Dispose();
    }

}
