using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Sway : MonoBehaviourPunCallbacks
{
   #region Variables
   //Public
   public float intesity;
   public float smooth;
   public bool isMine;
   
   //Private 
   private Quaternion originRotation;

   #endregion

   #region Monobehavior Callbacks

   private void Start()
   {
      originRotation = transform.localRotation;
   }

   private void Update()
   {
      UpdateSway();
   }

   #endregion

   #region Private Methods

   private void UpdateSway()
   {

      //Controls
      float xMouse = Input.GetAxis("Mouse X");
      float yMouse = Input.GetAxis("Mouse Y");

      if (!isMine)
      {
         xMouse = 0;
         yMouse = 0;
      }


      //calculate target rotation
      Quaternion t_x_adj = Quaternion.AngleAxis(-intesity * xMouse, Vector3.up);
      Quaternion t_y_adj = Quaternion.AngleAxis(intesity * yMouse, Vector3.right);
      Quaternion targetRotation = originRotation * t_x_adj * t_y_adj;

      //Rotate towards target rotation
      transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * smooth);
   }


   #endregion
}
