using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 10f;

    private void Start()
    {
        WorldGenerator w = WorldGenerator.World;
        Vector3 startPosition = new(0.5f, w.worldHeight * w.chunkHeight + 1.5f, 0.5f);
        transform.position = startPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.position += new Vector3(horizontal, 0f, vertical) * Time.deltaTime * speed;
        // playerCamera.transform.LookAt(gameObject.transform);


        if (Input.GetKeyDown(KeyCode.F))
        {
            var position = new Vector3(transform.position.x, transform.position.y - 1.5f, transform.position.z);
            Voxel? voxel = WorldGenerator.World.GetVoxel(position);
            if (voxel != null)
            {
                WorldGenerator.World.SetVoxel(position, (Voxel)voxel?.SetType(Block.AIR));
            }

        }
    }

   
}