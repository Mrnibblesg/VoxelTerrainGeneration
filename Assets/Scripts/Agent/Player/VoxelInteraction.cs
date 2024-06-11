using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class VoxelInteraction : NetworkBehaviour
{
    public Camera playerCamera;
    private LookingAtVoxel looking;
    private VoxelType currentType;
    private Player player;
    private NetworkedPlayer networkedPlayer;
    private Vector3[] voxelInfo;
    private Vector3 position;
    Vector3? altPosition;
    Vector3? altPosition2;
    private int breakRange;

    // Start is called before the first frame update
    void Start()
    {
        looking = gameObject.AddComponent<LookingAtVoxel>();
        player = GetComponent<Player>();
        networkedPlayer = GetComponent<NetworkedPlayer>();
        currentType = VoxelType.GRASS;
        voxelInfo = null;
        breakRange = 2;
        altPosition = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.networkedPlayer.isLocalPlayer)
            return;

        if (NetworkedChatController.ChatController.IsPaused())
            return;

        TypeSelection();

        //We don't have a first person view yet
        //FirstPerson();

        ThirdPerson();
    }

    private void TypeSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentType = VoxelType.GRASS;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentType = VoxelType.DIRT;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentType = VoxelType.STONE;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            currentType = VoxelType.WATER_SOURCE;
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
                        if (altPosition == null)
                        {
                            altPosition = position;
                        }
                        else
                        {
                            TwoPointReplace((Vector3)altPosition, position, VoxelType.AIR);
                            altPosition = null;
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
                        if (altPosition2 == null)
                        {
                            altPosition2 = position;
                        }
                        else
                        {
                            TwoPointReplace((Vector3)altPosition2, position, currentType);
                            altPosition2 = null;
                        }
                    }
                    else
                    {
                        MassPlace(position, currentType);
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
        player.TryPlace(position, currentType);
    }

    private void MassBreak(Vector3 position)
    {

        MassPlace(position, VoxelType.AIR);
    }

    private void MassPlace(Vector3 position, VoxelType type)
    {
        float voxelSize = 1 / player.CurrentWorld.parameters.Resolution;
        Vector3 offset = new Vector3(-voxelSize * breakRange, -voxelSize * breakRange, -voxelSize * breakRange);
        Vector3 p1 = position - offset;
        Vector3 p2 = position + offset;

        TwoPointReplace(p1, p2, type);
    }

    /// <summary>
    /// Corrects the given positions before attempting the replacement
    /// </summary>
    private void TwoPointReplace(Vector3 posOne, Vector3 posTwo, VoxelType type)
    {
        Vector3 c1 = new Vector3( // Corner 1
            Mathf.Min(posOne.x, posTwo.x),
            Mathf.Min(posOne.y, posTwo.y),
            Mathf.Min(posOne.z, posTwo.z));
        Vector3 c2 = new Vector3( // Corner 2
            Mathf.Max(posOne.x, posTwo.x),
            Mathf.Max(posOne.y, posTwo.y),
            Mathf.Max(posOne.z, posTwo.z));

        player.TryTwoPointReplace(c1, c2, type);

    }
}
