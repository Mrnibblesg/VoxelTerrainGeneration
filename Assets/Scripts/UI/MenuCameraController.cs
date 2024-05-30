using UnityEngine;

public class MenuCameraController : AbstractAgent
{
    public float speed = 2.0f;

    public override World CurrentWorld {
        set
        {
            currentWorld = value;
            UpdateChunkCoord();
        }
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        UpdateChunkCoord();
    }

}
