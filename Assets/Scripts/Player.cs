using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public float speed = 10f;

    private float mouseX = 0f;
    
    [SerializeField]
    private float mouseSpeedX = 2f;

    private float mouseY = 0f;

    [SerializeField]
    private float mouseSpeedY = 2f;

    [SerializeField]
    private float minMouseY = -10f;
    
    [SerializeField]
    private float maxMouseY = 40f;

    private Camera playerCamera;

    private Vector3Int currentChunkCoord;

    //Render distance in chunks
    private int renderDist = 4;
    private int unloadDist = 5;

    //TODO more sophisticated get and set for potential world switching.

    //Until we can ensure that the player's world is set before the player
    //becomes active, we must always use the null-conditional operator ?. with it.
    private World currentWorld;
    public World CurrentWorld {
        get
        {
            return currentWorld;
        }
        set
        {
            this.currentWorld = value;
            Vector3 startPosition = new(
                0.5f,
                value.worldHeight * value.chunkHeight / value.resolution + 1.5f,
                0.5f
            );
            transform.position = startPosition;

            UpdateChunkCoord();
        }
    }

    private void Start()
    {
        this.playerCamera = GetComponentInChildren<Camera>();

        this.mouseX = transform.eulerAngles.y;
        this.mouseY = playerCamera.transform.eulerAngles.x;
        
        // Check in WorldAccessor for a world
        World world = WorldAccessor.Identify(this);

        if (world is null)
        {
            world = WorldAccessor.Join(this);
        }

        CurrentWorld = world;
    }

    private void FixedUpdate()
    {
        UpdateMove();
    }

    void Update()
    {
        if (Input.GetMouseButton(2))
        {
            UpdateLook();
        }
        if (Input.GetAxis("Jump") != 0)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 5);
        }

        if (this.CurrentWorld is not null)
            UpdateChunkCoord();
    }
    
    /// <summary>
    /// Camera look update. Called in Update().
    /// </summary>
    void UpdateLook()
    {
        var playerAngle = transform.eulerAngles;
        this.mouseX += mouseSpeedX * Input.GetAxis("Mouse X");
        this.transform.eulerAngles = new Vector3(playerAngle.x, mouseX, 0);

        var cameraAngle = playerCamera.transform.eulerAngles;
        this.mouseY -= mouseSpeedY * Input.GetAxis("Mouse Y");
        this.mouseY = Mathf.Clamp(mouseY, minMouseY, maxMouseY);

        this.playerCamera.transform.eulerAngles = new Vector3(mouseY, cameraAngle.y, 0);
    }

    /// <summary>
    /// Move update. Called in FixedUpdate().
    /// </summary>
    void UpdateMove()
    {
        float combinedMultiplier = speed * Time.fixedDeltaTime;

        Vector3 forward = Input.GetAxis("Vertical") * transform.forward;
        Vector3 right = Input.GetAxis("Horizontal") * transform.right;

        transform.position += (forward + right).normalized * combinedMultiplier;
    }

    /// <summary>
    /// Calculates the player's current chunk coordinate. If it changes,
    /// then we notify the player's current world that it happened.
    /// </summary>
    private void UpdateChunkCoord()
    {
        // Ensure JobManager is initialized before taking any actions
        if (JobManager.Manager is null)
        {
            return;
        }

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

    /// <summary>
    /// Attempt to break a block in the current world, at world-space position.
    /// </summary>
    public void TryBreak(Vector3 pos)
    {
        currentWorld?.SetVoxel(pos, VoxelType.AIR);
    }
    public void TryBreakList(List<Vector3> pos)
    {
        List<VoxelType> types = new List<VoxelType>();
        for (int i = 0; i < pos.Count; i++)
        {
            types.Add(VoxelType.AIR);
        }
        currentWorld?.SetVoxels(pos, types);
    }
    /// <summary>
    /// Attempt to place a block in the current world, at world-space position.
    /// </summary>
    public void TryPlace(Vector3 pos, VoxelType type)
    {
        currentWorld?.SetVoxel(pos, type);
    }
    public void TryPlaceList(List<Vector3> pos, List<VoxelType> types)
    {
        currentWorld?.SetVoxels(pos, types);
    }
}