using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;

public struct GameState : ICloneable
{
    public const long SHOOT_DELAY = 30;
    public const float PLAYER_RADIUS = 0.5f;
    public const float PROJECTILE_RADIUS = 0.2f;
    public const float PROJECTILE_SPEED = 0.5f / 60f * 10f;
    public const float INITIAL_PLAYER_SPEED = 0.3f / 60f * 10;

    
    public NativeList<Asteroid> asteroids;
    public NativeList<Projectile> projectiles;

    public NativeArray<Player> players;

    public long currentGameStep;

    public object Clone()
    {
        GameState clone = new GameState();
        clone.asteroids = asteroids;
        clone.projectiles = projectiles;
        clone.players = players;
        return clone;
    }
}