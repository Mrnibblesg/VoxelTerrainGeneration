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
