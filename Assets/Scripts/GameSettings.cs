using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    DEATHMATCH = 0,
    TEAMDEATHMATCH = 1
}

public class GameSettings : MonoBehaviour
{
    public static GameMode gameMode = GameMode.DEATHMATCH;
    public static bool isAwayTeam = false;
    public static float XmouseSenivity = 10f;
    public static float YmouseSenivity = 10f;
}
