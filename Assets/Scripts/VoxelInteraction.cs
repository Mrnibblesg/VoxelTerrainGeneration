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
    private int breakRange;

    // Start is called before the first frame update
    void Start()
    {
        looking = gameObject.AddComponent<LookingAtVoxel>();
        player = GetComponent<Player>();
        currType = VoxelType.GRASS;
        voxelInfo = null;
        breakRange = 2;
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
            breakRange++;
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            breakRange--;
        }
    }

    private void FirstPerson()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            voxelInfo = looking.LookingAt(playerCamera);
            if (voxelInfo != null)
            {
                position = voxelInfo[0] - (voxelInfo[1] / player.CurrentWorld.parameters.Resolution / 2);
                BreakVoxel(position);
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            voxelInfo = looking.LookingAt(playerCamera);
            if (voxelInfo != null)
            {
                position = voxelInfo[0] + (voxelInfo[1] / player.CurrentWorld.parameters.Resolution / 2);
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
                position = voxelInfo[0] - (voxelInfo[1] / player.CurrentWorld.parameters.Resolution / 2);
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
                            TwoPointReplace((Vector3)alt_position, position, VoxelType.AIR);
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
                position = voxelInfo[0] + (voxelInfo[1] / player.CurrentWorld.parameters.Resolution / 2);

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
                            // TwoPointPlace((Vector3)alt_position, position);
                            alt_position = null;
                        }
                    }
                    else
                    {
                        MassPlace(position, currType);
                    }
                }
                else
                {
                    PlaceVoxel(position);
                }
            }
        }
    }

    private void BreakVoxel(Vector3 position)
    {
        player.TryBreak(position);
    }

    private void PlaceVoxel(Vector3 position)
    {
        player.TryPlace(position, currType);
    }

    private void MassBreak(Vector3 position)
    {

        MassPlace(position, VoxelType.AIR);
    }

    private void MassPlace(Vector3 position, VoxelType type)
    {
        List<Vector3> positions = new List<Vector3>();

        float voxelSize = 1 / player.CurrentWorld.parameters.Resolution;
        Vector3 offset = new Vector3(-voxelSize * breakRange, -voxelSize * breakRange, -voxelSize * breakRange);
        Vector3 p1 = position - offset;
        Vector3 p2 = position + offset;

        TwoPointReplace(p1, p2, type);
    }

    /// <summary>
    /// Corrects the given positions before attempting the replacement
    /// </summary>
    private void TwoPointReplace(Vector3 altPos, Vector3 pos, VoxelType type)
    {

        float startX;
        float startY;
        float startZ;
        float endX;
        float endY;
        float endZ;

        if (pos.x <= altPos.x)
        {
            startX = pos.x;
            endX = altPos.x;
        }
        else
        {
            startX = altPos.x;
            endX = pos.x;
        }

        if (pos.y <= altPos.y)
        {
            startY = pos.y;
            endY = altPos.y;
        }
        else
        {
            startY = altPos.y;
            endY = pos.y;
        }

        if (pos.z <= altPos.z)
        {
            startZ = pos.z;
            endZ = altPos.z;
        }
        else
        {
            startZ = altPos.z;
            endZ = pos.z;
        }


        player.TryTwoPointReplace(
            new Vector3(startX, startY, startZ),
            new Vector3(endX, endY, endZ),
            type);

    }
}
