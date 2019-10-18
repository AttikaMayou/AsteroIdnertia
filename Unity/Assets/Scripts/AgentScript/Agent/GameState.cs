using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;

public struct GameState
{
    [NativeDisableParallelForRestriction]
    public NativeList<Asteroid> asteroids;
    [NativeDisableParallelForRestriction]
    public NativeList<Projectile> projectiles;

    //public Player[] players;
    [NativeDisableParallelForRestriction]
    public NativeList<Player> players;

    public long currentGameStep;
    public long scoreStepDelay;
}