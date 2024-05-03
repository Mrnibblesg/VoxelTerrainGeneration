using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 10f;
    public Camera playerCamera;
    private LookingAtVoxel looking;

    private void Start()
    {
        WorldGenerator w = WorldGenerator.World;
        Vector3 startPosition = new(0.5f, w.worldHeight * w.chunkHeight + 1.5f, 0.5f); 
        transform.position = startPosition;
        looking = gameObject.AddComponent<LookingAtVoxel>();
    }
    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.position += new Vector3(horizontal, 0f, vertical) * Time.deltaTime * speed;

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

        if (Input.GetKeyDown(KeyCode.T))
        {
            Vector3[] voxelInfo = looking.LookingAt(playerCamera);
            if (voxelInfo != null)
            {
                var position = voxelInfo[0] + (voxelInfo[1] / 2);
                Voxel? voxel = WorldGenerator.World.GetVoxel(position);
                if (voxel != null && ((Voxel)voxel).type == VoxelType.Type.AIR)
                {
                    WorldGenerator.World.SetVoxel(position, (Voxel)voxel?.SetType(VoxelType.Type.DIRT));
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