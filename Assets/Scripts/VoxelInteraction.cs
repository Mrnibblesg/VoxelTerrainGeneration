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
                position.x = Mathf.Floor(position.x) + 0.5f;
                position.y = Mathf.Floor(position.y) + 0.5f;
                position.z = Mathf.Floor(position.z) + 0.5f;

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
                            alt_position = null;
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
        List<Vector3> positions = new List<Vector3>();

        float voxelSize = 1 / player.CurrentWorld.resolution;

        float start_x;
        float start_y;
        float start_z;
        float end_x;
        float end_y;
        float end_z;

        if (pos.x <= alt_pos.x)
        {
            start_x = pos.x;
            end_x = alt_pos.x;
        }
        else
        {
            start_x = alt_pos.x;
            end_x = pos.x;
        }

        if (pos.y <= alt_pos.y)
        {
            start_y = pos.y;
            end_y = alt_pos.y;
        }
        else
        {
            start_y = alt_pos.y;
            end_y = pos.y;
        }

        if (pos.z <= alt_pos.z)
        {
            start_z = pos.z;
            end_z = alt_pos.z;
        }
        else
        {
            start_z = alt_pos.z;
            end_z = pos.z;
        }

        for (float i = start_x; i <= end_x; i += voxelSize)
        {
            for (float j = start_y; j <= end_y; j += voxelSize)
            {
                for (float k = start_z; k <= end_z; k += voxelSize)
                {
                    positions.Add(new Vector3(i, j, k));
                }
            }
        }

        player.TryBreakList(positions);
    }
}
