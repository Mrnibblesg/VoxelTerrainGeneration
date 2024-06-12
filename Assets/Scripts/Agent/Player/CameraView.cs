using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraView : NetworkBehaviour
{
    public Transform firstPersonView;
    public Transform thirdPersonView;
    
    private Transform cameraTransform;
    private Transform targetView;

    private float switchSpeed;
    private bool isThirdPerson;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = this.transform;
        targetView = thirdPersonView;
        switchSpeed = 5f;
        isThirdPerson = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleView();
        }

        // Smoothly move the camera to the target view
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetView.position, switchSpeed * Time.deltaTime);
        cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, targetView.rotation, switchSpeed * Time.deltaTime);

    }

    void ToggleView()
    {
        isThirdPerson = !isThirdPerson;

        if (isThirdPerson)
        {
            targetView = thirdPersonView;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            targetView = firstPersonView;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
