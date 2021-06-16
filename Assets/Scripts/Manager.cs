using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Random = UnityEngine.Random;

public class PlayerInfo
{
    public string nickanme;
    public int actor;
    public short kills;
    public short deaths;
    public bool awayTeam;

    public PlayerInfo(string n, int a, short k, short d, bool t)
    {
        this.nickanme = n;
        this.actor = a;
        this.kills = k;
        this.deaths = d;
        this.awayTeam = t;
    }
}

public enum GameState
{
    Waiting = 0,
    Starting = 1,
    Playing = 2,
    Ending = 3
}
public class Manager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region Fields


    public int mainMenu = 0;
    public int killCount = 20;
    public int scoreCount = 20;
    public int matchLenght = 180;
    
    
    
    public GameObject mapCam;

    public string playerPrefab;
    public string redPlayerPrefab;
    public string bluePlayerPrefab;
    public Transform[] spawnPoints;
    public Transform[] spawnRedPoints;
    public Transform[] spawnBluePoints;

    public List<PlayerInfo> playerInfos = new List<PlayerInfo>();
    public int myInd;


    private bool playerAdded;
    
    private Text deathUI;
    private Text killsUI;
    private Text timerUI;
    private Transform leaderboardUI;
    private Transform endGameUI;

    private GameState state = GameState.Waiting;
    private PlayerInfo winner;


    private int homeSize, awaySize;
    private int currentTime;
    private bool winHome, winAway;
    private Coroutine timerCoroutine;


    #endregion

    #region Codes

    public enum EventCodes : byte
    {
        NewPlayer,
        UpdatePlayers,
        ChangeStat,
        RefreshTimer
    }

    #endregion

    #region SystemMethods
    
    private void Start()
    {
        mapCam.SetActive(false);
        
        ValidateConnection();
        NewPlayer_S();
        InitializedUI();
        InitializeTimer();

        if (PhotonNetwork.IsMasterClient)
        {
            playerAdded = true;
            if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
            {
                if (GameSettings.isAwayTeam)
                {
                    SpawnRedPlayer();
                }
                else
                {
                    SpawnBluePlayer();
                }
            }
            else
            {
                Spawn();
            }

        }
        
    }

    private void Update()
    {

        if (state == GameState.Ending)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(leaderboardUI.gameObject.activeSelf) leaderboardUI.gameObject.SetActive(false);
            else Leaderboard(leaderboardUI);
        }
        
    }
    
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #endregion
     
    #region Methods
    
    public void SpawnRedPlayer()
    {
        Debug.Log("red team");
        Transform spawnPoint = spawnRedPoints[Random.Range(0, spawnRedPoints.Length)];
        PhotonNetwork.Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

    }

    public void SpawnBluePlayer()
    {
        Debug.Log("blue team");
        Transform spawnPoint = spawnBluePoints[Random.Range(0, spawnBluePoints.Length)];
        PhotonNetwork.Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    public void Spawn()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        PhotonNetwork.Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    private void ValidateConnection()
    {
        if (PhotonNetwork.IsConnected) return;
        SceneManager.LoadScene(mainMenu);
    }

    private void StateCheck()
    {
        if (state == GameState.Ending)
        {
            EndGame();
        }
    }

    private void ScoreCheck()
    {
        //Define temporary variables    
        if (GameSettings.gameMode == GameMode.DEATHMATCH)
        { 
            bool detecWin = false;
            //Check to see if any player has met the win conditions
            foreach (PlayerInfo a in playerInfos)
            {
                //DEATHMATCH
                if (a.kills >= killCount)
                {
                    winner = a;
                    detecWin = true;
                    break;
                }
            }
            //Did we find a winner?
            if (detecWin)
            {
                //Are we the master client? It's game still going?
                if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
                {
                    //If so, tell the other players that a winner has been detected
                    UpdatePlayers_S((int)GameState.Ending, playerInfos);
                }
            }
        }

        if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
        {
            winHome = false;
            winAway = false;
            int scoreHome = 0;
            int scoreAway = 0;
            foreach (PlayerInfo a in playerInfos)
            {
               if(a.awayTeam)
                   if (a.kills * 10 - a.deaths * 2 <= 0)
                   {
                       scoreAway += 0;
                   }
                   else
                   {
                       scoreAway += a.kills * 10 - a.deaths * 2;
                   }

               if (!a.awayTeam)
               {
                   if (a.kills * 10 - a.deaths * 2 <= 0)
                   {
                       scoreHome += 0;
                   }
                   else
                   {
                       scoreHome += a.kills * 10 - a.deaths * 2;
                   }
               }
            }

            if (scoreHome >= scoreCount)
            {
                winHome = true;
            }
            else if (scoreAway >= scoreCount)
            {
                winAway = true;
            }
            //Did we find a winner?
            if (winAway || winHome)
            {
                //Are we the master client? It's game still going?
                if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
                {
                    //If so, tell the other players that a winner has been detected
                    UpdatePlayers_S((int)GameState.Ending, playerInfos);
                }
            }
        }
    }

    private void EndGame()
    {
        //Set game state to the EndGame
        state = GameState.Ending;
        
        //set timer to 0
        if(timerCoroutine != null) StopCoroutine(timerCoroutine);
        currentTime = 0;
        RefreshTimerUI();
        
        //Disable room
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        
        mapCam.SetActive(true);

        endGameUI.gameObject.SetActive(true);
        if (GameSettings.gameMode == GameMode.DEATHMATCH)
        {
            endGameUI.GetChild(0).gameObject.SetActive(true);
            endGameUI.Find("DM/Design/Wins").GetComponent<Text>().text = "WINS\n" + $"{winner.nickanme}";
            Leaderboard(endGameUI.Find("DM/Leaderboard"));
        }
        else if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
        {
            endGameUI.GetChild(1).gameObject.SetActive(true);
            if (winHome)
            {
                endGameUI.Find("TDM/Design/Wins").GetComponent<Text>().text = "WINS\n" + "BLUE";
            }
            else if(winAway)
            {
                endGameUI.Find("TDM/Design/Wins").GetComponent<Text>().text = "WINS\n" + "RED";
            }
            Leaderboard(endGameUI.Find("TDM/Leaderboard"));
        }
        

         StartCoroutine(End(6f));
    }

    private void InitializedUI()
    {
        deathUI = GameObject.Find("HUD/Stats/Deaths/Text").GetComponent<Text>();
        killsUI = GameObject.Find("HUD/Stats/Kills/Text").GetComponent<Text>();
        timerUI = GameObject.Find("HUD/Timer/Text").GetComponent<Text>();
        leaderboardUI = GameObject.Find("HUD").transform.Find("Leaderboard").transform;
        endGameUI = GameObject.Find("Canvas").transform.Find("EndGame").transform;

        RefreshMyStats();
    }

    private void InitializeTimer()
    {
        currentTime = matchLenght;
        RefreshTimerUI();

        if (PhotonNetwork.IsMasterClient)
        {
            timerCoroutine = StartCoroutine(Timer());
        }
    }

    private void RefreshMyStats()
    {
        if (playerInfos.Count > myInd)
        {
            deathUI.text = $"{playerInfos[myInd].deaths} DEATHS";
            killsUI.text = $"{playerInfos[myInd].kills} KILLS";
        }
        else
        {
            deathUI.text = "0 DEATHS";
            killsUI.text = "0 KILLS";
        }
    }

    private void RefreshTimerUI()
    {
        string minutes = (currentTime / 60).ToString("00");
        string seconds = (currentTime % 60).ToString("00");
        timerUI.text = $"{minutes}:{seconds}";
    }

    private void Leaderboard(Transform lb)
    {
        Transform lbHelp = lb;
        
        //Sort
        List<PlayerInfo> sorted = SortPlayers(playerInfos);
        
        if (GameSettings.gameMode == GameMode.DEATHMATCH) lb = lb.Find("DM/tab");
        if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
        {
            int scoreA = 0;
            int scoreH = 0;
            foreach (PlayerInfo a in sorted)
            {
                if (a.awayTeam)
                {
                    if (a.kills * 10 - a.deaths * 2 <= 0)
                    {
                        scoreA += 0;
                    }
                    else
                    {
                        scoreA += a.kills * 10 - a.deaths * 2;
                    }
                    
                }
                else 
                if (a.kills * 10 - a.deaths * 2 <= 0)
                {
                    scoreH += 0;
                }
                else
                {
                    scoreH += a.kills * 10 - a.deaths * 2;
                }
            }
            lb.Find("TDM/Design/Mode/Score/Home").GetComponent<Text>().text = scoreH.ToString();
            lb.Find("TDM/Design/Mode/Score/Away").GetComponent<Text>().text = scoreA.ToString();
            lb = lb.Find("TDM/tab");
            
        }

        //Clean up
        for (int i = 1; i < lb.childCount; i++)
        {
            Destroy(lb.GetChild(i).gameObject);
        }

        //Cache prefab
        GameObject playerCard = lb.GetChild(0).gameObject;
        playerCard.SetActive(false);

        

        //Display
        int ind = 1;
        

        foreach (PlayerInfo a in sorted)
        {
            GameObject newCard = Instantiate(playerCard, lb);

            if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
            {
                newCard.transform.Find("Home").gameObject.SetActive(!a.awayTeam);
                newCard.transform.Find("Away").gameObject.SetActive(a.awayTeam);
                if (a.awayTeam)
                {
                    newCard.transform.Find("Num/Text").GetComponent<Text>().text = (ind - homeSize + 1).ToString();

                }
                else
                {
                    newCard.transform.Find("Num/Text").GetComponent<Text>().text = ind.ToString();

                }
            }
            else
            {
                newCard.transform.Find("Num/Text").GetComponent<Text>().text = ind.ToString();
            }

            newCard.transform.Find("Name/Text").GetComponent<Text>().text = a.nickanme;
            if (a.kills * 10 - a.deaths * 2 <= 0)
            {
                newCard.transform.Find("Score/Text").GetComponent<Text>().text = $"0 SCORE";
            }
            else
            {
                newCard.transform.Find("Score/Text").GetComponent<Text>().text = $"{a.kills * 10 - a.deaths * 2} SCORE";
            }

            newCard.transform.Find("Kills/Text").GetComponent<Text>().text = $"{a.kills} KILLS";
            newCard.transform.Find("Deaths/Text").GetComponent<Text>().text = $"{a.deaths} DEATH";
            ind++;
            newCard.SetActive(true);
        }
        //Activate
        lb.gameObject.SetActive(true);
        lb.parent.gameObject.SetActive(true);
        lb = lbHelp;
        lb.gameObject.SetActive(true);

    }

    private List<PlayerInfo> SortPlayers(List<PlayerInfo> plInfo)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        if (GameSettings.gameMode == GameMode.DEATHMATCH)
        {
            while (sorted.Count < plInfo.Count)
            {
                //Set defaults
                short highest = -1;
                PlayerInfo selection = plInfo[0];

                // Grab next highest
                foreach (PlayerInfo a in plInfo)
                {
                    if (sorted.Contains(a)) continue;
                    if (a.kills > highest)
                    {
                        selection = a;
                        highest = a.kills;
                    }
                }

                // Add player
                sorted.Add(selection);
            }

            winner = sorted[0];
            
        }
        if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
        {
            List<PlayerInfo> homeSorted = new List<PlayerInfo>();
            List<PlayerInfo> awaySorted = new List<PlayerInfo>();

            homeSize = 0;
            awaySize = 0;
            
            
            foreach (PlayerInfo p in plInfo)
            {
                if (p.awayTeam) awaySize++;
                else homeSize++;
            }
            
            while (homeSorted.Count < homeSize)
            {
                //Set defaults
                short highest = -1;
                PlayerInfo selection = plInfo[0];

                // Grab next highest
                foreach (PlayerInfo a in plInfo)
                {
                    if (a.awayTeam) continue;
                    if (homeSorted.Contains(a)) continue;
                    if (a.kills > highest)
                    {
                        selection = a;
                        highest = a.kills;
                    }
                }

                // Add player
                homeSorted.Add(selection);
            }
            
            
            while (awaySorted.Count < awaySize)
            {
                //Set defaults
                short highest = -1;
                PlayerInfo selection = plInfo[0];

                // Grab next highest
                foreach (PlayerInfo a in plInfo)
                {
                    if (!a.awayTeam) continue;
                    if (awaySorted.Contains(a)) continue;
                    if (a.kills > highest)
                    {
                        selection = a;
                        highest = a.kills;
                    }
                }

                // Add player
                awaySorted.Add(selection);
            }
            sorted.AddRange(homeSorted);
            sorted.AddRange(awaySorted);
            winner = sorted[0];
        }
        
        return sorted;
    }

    private bool CalculateTeam()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount % 2 == 0;
    }
    
    #endregion

    #region PhotonMethods
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(mainMenu);
    }
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;

        EventCodes e = (EventCodes) photonEvent.Code;
        object[] o = (object[]) photonEvent.CustomData;

        switch (e)
        {
            case EventCodes.NewPlayer:
                NewPlayer_R(o);
                break;
            case EventCodes.UpdatePlayers:
                UpdatePlayers_R(o);
                break;
            case EventCodes.ChangeStat:
                ChangeStat_R(o);
                break;
            case EventCodes.RefreshTimer:
                RefreshTimer_R(o);
                break;
        }
    }
    
    #endregion

    #region Events

    public void NewPlayer_S()
    {
        
        object[] package = new object[5];
        package[0] = PhotonNetwork.LocalPlayer.NickName;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = (short) 0;
        package[3] = (short) 0;
        package[4] = CalculateTeam();
        Debug.Log(PhotonNetwork.LocalPlayer.ActorNumber);
        PhotonNetwork.RaiseEvent(
            (byte) EventCodes.NewPlayer,
            package,
            new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient},
            new SendOptions {Reliability = true}
        );
    }

    public void NewPlayer_R(object[] data)
    {
        PlayerInfo p = new PlayerInfo(
            (string) data[0],
            (int) data[1],
            (short) data[2],
            (short) data[3],
            (bool) data[4]
        );
        playerInfos.Add(p);

        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Red Player"))
        {
            gameObject.GetComponent<PlayerControl>().TrySync();
        }
        
        UpdatePlayers_S((int)state, playerInfos);
    }

    public void UpdatePlayers_S(int state, List<PlayerInfo> info)
    {
        object[] package = new object[info.Count + 1];

        package[0] = state;
        for (int i = 0; i < info.Count; i++)
        {
            object[] piece = new object[5];

            piece[0] = info[i].nickanme;
            piece[1] = info[i].actor;
            piece[2] = info[i].kills;
            piece[3] = info[i].deaths;
            piece[4] = info[i].awayTeam;

            package[i + 1] = piece;
        }

        PhotonNetwork.RaiseEvent(
            (byte) EventCodes.UpdatePlayers,
            package,
            new RaiseEventOptions {Receivers = ReceiverGroup.All},
            new SendOptions {Reliability = true}
        );

    }

    public void UpdatePlayers_R(object[] data)
    {
        state = (GameState) data[0];
        
        if (playerInfos.Count < data.Length - 1)
        {
            foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Red Player"))
            {
                gameObject.GetComponent<PlayerControl>().TrySync();
            }  
        }
        
        playerInfos = new List<PlayerInfo>();

        for (int i = 1; i < data.Length; i++)
        {
            object[] extract = (object[]) data[i];

            PlayerInfo p = new PlayerInfo(
                (string) extract[0],
                (int) extract[1],
                (short) extract[2],
                (short) extract[3],
                (bool) extract[4]
            );

            playerInfos.Add(p);

            if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor)
            {
                myInd = i - 1;
                
                //IF we have been waiting to be added to the game then spawn us in
                if (!playerAdded)
                {
                    playerAdded = true;
                    GameSettings.isAwayTeam = p.awayTeam;
                    if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
                    {
                        if (GameSettings.isAwayTeam)
                        {
                            SpawnRedPlayer();
                        }
                        else
                        {
                            SpawnBluePlayer();
                        }
                    }
                    else
                    {
                        Spawn();
                    }
                }
            }
        }
        
        StateCheck();
    }

    public void ChangeStat_S(int actor, byte stat, byte amt)
    {
        object[] package = new object[] {actor, stat, amt};

        PhotonNetwork.RaiseEvent(
            (byte) EventCodes.ChangeStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void ChangeStat_R(object[] data)
    {
       
        int actor = (int) data[0];
        byte stat = (byte) data[1];
        byte amt = (byte) data[2];
        for (int i = 0; i < playerInfos.Count; i++)
        {
            if (playerInfos[i].actor == actor)
            {
                switch (stat)
                {
                    case 0:
                        playerInfos[i].kills += amt;
                        Debug.Log($"Player {playerInfos[i].nickanme} : kills = {playerInfos[i].kills}");
                        break;
                    case 1:
                        playerInfos[i].deaths += amt;
                        Debug.Log($"Player {playerInfos[i].nickanme} : deaths = {playerInfos[i].deaths}");
                        break;
                }
                
                if(i == myInd) RefreshMyStats(); 
                if(leaderboardUI.gameObject.activeSelf) Leaderboard(leaderboardUI);
                
                break;
            }
        }
        
        ScoreCheck();
    }

    public void RefreshTimer_S()
    {
        object[] package = new object[] {currentTime};

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.RefreshTimer,
            package,
            new RaiseEventOptions{Receivers = ReceiverGroup.All},
            new SendOptions{Reliability = true}
            );
    }

    public void RefreshTimer_R(object[] data)
    {
        currentTime = (int) data[0];
        RefreshTimerUI();
    }
    
    #endregion

    #region Coroutines

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);

        currentTime -= 1;

        if (currentTime <= 0)
        {
            timerCoroutine = null;
            UpdatePlayers_S((int)GameState.Ending, playerInfos);
        }
        else
        {
            RefreshTimer_S();
            timerCoroutine = StartCoroutine(Timer());
        }
    }
    
    private IEnumerator End(float wait)
    {
        yield return new WaitForSeconds(wait);

        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    #endregion
}