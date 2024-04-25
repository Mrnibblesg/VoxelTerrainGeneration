using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 10f;
    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.position += new Vector3(horizontal, 0f, vertical) * Time.deltaTime * speed;

        if (Input.GetKeyDown(KeyCode.F))
        {
            var position = new Vector3(transform.position.x, transform.position.y - 1.5f, transform.position.z);

            WorldGenerator.World.SetVoxel(position, WorldGenerator.World.GetVoxel(position).SetActive(false));
        }
    }
}
