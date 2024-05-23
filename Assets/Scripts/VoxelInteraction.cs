using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelInteraction : MonoBehaviour
{
    public Camera playerCamera;
    private LookingAtVoxel looking;
    private VoxelType currType;
    private Player player;
    private Vector3[] voxelInfo;
    private Vector3 position;
    Vector3? alt_position;
    private int breakCoefficient;

    // Start is called before the first frame update
    void Start()
    {
        looking = gameObject.AddComponent<LookingAtVoxel>();
        player = GetComponent<Player>();
        currType = VoxelType.GRASS;
        voxelInfo = null;
        breakCoefficient = 2;
        alt_position = null;
    }

    // Update is called once per frame
    void Update()
    {
        TypeSelection();

        FirstPerson();

        ThirdPerson();
    }

    private void TypeSelection()
    {
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

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            currType = VoxelType.WATER_SOURCE;
        }

        if (Input.GetKeyDown(KeyCode.Period))
        {
            breakCoefficient++;
            Debug.Log("Break Coefficient: " + breakCoefficient);
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            breakCoefficient--;
            Debug.Log("Break Coefficient: " + breakCoefficient);
        }
    }

    private void FirstPerson()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            voxelInfo = looking.LookingAt(playerCamera);
            if (voxelInfo != null)
            {
                position = voxelInfo[0] - (voxelInfo[1] / player.CurrentWorld.resolution / 2);
                BreakVoxel(position);
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            voxelInfo = looking.LookingAt(playerCamera);
            if (voxelInfo != null)
            {
                position = voxelInfo[0] + (voxelInfo[1] / player.CurrentWorld.resolution / 2);
                PlaceVoxel(position);
            }
        }
    }

    private void ThirdPerson()
    {
        if (Input.GetMouseButtonDown(0))
        {
            voxelInfo = looking.ClickedVoxel(playerCamera);
            if (voxelInfo != null)
            {
                position = voxelInfo[0] - (voxelInfo[1] / player.CurrentWorld.resolution / 2);
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        if (alt_position == null)
                        {
                            alt_position = position;
                        }
                        else
                        {
                            TwoPointBreak((Vector3)alt_position, position);
                        }
                    }
                    else
                    {
                        MassBreak(position);
                    }
                }
                else
                {
                    BreakVoxel(position);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            voxelInfo = looking.ClickedVoxel(playerCamera);
            if (voxelInfo != null)
            {
                position = voxelInfo[0] + (voxelInfo[1] / player.CurrentWorld.resolution / 2);
                PlaceVoxel(position);
            }
        }

        /*if (Input.GetMouseButtonDown(0))
        {
            voxelInfo = looking.ClickedVoxel(playerCamera);
            if (voxelInfo != null)
            {
                position = voxelInfo[0] + (voxelInfo[1] / player.CurrentWorld.resolution / 2);
                MassBreak(position);
            }
        }*/
    }

    private void BreakVoxel(Vector3 position)
    {
        player.TryBreak(position);
    }

    private void PlaceVoxel(Vector3 position)
    {
        Voxel? voxel = player.CurrentWorld.VoxelFromGlobal(position);
        if (voxel != null && ((Voxel)voxel).type == VoxelType.AIR)
        {
            player.TryPlace(position, currType);
        }
    }

    private void MassBreak(Vector3 position)
    {
        Debug.Log("In Mass Break");

        List<Vector3> positions = new List<Vector3>();

        float voxelSize = 1 / player.CurrentWorld.resolution;

        for (float i = 0; i <= voxelSize * breakCoefficient; i += voxelSize)
        {
            for (float j = 0; j <= voxelSize * breakCoefficient; j += voxelSize)
            {
                for (float k = 0; k <= voxelSize * breakCoefficient; k += voxelSize)
                {
                    positions.Add(position + new Vector3(i, j, k));
                    positions.Add(position + new Vector3(-i, j, k));
                    positions.Add(position + new Vector3(i, j, -k));
                    positions.Add(position + new Vector3(-i, j, -k));
                    positions.Add(position + new Vector3(i, -j, k));
                    positions.Add(position + new Vector3(-i, -j, k));
                    positions.Add(position + new Vector3(i, -j, -k));
                    positions.Add(position + new Vector3(-i, -j, -k));
                }
            }
        }

        player.TryBreakList(positions);
    }

    private void TwoPointBreak(Vector3 alt_pos, Vector3 pos)
    {
        Debug.Log("In Two Point Break");

        float voxelSize = 1 / player.CurrentWorld.resolution;

        for (float i = 0; i <= voxelSize * breakCoefficient; i += voxelSize)
        {
            Debug.Log("In the loop, i value is: " + i);
            for (float j = 0; j <= voxelSize * breakCoefficient; j += voxelSize)
            {
                for (float k = 0; k <= voxelSize * breakCoefficient; k += voxelSize)
                {
                    BreakVoxel(position + new Vector3(i, j, k));
                    BreakVoxel(position + new Vector3(-i, j, k));
                    BreakVoxel(position + new Vector3(i, j, -k));
                    BreakVoxel(position + new Vector3(-i, j, -k));
                    BreakVoxel(position + new Vector3(i, -j, k));
                    BreakVoxel(position + new Vector3(-i, -j, k));
                    BreakVoxel(position + new Vector3(i, -j, -k));
                    BreakVoxel(position + new Vector3(-i, -j, -k));
                }
            }
        }
    }
}
