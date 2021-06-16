using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ManyRaycastBullet : MonoBehaviour
{
   public float speed;
   public float gravity = 0;
   public int damage = 20;
   private Vector3 startPosition;
   private Vector3 startForward;
   
   private bool isInitialized = false;
   private float startTime = -1;
   public bool isMine;
   

   public void Initialize(Transform startPoint)
   {
      startPosition = startPoint.position;
      startForward = startPoint.forward.normalized;
      isInitialized = true;
   }
   
   private Vector3 FindPointOnParabola(float time)
   {
      Vector3 point = startPosition + (startForward * speed * time);
      Vector3 gravityVec = Vector3.down * gravity * time * time;
      return point + gravityVec;
   }

   private bool CastRayBetwenPoints(Vector3 startPoint, Vector3 endPoint, out RaycastHit hit)
   {
      return Physics.Raycast(startPoint, endPoint - startPoint, out hit, (endPoint - startPoint).magnitude);
   }

   private void OnHit(RaycastHit hit)
   {
      ShootableObjects shootableObject = hit.transform.GetComponent<ShootableObjects>();
      EnemyHits enemy = hit.transform.GetComponent<EnemyHits>();
      if (shootableObject)
      {
         shootableObject.OnHit(hit);
         Destroy(gameObject);
      }
      else if (enemy)
      {
         if (isMine)
         {
            bool applyDamage = false;
                    
            if (GameSettings.gameMode == GameMode.DEATHMATCH)
            {
               applyDamage = true;
            }
                    
            if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
            {
               if (hit.collider.transform.root.gameObject.GetComponent<PlayerControl>().awayTeam !=
                   GameSettings.isAwayTeam)
               {
                  applyDamage = true;
               }
            }

            if (applyDamage)
            {
               enemy.OnHit(hit);
               //shooting other player on network
               if (hit.collider.gameObject.layer == 11)
               {
                  hit.collider.gameObject.GetPhotonView()
                     .RPC("Take_Damage", RpcTarget.All, damage, PhotonNetwork.LocalPlayer.ActorNumber);
               }
            }
         }
         Destroy(gameObject);
      }
   }

   private void FixedUpdate()
   {
      if (!isInitialized) return;
      if (startTime < 0) startTime = Time.time;

      RaycastHit hit;
      float currentTime = Time.time - startTime;
      float prevTime = currentTime - Time.fixedDeltaTime;
      float nextTime = currentTime + Time.fixedDeltaTime;
      Vector3 currentPoint = FindPointOnParabola(currentTime);
      Vector3 nextPoint = FindPointOnParabola(nextTime);
      if (prevTime > 0)
      {
         Vector3 prevPoint = FindPointOnParabola(prevTime);
         if (CastRayBetwenPoints(prevPoint, currentPoint, out hit))
         {
            OnHit(hit);
         }
      }

      if (CastRayBetwenPoints(currentPoint, nextPoint, out hit))
      {
         OnHit(hit);
      }
   }

   private void Update()
   {
      if (!isInitialized || startTime < 0) return;
      float currentTime = Time.time - startTime;
      Vector3 currentPoint = FindPointOnParabola(currentTime);
      transform.position = currentPoint;
   }
}
