using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LobbyManager : MonoBehaviourPunCallbacks
{

    #region Fields

    public Text LogText;
    public Text modeValue;
    public InputField nickname;
    public InputField roomnameField;
    public Slider maxPlayerSlider;
    public Text maxPLayerValue;
    public Text mouseXText;
    public Text mouseYText;
    
    public GameObject tabLog;
    public GameObject tabRoomList;
    public GameObject tabCreate;
    public GameObject tabHelp;

    public GameObject buttonRoom;
    
    private List<RoomInfo> roomList;
    
    #endregion

    #region System Methods

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Connect();
        Debug.Log(Cursor.lockState);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.F1))
        {
            tabHelp.SetActive(true);
        }
        else
        {
            tabHelp.SetActive(false);
        }
    }

    #endregion

    #region Photon Methods

    public override void OnConnectedToMaster()
    {
        Log("Connected to Master");

        PhotonNetwork.JoinLobby();
        base.OnConnectedToMaster();
    }
    
    public void Create()
    {
        Debug.Log("Create Room");
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte)maxPlayerSlider.value;

        options.CustomRoomPropertiesForLobby = new string[] {"map", "mode"};
        
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
        properties.Add("map", 0);
        properties.Add("mode", (int)GameSettings.gameMode);
        options.CustomRoomProperties = properties;
        
        PhotonNetwork.NickName = nickname.text;
         
        PhotonNetwork.CreateRoom(roomnameField.text, options);
    }

    public void ChangeMacPlayersSlider(float value)
    {
        maxPLayerValue.text = Mathf.RoundToInt(value).ToString();
    }
    
    public void ChangeMuseX(float value)
    {
        mouseXText.text = Mathf.RoundToInt(value).ToString();
        GameSettings.XmouseSenivity = value;
    }
    public void ChangeMouseY(float value)
    {
        mouseYText.text = Mathf.RoundToInt(value).ToString();
        GameSettings.YmouseSenivity = value;
    }
    
    
    public void JoinRandomRoom()
    {
        Debug.Log("JOIN RANDOM ROOM ");
        PhotonNetwork.NickName = nickname.text;
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Create ();

        base.OnJoinRandomFailed(returnCode, message);
    }

    public void JoinRoom(Transform button)
    {
        Debug.Log("JOIN ROOM");
        
        string roomName = button.transform.Find("NameRoom").GetComponent<Text>().text;
        
        VerifyUsername();

        RoomInfo roomInfo = null;
        Transform buttonParent = button.parent;
        for (int i = 0; i < buttonParent.childCount; i++)
        {
            if (buttonParent.GetChild(i).Equals(button))
            {
                roomInfo = roomList[i];
                break;
            }
        }

        if (roomInfo != null)
        {
            LoadGameSettings(roomInfo);
            PhotonNetwork.NickName = nickname.text;
            PhotonNetwork.JoinRoom(roomName);
        }
        
    }

    public void LoadGameSettings(RoomInfo info)
    {
        GameSettings.gameMode = (GameMode) info.CustomProperties["mode"];
        Debug.Log(System.Enum.GetName(typeof(GameMode), GameSettings.gameMode));
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnJoinedRoom()
    {
        Log("Joined the room");
        StartGame();
    }

    private void Log(string message)
    {
        Debug.Log(message);
        if (tabLog.activeSelf)
        {
            LogText.text += "\n";
            LogText.text += message;
        }
    }

    #endregion

    #region Methods
    private void VerifyUsername ()
    {
        if (string.IsNullOrEmpty(nickname.text))
        {
            nickname.text = "RANDOM_USER_" + Random.Range(100, 1000);
        }
        
    }

    public void TabCloseAll()
    { 
        tabLog.SetActive(false);
        tabRoomList.SetActive(false);
        tabCreate.SetActive(false);
    }

    public void TabOpenLog()
    {
        TabCloseAll();
        tabLog.SetActive(true);
    }

    public void TabOpenRooms()
    {
        TabCloseAll();
        tabRoomList.SetActive(true);
    }

    public void TabOpenCreateRoom()
    {
        TabCloseAll();
        tabCreate.SetActive(true);
        
        GameSettings.gameMode = (GameMode) 0;
        modeValue.text = "MODE: " + System.Enum.GetName(typeof(GameMode), (GameMode) 0);

        maxPlayerSlider.value = maxPlayerSlider.maxValue;
        maxPLayerValue.text = Mathf.RoundToInt(maxPlayerSlider.value).ToString();
    }
    

    private void ClearRoomList()
    {
        Transform content = tabRoomList.transform.Find("Scroll View/Viewport/Content");
        foreach (Transform a in content)
        {
            Destroy(a.gameObject);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> list)
    {
        roomList = list;
        ClearRoomList();
        
        Debug.Log("LOADED ROOMS @ " + Time.time);
        
        Transform content = tabRoomList.transform.Find("Scroll View/Viewport/Content");

        foreach (RoomInfo a in roomList)
        {
            GameObject newRoomButton = Instantiate(buttonRoom, content) as GameObject;

            newRoomButton.transform.Find("MODE").GetComponent<Text>().text =
                System.Enum.GetName(typeof(GameMode), (GameMode) a.CustomProperties["mode"]);
            newRoomButton.transform.Find("NameRoom").GetComponent<Text>().text = a.Name;
            newRoomButton.transform.Find("Players").GetComponent<Text>().text = a.PlayerCount + "/" + a.MaxPlayers;
            
            newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(newRoomButton.transform); });
        }
        
        base.OnRoomListUpdate(roomList);
    }
    
    void Connect()
    {
        PhotonNetwork.NickName = "Player " + Random.Range(1000, 9999);
        Log("Player's name is set to " + PhotonNetwork.NickName);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1.5";
        PhotonNetwork.ConnectUsingSettings();
    }

    public void StartGame()
    {
        VerifyUsername();
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangeMode()
    {
        int newMode = (int) GameSettings.gameMode + 1;
        if (newMode >= System.Enum.GetValues(typeof(GameMode)).Length) newMode = 0;
        GameSettings.gameMode = (GameMode) newMode;
        modeValue.text = "MODE: " + System.Enum.GetName(typeof(GameMode), newMode);
    }

    #endregion

}
