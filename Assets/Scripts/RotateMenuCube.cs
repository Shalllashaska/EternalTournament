using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMenuCube : MonoBehaviour
{
    public float speed = 10f;

    private Vector3 rot = new Vector3(0, 1, 0);

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rot * Time.deltaTime * speed, Space.World);
    }
}
