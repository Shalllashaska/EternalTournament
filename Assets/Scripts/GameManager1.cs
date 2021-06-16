using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager1 : MonoBehaviourPunCallbacks
{

    public void Button()
    {
        Debug.Log("YES");
    }

    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public override void OnLeftRoom()
    {
        
        SceneManager.LoadScene(0);
    }
}
