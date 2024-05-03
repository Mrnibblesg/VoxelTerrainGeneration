using UnityEngine;

public class LookingAtVoxel : MonoBehaviour
{
    // public Camera playerCamera;
    private LayerMask playerLayer;
    private Vector3 lookDir;

    // Start is called before the first frame update
    void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
    }

    public Vector3[] LookingAt(Camera cam)
    {
        lookDir = cam.transform.forward;
        Debug.DrawRay(cam.transform.position, lookDir * 20);
        Ray ray = new Ray(cam.transform.position, lookDir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 20, ~playerLayer))
        {
            return new Vector3[] { hit.point, hit.normal };
        }

        return null;
    }

    public Vector3[] ClickedVoxel(Camera cam)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 20, ~playerLayer))
        {
            return new Vector3[] { hit.point, hit.normal };
        }

        return null;
    }
}
