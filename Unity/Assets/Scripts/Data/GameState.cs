using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;

public struct GameState
{
    public const long SHOOT_DELAY = 30;
    public const float PLAYER_RADIUS = 0.5f;
    public const float PROJECTILE_RADIUS = 0.2f;
    public const float ASTEROID_RADIUS = 5.0f;
    public const float ASTEROID_MINIMUM_SPEED = 10.0f;
    public const float ASTEROID_MAXIMUM_SPEED = 30.0f;
    public const float PROJECTILE_SPEED = 0.5f / 60f * 10f;
    public const float INITIAL_PLAYER_SPEED = 0.3f / 60f * 10;
    public const float ACCELERATION_SPEED = INITIAL_PLAYER_SPEED / 60;
    public const float DECELERATION_SPEED = 0.025f;
    public const float MAX_VELOCITY = 0.2f;
    public const float MAX_ROTATION_VELOCITY = 0.05f;
    public const float ROTATION_ACCELERATION_SPEED = ACCELERATION_SPEED;
    public const float ROTATION_DECELERATION_SPEED = 0.025f;


    public NativeList<Asteroid> asteroids;
    public NativeList<Projectile> projectiles;

    public Player[] players;



    public long currentGameStep;

    
}