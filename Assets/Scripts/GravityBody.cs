using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviourPunCallbacks
{
    public GravityAttractor planet;
    private Rigidbody rb;
    private void Awake()
    {
        //planet = GameObject.FindGameObjectWithTag("Planet").GetComponent<GravityAttractor>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        if (planet != null)
        {
            planet.Attract(rb);
        }
    }
}
