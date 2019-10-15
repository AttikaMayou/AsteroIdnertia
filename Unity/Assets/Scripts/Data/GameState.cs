using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;

public struct GameState : ICloneable
{
    public NativeList<Asteroid> asteroids;
    public NativeList<Projectile> projectiles;

    public Player[] players;

    

    public long currentGameStep;

    public object Clone()
    {
        GameState clone = new GameState();
        clone.asteroids = asteroids;
        clone.projectiles = projectiles;
        clone.players[0] = players[0];
        clone.players[1] = players[1];
        return clone;
    }
}