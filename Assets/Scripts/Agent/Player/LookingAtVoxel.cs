using UnityEngine;

public class LookingAtVoxel : MonoBehaviour
{
    // public Camera playerCamera;
    private LayerMask playerLayer;
    private Vector3 viewportPoint = new Vector3(0.5f, 0.5f, 0);

    // Start is called before the first frame update
    void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
    }

    public Vector3[] LookingAt(Camera cam, float range=20f)
    {
        Ray ray = cam.ViewportPointToRay(viewportPoint);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range, ~playerLayer))
        {
            return new Vector3[] { hit.point, hit.normal };
        }

        return null;
    }

    public Vector3[] ClickedVoxel(Camera cam, float range=20f)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range, ~playerLayer))
        {
            return new Vector3[] { hit.point, hit.normal };
        }

        return null;
    }
}
