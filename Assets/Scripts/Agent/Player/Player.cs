using UnityEngine;
using System.Collections.Generic;
using Mirror;
using TMPro;
using Unity.VisualScripting;

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

    public Camera Camera { get; private set; }

    public override World CurrentWorld
    {
        get => currentWorld;
        set
        {
            //this.currentWorld?.UnloadAll();
            
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

    private NetworkedPlayer networkedPlayer;


    public void Start()
    {
        this.Camera = transform.GetChild(0).GetComponent<Camera>();
        this.networkedPlayer = this.gameObject.GetComponent<NetworkedPlayer>();
        this.NetworkedAgent = this.networkedPlayer;

        if (!this.networkedPlayer.isLocalPlayer)
        {
            Camera.gameObject.SetActive(false);

            return;
        }
        else
        {
            Camera.gameObject.SetActive(true);
        }

        this.mouseX = transform.eulerAngles.y;
        this.mouseY = Camera.transform.eulerAngles.x;
        this.mouseY = Camera.transform.eulerAngles.x;
        this.RenderDist = 14;
        this.UnloadDist = 15;

        this.mouseX = transform.eulerAngles.y;
        this.mouseY = Camera.transform.eulerAngles.x;
    }

    private void FixedUpdate()
    {
        if (!this.networkedPlayer.isLocalPlayer)
            return;

        if (NetworkedChatController.ChatController.IsPaused())
            return;

        UpdateMove();
    }

    private void Awake()
    {
        // Check in WorldAccessor for a world
        World world = WorldAccessor.Identify(this);

        if (world is null)
        {
            world = WorldAccessor.Join(this);
        }

        CurrentWorld = world;
    }

    public override void Update()
    {
        base.Update();

        if (! this.networkedPlayer.isLocalPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            NetworkedChatController.ChatController.Pause();
        }

        if (NetworkedChatController.ChatController.IsPaused())
            return;

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

        if (this.CurrentWorld is not null)
            UpdateChunkCoord();

    }
    /// <summary>
    /// Teleport the player when shift and left mouse button are pressed.
    /// </summary>
    void UpdateTeleport()
    {
        if (Input.GetKey(KeyCode.T))
        {
            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
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

        var cameraAngle = Camera.transform.eulerAngles;
        this.mouseY -= mouseSpeedY * Input.GetAxis("Mouse Y");
        this.mouseY = Mathf.Clamp(mouseY, minMouseY, maxMouseY);

        this.Camera.transform.eulerAngles = new Vector3(mouseY, cameraAngle.y, 0);
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