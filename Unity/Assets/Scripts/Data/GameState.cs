using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;

public struct GameState
{
    public NativeList<Asteroid> asteroids;
    public NativeList<Projectile> projectiles;

    public Player[] players;

    public long currentGameStep;
    public long scoreStepDelay;
}