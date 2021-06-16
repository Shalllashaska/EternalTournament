using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class EnemyHits : MonoBehaviour
{
    public GameObject particlesPrefab;

    public void OnHit(RaycastHit hit)
    {
        GameObject particles = Instantiate(particlesPrefab, hit.point + (hit.normal * 0.05f),
            Quaternion.LookRotation(hit.normal), hit.collider.transform);
        Destroy(particles, 60f);
        
    }
}
