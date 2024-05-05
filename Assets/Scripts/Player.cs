using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 10f;

    private Vector3Int currentChunkCoord;

    //Render distance in chunks
    private int renderDist = 3;
    private int unloadDist = 4;

    //TODO more sophisticated get and set for potential world switching.

    //Until we can ensure that the player's world is set before the player
    //becomes active, we must always use the null-conditional operator ?. with it.
    private World currentWorld;
    public World CurrentWorld {
        get { return this.currentWorld; }
        set
        {
            this.currentWorld = value;
            Vector3 startPosition = new(
                0.5f,
                value.worldHeight * value.chunkHeight + 1.5f,
                0.5f
            );
            transform.position = startPosition;

            UpdateChunkCoord();
        }
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.position += new Vector3(horizontal, 0f, vertical) * Time.deltaTime * speed;
        // playerCamera.transform.LookAt(gameObject.transform);

        UpdateChunkCoord(); //TODO uncomment, only commented for testing
    }

    /// <summary>
    /// Calculates the player's current chunk coordinate. If it changes,
    /// then we notify the player's current world that it happened.
    /// </summary>
    private void UpdateChunkCoord()
    {
        Vector3Int chunkCoord = new(
            Mathf.FloorToInt(transform.position.x / currentWorld.chunkSize),
            Mathf.FloorToInt(transform.position.y / currentWorld.chunkHeight),
            Mathf.FloorToInt(transform.position.z / currentWorld.chunkSize)
        );
        if (currentChunkCoord != chunkCoord)
        {
            currentChunkCoord = chunkCoord;
            currentWorld?.UpdateNearbyChunks(currentChunkCoord, renderDist, unloadDist);
        }
    }

    /// <summary>
    /// Attempt to break a block in the current world, at world-space position.
    /// </summary>
    public void TryBreak(Vector3 pos)
    {
        currentWorld?.SetVoxel(pos, VoxelType.AIR);
    }
    /// <summary>
    /// Attempt to place a block in the current world, at world-space position.
    /// </summary>
    public void TryPlace(Vector3 pos, VoxelType type)
    {
        currentWorld?.SetVoxel(pos, type);
    }
}