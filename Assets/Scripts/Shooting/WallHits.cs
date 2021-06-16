using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHits : ShootableObjects
{
   public GameObject particlesPrefab;

   public override void OnHit(RaycastHit hit)
   {
      GameObject particles = Instantiate(particlesPrefab, hit.point + (hit.normal * 0.05f),
         Quaternion.LookRotation(hit.normal), transform.root.parent);
      Destroy(particles,  60f);
   }
}
