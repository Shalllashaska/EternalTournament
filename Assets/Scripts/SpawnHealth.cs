using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHealth : MonoBehaviour
{
    public GameObject prefabHealth;
    

    public Transform[] spawnHealth;
  
    
    void Start()
    {
        
        for (int i = 0; i < spawnHealth.Length; i++)
        {
            Quaternion rot = spawnHealth[i].rotation;
            GameObject currentGun = Instantiate(prefabHealth, spawnHealth[i].position, rot, spawnHealth[i]);
        }
    }
    
    
}
