using UnityEngine;

public class MenuCameraController : AuthoritativeAgent
{
    public int speed;

    //The camera has its world automatically set from the
    //WorldBuilder file

    public override void Update()
    {
        base.Update();
        Move(speed * Time.deltaTime * Vector3.right);
    }
}
