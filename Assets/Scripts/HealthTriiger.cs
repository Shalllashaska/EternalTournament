using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthTriiger : MonoBehaviour
{
    public GameObject parent;
    public int HealthPlus = 20;
  

    [HideInInspector]public bool active;
    
    private void Start()
    {
        active = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Red Player"))
        {
            if (other.GetComponent<Health>().currentHealth < other.GetComponent<Health>().OverMaxHealthPl)
            {
                if ((other.GetComponent<Health>().currentHealth += HealthPlus) >
                    other.GetComponent<Health>().OverMaxHealthPl)
                {
                    other.GetComponent<Health>().currentHealth = other.GetComponent<Health>().OverMaxHealthPl;
                    active = false;
                    parent.SetActive(false);
                    Invoke("Spawn", 10f);
                    return;
                }
                else
                {
                    active = false;
                    parent.SetActive(false);
                    Invoke("Spawn", 10f);
                    return;
                }
            }
        }
    }
    private void Spawn()
    {
        active = true;
        parent.SetActive(true);
    }
}
