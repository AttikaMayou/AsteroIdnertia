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
    public const float ASTEROID_RADIUS = 5.0f;
    public const float PROJECTILE_SPEED = 0.5f / 60f * 10f;
    public const float INITIAL_PLAYER_SPEED = 0.3f / 60f * 10;
    public const float ACCELERATION_SPEED = INITIAL_PLAYER_SPEED / 60;
    public const float DECELERATION_SPEED = 0.025f;
    public const float MAX_VELOCITY = 0.05f;

    
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