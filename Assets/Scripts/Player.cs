using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 10f;
    public Camera playerCamera;
    private LookingAtVoxel looking;
    private VoxelType.Type currType;

    private void Start()
    {
        WorldGenerator w = WorldGenerator.World;
        Vector3 startPosition = new(0.5f, w.worldHeight * w.chunkHeight + 1.5f, 0.5f); 
        transform.position = startPosition;
        looking = gameObject.AddComponent<LookingAtVoxel>();
        currType = VoxelType.Type.GRASS;
    }
    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.position += new Vector3(horizontal, 0f, vertical) * Time.deltaTime * speed;

        float MouseX = Input.GetAxis("Mouse X");
        float MouseY = Input.GetAxis("Mouse Y");

        playerCamera.transform.eulerAngles += new Vector3(-MouseY, MouseX, 0);
        // playerCamera.transform.LookAt(gameObject.transform);

        CheckKeyPress();
    }

    private void CheckKeyPress()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Vector3[] voxelInfo = looking.LookingAt(playerCamera);
            if (voxelInfo != null)
            {
                var position = voxelInfo[0] - (voxelInfo[1] / 2);
                Voxel? voxel = WorldGenerator.World.GetVoxel(position);
                if (voxel != null)
                {
                    WorldGenerator.World.SetVoxel(position, (Voxel)voxel?.SetType(VoxelType.Type.AIR));
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currType = VoxelType.Type.GRASS;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currType = VoxelType.Type.DIRT;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currType = VoxelType.Type.STONE;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currType = VoxelType.Type.GLASS;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Vector3[] voxelInfo = looking.LookingAt(playerCamera);
            if (voxelInfo != null)
            {
                var position = voxelInfo[0] + (voxelInfo[1] / 2);
                Voxel? voxel = WorldGenerator.World.GetVoxel(position);
                if (voxel != null && ((Voxel)voxel).type == VoxelType.Type.AIR)
                {
                    WorldGenerator.World.SetVoxel(position, (Voxel)voxel?.SetType(currType));
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3[] voxelInfo = looking.LookingAt(playerCamera);
            Debug.Log("Looking at point: " + voxelInfo[0]);
            Debug.Log("Block Normal Vector: " + voxelInfo[1]);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(transform.position);
        }
    }
}