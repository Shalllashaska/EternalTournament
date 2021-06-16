using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGuns : MonoBehaviour
{
    public GameObject prefabPistol;
    public GameObject prefabRifle;
    public GameObject prefabSniperRifle;

    public Transform[] spawnPistol;
    public Transform[] spawnRifle;
    public Transform[] spawnSniperRifle;
    
    void Start()
    {
        
        for (int i = 0; i < spawnPistol.Length; i++)
        {
            Quaternion rot = spawnPistol[i].rotation;
            GameObject currentGun = Instantiate(prefabPistol, spawnPistol[i].position, rot, spawnPistol[i]);
        }
        for (int i = 0; i < spawnRifle.Length; i++)
        {
            Quaternion rot = spawnRifle[i].rotation;
            GameObject currentGun = Instantiate(prefabRifle, spawnRifle[i].position, rot, spawnRifle[i]);
        }
        for (int i = 0; i < spawnSniperRifle.Length; i++)
        {
            Quaternion rot = spawnSniperRifle[i].rotation;
            GameObject currentGun = Instantiate(prefabSniperRifle, spawnSniperRifle[i].position, rot, spawnSniperRifle[i]);
        }
    }
}
