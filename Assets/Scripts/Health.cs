using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.UIElements;
using Random = UnityEngine.Random;

public class Health : MonoBehaviour
{
    public int OverMaxHealthPl = 190;
    public int MaxHealthPl = 100;
    public float recovery = 2f;
    public float time;
    public Text health;

    public Transform parentHit;
    public GameObject[] Hit;
    public GameObject[] HitPref;
   
   
    public int currentHealth;

    private TextMeshPro counter;
    private Manager manager;
    private float currentRecovery;
    private PhotonView photonView;
  
    
    
    private void Start()
    { 
        currentHealth = MaxHealthPl;
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        photonView = GetComponent<PhotonView>();
        currentRecovery = recovery;
    }

    public void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.U))
        {
            TakeDamage(50);
        }*/

        health.text = currentHealth.ToString();
        LowHealth();
    }

    private void LowHealth()
    {
        if (currentHealth <= 50)
        {
            Hit[0].SetActive(true);
        }
        else
        {
            Hit[0].SetActive(false);
        }
        if (currentHealth <= 25)
        {
            Hit[1].SetActive(true);
        }
        else
        {
            Hit[1].SetActive(false);
        }
        if (currentHealth <= 10)
        {
            Hit[2].SetActive(true);
        }
        else
        {
            Hit[2].SetActive(false);
        }
    }
    private void FixedUpdate()
    {
        if (currentRecovery > 0)
        {
            currentRecovery -= Time.fixedDeltaTime;
        }

        if (currentHealth > MaxHealthPl && currentRecovery <= 0)
        {
            currentHealth -= 1;
            currentRecovery = recovery;
        }

       
    }

    
    public void TakeDamage(int damage, int actor)
    {
        if (photonView.IsMine)
        {
            currentHealth -= damage;
            GameObject NewHitBlood = Instantiate(HitPref[Random.Range(0, 3)], parentHit);

            if (currentHealth <= 0)
            {
                Debug.Log("You died");
                if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
                {
                    if (GameSettings.isAwayTeam)
                    {
                        manager.SpawnRedPlayer();
                    }
                    else
                    {
                        manager.SpawnBluePlayer();
                    }
                }
                else
                {
                    manager.Spawn();
                }


                manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
                if(actor >= 0) manager.ChangeStat_S(actor, 0 , 1);
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
