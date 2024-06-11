using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct ChunkMessage
{
    public Vector3Int ChunkCoords;
    public VoxelRun Voxels;
    public string WorldName;
}