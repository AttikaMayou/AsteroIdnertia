using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Player
{
    public long score;
    public float speed;
    public Vector2 position;
    public long lastShootStep;
    public bool isGameOver;
    public Vector2 velocity;
    public float rotationVelocity;
    public Vector2 lookDirection;
    //public int playerID;

}