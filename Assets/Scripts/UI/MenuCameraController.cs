using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    public float speed = 2.0f;

    private World currentWorld;
    public World CurrentWorld {
        get { return this.currentWorld; }
        set
        {
            this.currentWorld = value;
            UpdateChunkCoord();
        }
    }
    private Vector3Int currentChunkCoord;
    private int renderDist = 7;
    private int unloadDist = 8;

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        UpdateChunkCoord();
    }

    private void UpdateChunkCoord()
    {
        Vector3Int chunkCoord = new(
            Mathf.FloorToInt(transform.position.x / (currentWorld.chunkSize / currentWorld.resolution)),
            Mathf.FloorToInt(transform.position.y / (currentWorld.chunkHeight / currentWorld.resolution)),
            Mathf.FloorToInt(transform.position.z / (currentWorld.chunkSize / currentWorld.resolution))
        );
        if (currentChunkCoord != chunkCoord)
        {
            currentChunkCoord = chunkCoord;
            currentWorld?.UpdatePlayerChunkPos(currentChunkCoord, renderDist, unloadDist);
        }
    }
}
