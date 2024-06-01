using UnityEngine;

public class MenuCameraController : AuthoritativeAgent
{
    [SerializeField]
    private int speed = 4;

    //The camera has its world automatically set from the
    //WorldBuilder file

    public override void Update()
    {
        base.Update();
        Move(speed * Time.deltaTime * Vector3.right);
    }
}
