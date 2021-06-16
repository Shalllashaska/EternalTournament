using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotationplanet : MonoBehaviour
{
    public GameObject Target;

    public float Angel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(Target.transform.position, Vector3.left, Angel * Time.deltaTime);
    }
}
