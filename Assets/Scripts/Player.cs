using UnityEngine;
using System.Collections.Generic;

public class Player : AbstractAgent
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

    //Render distance in chunks
    private int renderDist = 7;
    private int unloadDist = 8;

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

        if (Input.GetAxis("Jump") != 0)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 45);
        }
    }
}