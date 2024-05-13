using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelInteraction : MonoBehaviour
{
    public Camera playerCamera;
    private LookingAtVoxel looking;
    private VoxelType currType;
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        looking = gameObject.AddComponent<LookingAtVoxel>();
        player = GetComponent<Player>();
        currType = VoxelType.GRASS;
    }

    // Update is called once per frame
    void Update()
    {
        // type selection
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currType = VoxelType.GRASS;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currType = VoxelType.DIRT;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currType = VoxelType.STONE;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currType = VoxelType.GLASS;
        }

        // first person
        if (Input.GetKeyDown(KeyCode.F))
        {
            Vector3[] voxelInfo = looking.LookingAt(playerCamera);
            if (voxelInfo != null)
            {
                Vector3 position = voxelInfo[0] - (voxelInfo[1] / player.CurrentWorld.resolution / 2);
                Voxel? voxel = player.CurrentWorld.VoxelFromGlobal(position);
                if (voxel != null)
                {
                    player.TryBreak(position);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Vector3[] voxelInfo = looking.LookingAt(playerCamera);
            if (voxelInfo != null)
            {
                Vector3 position = voxelInfo[0] + (voxelInfo[1] / player.CurrentWorld.resolution / 2);
                Voxel? voxel = player.CurrentWorld.VoxelFromGlobal(position);
                if (voxel != null && ((Voxel)voxel).type == VoxelType.AIR)
                {
                    player.TryPlace(position, currType);
                }
            }
        }

        // third person
        if (Input.GetMouseButtonDown(0))
        {
            Vector3[] voxelInfo = looking.ClickedVoxel(playerCamera);
            if (voxelInfo != null)
            {
                Vector3 position = voxelInfo[0] - (voxelInfo[1] / player.CurrentWorld.resolution / 2);
                Voxel? voxel = player.CurrentWorld.VoxelFromGlobal(position);
                if (voxel != null)
                {
                    player.TryBreak(position);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3[] voxelInfo = looking.ClickedVoxel(playerCamera);
            if (voxelInfo != null)
            {
                Vector3 position = voxelInfo[0] + (voxelInfo[1] / player.CurrentWorld.resolution / 2);
                Voxel? voxel = player.CurrentWorld.VoxelFromGlobal(position);
                if (voxel != null && ((Voxel)voxel).type == VoxelType.AIR)
                {
                    player.TryPlace(position, currType);
                }
            }
        }
    }
}
