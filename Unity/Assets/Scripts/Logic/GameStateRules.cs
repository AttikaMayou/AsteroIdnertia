﻿using UnityEngine;
using Unity.Collections;

public class GameStateRules : MonoBehaviour
{
    public static void Init(ref GameState gs)
    {
        // Very Bad !! Ah ça je te fais pas dire hahaha. Et sinon même si c'est pas propre,
        //oublie pas de vérifier que c'est pas null avant de faire des opérations dessus
        var allAsteroids = GameObject.FindGameObjectsWithTag("Asteroid");

        gs.asteroids = new NativeList<Asteroid>(allAsteroids.Length, Allocator.Persistent);

        for (var i = 0; i < allAsteroids.Length; i++)
        {
            var asteroid = new Asteroid
            {
                position = allAsteroids[i].transform.position,
                //TODO : Setup random direction :
                direction = Vector2.down,
                size = Random.Range(0.5f, 3f)
            };
            gs.asteroids.Add(asteroid);
        }

        // Taille de la liste à déterminer
        gs.projectiles = new NativeList<Projectile>(100, Allocator.Persistent);

        // Initialisation des players
        gs.players = new NativeArray<Player>(2, Allocator.Persistent);
        for (int i = 0; i < 2; i++)
        {
            var player = new Player
            {
                score = 0,
                speed = GameState.INITIAL_PLAYER_SPEED,
                position = Vector2.zero,
                lastShootStep = -GameState.SHOOT_DELAY,
                isGameOver = false,
                playerID = i
            };
            gs.players[i] = player;
        }
    }


    public static void Step(ref GameState gs, ActionsTypes action, int playerID)
    {
        if (gs.players[playerID].isGameOver)
        {
            // TODO : Game Over Logic
            throw new System.Exception("Player " + playerID + " is in a Game Over State");
        }

        UpdateAsteroidsPosition(ref gs);
        UpdateProjectiles(ref gs);
        HandleAgentInputs(ref gs, action);
        HandleCollisions(ref gs);
        gs.currentGameStep += 1;
    }

    static void UpdateAsteroidsPosition(ref GameState gs)
    {
        for(var i = 0; i < gs.asteroids.Length; i++)
        {
            var asteroid = gs.asteroids[i];
            asteroid.position += gs.asteroids[i].speed;
            gs.asteroids[i] = asteroid;
        }
    } 

    static void UpdateProjectiles(ref GameState gs)
    {
        for(var i = 0; i < gs.projectiles.Length; i++)
        {
            var projectile = gs.projectiles[i];
            projectile.position += gs.projectiles[i].speed * Vector2.up;
            gs.projectiles[i] = projectile;
        }
    }

    static void HandleCollisions(ref GameState gs)
    {
        
    }

    static void HandleAgentInputs(ref GameState gs, ActionsTypes action)
    {

    }

    private static readonly ActionsTypes[] AvailableActions = new[] { ActionsTypes.Nothing, ActionsTypes.MoveLeft, ActionsTypes.MoveRight, ActionsTypes.Shoot };

    public static ActionsTypes[] GetAvailableActions(ref GameState gs)
    {
        return AvailableActions;
    }



}



