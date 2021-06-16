using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRaycast : MonoBehaviour
{
    public float speed;
    public Vector3 direction;
    public LayerMask canBeShoot;
    
    void Update()
    {
        gameObject.transform.position += ( gameObject.transform.forward * Time.fixedDeltaTime * speed);
        Invoke("Des", 2f);
    }

    public void Des()
    {
        Destroy(gameObject);
    }


}
