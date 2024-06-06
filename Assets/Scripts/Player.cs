using UnityEngine;
using System.Collections.Generic;

public class Player : AuthoritativeAgent
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

    public override World CurrentWorld {
        get => currentWorld;
        set
        {
            this.currentWorld?.UnloadAll();
            this.currentWorld = value;
            Vector3 startPosition = new(
                0.5f,
                value.parameters.WorldHeightInChunks * value.parameters.ChunkHeight / value.parameters.Resolution + 1.5f,
                0.5f
            );
            transform.position = startPosition;

            UpdateChunkCoord();
        }
    }

    public void Start()
    {
        this.playerCamera = GetComponentInChildren<Camera>();

        this.mouseX = transform.eulerAngles.y;
        this.mouseY = playerCamera.transform.eulerAngles.x;
        this.mouseY = playerCamera.transform.eulerAngles.x;
        this.RenderDist = 15;
        this.UnloadDist = 16;
        
    }

    private void FixedUpdate()
    {
        UpdateMove();
    }

    public override void Update()
    {
        base.Update();
        if (Input.GetMouseButton(2))
        {
            UpdateLook();
        }
        if ((currentWorld.VoxelFromGlobal(transform.position)?.type ?? VoxelType.AIR) != VoxelType.AIR)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            transform.position += Vector3.up;
        }
        UpdateTeleport();
    }
    /// <summary>
    /// Teleport the player when shift and left mouse button are pressed.
    /// </summary>
    void UpdateTeleport()
    {
        if (Input.GetKey(KeyCode.T))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Teleport(hit.point);
            }
        }
    }

    /// <summary>
    /// Teleport the player to the specified position.
    /// </summary>
    void Teleport(Vector3 position)
    {
        transform.position = position + Vector3.up; // Add a little offset to avoid sinking into the ground
        GetComponent<Rigidbody>().velocity = Vector3.zero; // Reset velocity to prevent carrying over momentum
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

        Vector3 up = new();
        if (Input.GetAxis("Jump") != 0)
        {
            up = Vector3.up * 45;
        }

        Move((forward + right).normalized * combinedMultiplier, up);

        
    }
    /// <summary>
    /// Move the player. The player has a rigidbody, so we override it and use
    /// the rigidbody.
    /// </summary>
    public override void Move(Vector3 offset, Vector3 force = new())
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.MovePosition(transform.position + offset);
        rb.AddForce(force);
    }
}