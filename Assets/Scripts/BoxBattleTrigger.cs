using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxBattleTrigger : MonoBehaviour
{
   public GameObject HomeRed;
   public GameObject HomeBlue;
   private void OnTriggerExit(Collider other)
   {
      if (other.CompareTag("Red Player"))
      {
         other.transform.position = HomeRed.transform.position;
         other.attachedRigidbody.velocity = Vector3.zero;
      }
      else if(other.CompareTag("Blue Player"))
      {
         other.transform.position = HomeBlue.transform.position;
         other.attachedRigidbody.velocity = Vector3.zero;
      }
   }
}
