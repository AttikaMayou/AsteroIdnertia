using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;

public struct GameState
{
    [ReadOnly]
    public NativeList<Asteroid> asteroids;
    [ReadOnly]
    public NativeList<Projectile> projectiles;

    //public Player[] players;
    [ReadOnly]
    public NativeList<Player> players;

    public long currentGameStep;
    public long scoreStepDelay;
}