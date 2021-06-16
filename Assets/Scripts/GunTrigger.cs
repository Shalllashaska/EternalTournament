using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GunTrigger : MonoBehaviour
{
    public GameObject parent;
    public GameObject ourPref;
    public int NumberOfGun;
    public Gun gunPref;
    
    
    private Transform spawnPoint;

    public bool active;
    private void Start()
    {
        spawnPoint = parent.transform.parent;
        active = true;
       
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Red Player"))
        {
            if (other.GetComponent<WeaponMenadger>().loadout[NumberOfGun] == null)
            {
                other.GetComponent<WeaponMenadger>().loadout[NumberOfGun] = gunPref;
                if (NumberOfGun == 0)
                {
                    other.GetComponent<WeaponMenadger>().EquipPistol();
                    other.GetComponent<WeaponMenadger>().loadout[0].Initialize();
                }
                else if (NumberOfGun == 1)
                {
                    other.GetComponent<WeaponMenadger>().EquipRifle();
                    other.GetComponent<WeaponMenadger>().loadout[1].Initialize();
                }
                else if (NumberOfGun == 2)
                {
                    other.GetComponent<WeaponMenadger>().EquipSniperRifle();
                    other.GetComponent<WeaponMenadger>().loadout[2].Initialize();
                }
                active = false;
                parent.SetActive(false);
                Invoke("Spawn", 10f);
            }
            else
            {
                if (NumberOfGun == 0)
                {
                    if (other.GetComponent<WeaponMenadger>().photonView)
                    {
                        other.GetComponent<WeaponMenadger>().loadout[0].SetStash(20);
                    }
                }
                else if (NumberOfGun == 1)
                {
                    if (other.GetComponent<WeaponMenadger>().photonView)
                    {
                        other.GetComponent<WeaponMenadger>().loadout[1].SetStash(40);
                    }
                }
                else if (NumberOfGun == 2)
                {
                    if (other.GetComponent<WeaponMenadger>().photonView)
                    {
                        other.GetComponent<WeaponMenadger>().loadout[2].SetStash(10);
                    }
                }
                active = false;
                parent.SetActive(false);
                Invoke("Spawn", 10f);
            }
        }
    }
    private void Spawn()
    {
        active = true;
        parent.SetActive(true);
    }
}
