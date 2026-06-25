using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : ScriptableObject
{
    public int BoardSizeX = 5;

    public int BoardSizeY = 5;

    public int MatchesMin = 3;

    public int LevelMoves = 16;

    public float LevelTime = 30f;

    public float TimeForHint = 5f;

    public ContainerSettings ContainerSettings = new ContainerSettings { Capacity = 5, Match_Min = 3 };
    public Vector3 ContainerStartPos = new Vector3(-2, -4, 0);
}
