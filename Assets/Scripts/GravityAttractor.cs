using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class GravityAttractor : MonoBehaviour
{
    public float gravity = -10f;
    
    public bool FixedDirection;

    public float RotationSpeed = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<GravityBody>())
        {
            other.GetComponent<GravityBody>().planet = this.GetComponent<GravityAttractor>();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<GravityBody>())
        {
            other.GetComponent<GravityBody>().planet = null;
        }
    }
    public void Attract(Rigidbody body)
    {
        Vector3 targetDirection;
        if (FixedDirection)
        {
            targetDirection = transform.up;
        }
        else
        {
            targetDirection = (body.position - transform.position).normalized;
        }
       
        Vector3 localUp = body.transform.up;
        body.AddForce(targetDirection * gravity);
        //body.rotation = Quaternion.FromToRotation(localUp, targetDirection) * body.rotation;
        //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        Quaternion targetRotation = Quaternion.FromToRotation(localUp, targetDirection) * body.rotation;
        body.rotation = Quaternion.Lerp(body.rotation, targetRotation, RotationSpeed * Time.deltaTime);
    }
}
