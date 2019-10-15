﻿using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;

public class GameStateRules : MonoBehaviour
{
    public static void Init(ref GameState gs, PlayerManager playerManager)
    {
        // Initialisation des players
        Debug.Log("Initialization");

        gs.players = new Player[2];

        var player = new Player
        {
            score = 0,
            speed = GameState.INITIAL_PLAYER_SPEED,
            position = new Vector2(-22.8f, 0f),
            lastShootStep = -GameState.SHOOT_DELAY,
            isGameOver = false,
        };
        gs.players[0] = player;

        var player2 = new Player
        {
            score = 0,
            speed = GameState.INITIAL_PLAYER_SPEED,
            position = new Vector2(21.4f, 0f),
            lastShootStep = -GameState.SHOOT_DELAY,
            isGameOver = false
        };
        gs.players[1] = player2;

        var allAsteroids = playerManager.asteroidsView;

        var positions = GetAsteroidsInitialPositions(allAsteroids);

        gs.asteroids = new NativeList<Asteroid>(allAsteroids.Count, Allocator.Persistent);

        for (var i = 0; i < allAsteroids.Count; i++)
        {
            var asteroid = new Asteroid
            {
                position = positions[i],
                direction = positions[i] - new Vector2(Random.Range(-30f, 30.0f), 0),
                initialPosition = positions[i]
            };
            asteroid.direction = asteroid.direction.normalized * Random.Range(GameState.ASTEROID_MINIMUM_SPEED, GameState.ASTEROID_MAXIMUM_SPEED);
            gs.asteroids.Add(asteroid);

            allAsteroids[i].position = positions[i];
        }

        // Taille de la liste à déterminer
        gs.projectiles = new NativeList<Projectile>(100, Allocator.Persistent);
    }

    //Generate random position for all asteroids at initialization
    private static Vector2[] GetAsteroidsInitialPositions(List<Transform> asteroids)
    {
        var positions = new Vector2[asteroids.Count];

        for (var i = 0; i < asteroids.Count; i++)
        {
            positions[i] = GetRandomPosition();
        }

        return positions;
    }

    //Generate a random position within the world
    private static Vector2 GetRandomPosition()
    {
        var minimalBoundary = -150.0f;
        var maximumBoundary = 150.0f;

        var position = new Vector2(Random.Range(minimalBoundary, maximumBoundary), Random.Range(minimalBoundary, maximumBoundary));
        return position;
    }

    private static void GenerateNewAsteroid(ref GameState gs)
    {
        var position = GetRandomPosition();

        var asteroid = new Asteroid
        {
            position = position,
            direction = position - new Vector2(Random.Range(-30f, 30.0f), 0),
            initialPosition = position
        };

        asteroid.direction = asteroid.direction.normalized * Random.Range(GameState.ASTEROID_MINIMUM_SPEED, GameState.ASTEROID_MAXIMUM_SPEED);
        gs.asteroids.Add(asteroid);
    }

    public static void Step(ref GameState gs, ActionsTypes actionPlayer1, ActionsTypes actionPlayer2)
    {
        if (gs.players[0].isGameOver && gs.players[1].isGameOver)
        {
            // TODO : Game Over Logic
            throw new System.Exception("This player is in Game Over State");
        }

        UpdateAsteroidsPosition(ref gs);
        UpdateProjectiles(ref gs);
        HandleAgentInputs(ref gs, new ActionsTypes[] { actionPlayer1, actionPlayer2 });
        HandleCollisions(ref gs);
        gs.currentGameStep += 1;
    }

    static void UpdateAsteroidsPosition(ref GameState gs)
    {
        // Job System Test
        //AsteroidsMovementsJobs job = new AsteroidsMovementsJobs { asteroids = gs.asteroids };

        //JobHandle handle = job.Schedule();
        //handle.Complete();


        for (var i = 0; i < gs.asteroids.Length; i++)
        {
            var asteroid = gs.asteroids[i];
            asteroid.position += -gs.asteroids[i].direction * 0.005f;
            gs.asteroids[i] = asteroid;
        }
    }

    #region Test Job System
    // Job System Test
    //[BurstCompile]
    //public struct AsteroidsMovementsJobs : IJob
    //{
    //    public NativeArray<Asteroid> asteroids;
    //    public void Execute()
    //    {
    //        for (var i = 0; i < asteroids.Length; i++)
    //        {
    //            var asteroid = asteroids[i];
    //            asteroid.position += asteroids[i].speed;
    //            asteroids[i] = asteroid;
    //        }
    //    }
    //}
    #endregion

    static void UpdateProjectiles(ref GameState gs)
    {
        for (var i = 0; i < gs.projectiles.Length; i++)
        {
            var projectile = gs.projectiles[i];
            projectile.position += gs.projectiles[i].speed * Vector2.up;
            gs.projectiles[i] = projectile;
        }
    }

    static void HandleCollisions(ref GameState gs)
    {
        //Collision entre asteroids et player 
        for (var i = 0; i < gs.asteroids.Length; i++)
        {
            var sqrDistance = (gs.asteroids[i].position - gs.players[0].position).sqrMagnitude;

            if (!(sqrDistance
                  <= Mathf.Pow(GameState.ASTEROID_RADIUS + GameState.PLAYER_RADIUS,
                      2)))
            {
                continue;
            }

            //gs.isGameOver = true;
            // TODO : gérer s
            return;
        }

        for (var i = 0; i < gs.projectiles.Length; i++)
        {
            if (gs.projectiles[i].position.y > 80)
            {
                gs.projectiles.RemoveAtSwapBack(i);
                i--;
                continue;
            }

            for (var j = 0; j < gs.asteroids.Length; j++)
            {
                var sqrDistance = (gs.projectiles[i].position - gs.asteroids[j].position).sqrMagnitude;
                // Asteroid Radius est dépendant de projectile.size
                if (!(sqrDistance
                  <= Mathf.Pow(GameState.PROJECTILE_RADIUS + GameState.ASTEROID_RADIUS,
                      2)))
                {
                    continue;
                }

                gs.projectiles.RemoveAtSwapBack(i);
                i--;
                gs.asteroids.RemoveAtSwapBack(j);
                j--;
                break;
            }
        }

        if (gs.asteroids.Length == 0)
        {
            //gs.isGameOver = true;
        }
    }

    static void HandleAgentInputs(ref GameState gs, ActionsTypes[] actionPlayers)
    {
        for (var i = 0; i < actionPlayers.Length; i++)
        {
            //Player 1
            switch (actionPlayers[i])
            {
                case ActionsTypes.Nothing:
                    {
                        gs.players[i].velocity.x = Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        gs.players[i].velocity.y = Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        break;
                    }
                case ActionsTypes.RotateLeft:
                    {
                        //gs.players[i].position += Vector2.left * gs.players[i].speed;
                        var targetVel = gs.players[i].velocity.x - GameState.ACCELERATION_SPEED * 200;
                        gs.players[i].velocity.x = Mathf.Lerp(gs.players[i].velocity.x, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        break;
                    }

                case ActionsTypes.RotateRight:
                    {
                        //gs.players[i].position += Vector2.right * gs.players[i].speed;

                        var targetVel = gs.players[i].velocity.x + GameState.ACCELERATION_SPEED * 200;
                        gs.players[i].velocity.x = Mathf.Lerp(gs.players[i].velocity.x, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        break;

                        //var targetVel = gs.player.velocity + GameState.ACCELERATION_SPEED * 10;
                        //gs.player.velocity = (long)Mathf.Lerp(gs.player.velocity, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        
                    }
                case ActionsTypes.MoveUp:
                    {
                        var targetVel = gs.players[i].velocity.y + GameState.ACCELERATION_SPEED * 200;
                        gs.players[i].velocity.y = Mathf.Lerp(gs.players[i].velocity.y, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        break;
                    }
                case ActionsTypes.MoveDown:
                    {
                        var targetVel = gs.players[i].velocity.y - GameState.ACCELERATION_SPEED * 200;
                        gs.players[i].velocity.y = Mathf.Lerp(gs.players[i].velocity.y, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        break;
                    }
                case ActionsTypes.Shoot:
                    {
                        gs.players[i].velocity.x = Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        gs.players[i].velocity.y = Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        if (gs.currentGameStep - gs.players[i].lastShootStep < GameState.SHOOT_DELAY)
                        {
                            break;
                        }

                        gs.players[i].lastShootStep = gs.currentGameStep;
                        gs.projectiles.Add(new Projectile
                        {
                            position = gs.players[i].position,
                            speed = GameState.PROJECTILE_SPEED
                        });
                        break;
                        // Shoot Logic
                    }

            }
          //  gs.players[i].velocity = Mathf.Clamp(gs.players[i].velocity.x, -GameState.MAX_VELOCITY, GameState.MAX_VELOCITY);
            gs.players[i].velocity = Vector2.ClampMagnitude(gs.players[i].velocity, GameState.MAX_VELOCITY);
            gs.players[i].position += gs.players[i].velocity;
            Debug.Log(gs.players[i].velocity);
        }
    }

    private static readonly ActionsTypes[] AvailableActions = new[] { ActionsTypes.Nothing, ActionsTypes.RotateLeft, ActionsTypes.RotateRight, ActionsTypes.MoveUp, ActionsTypes.MoveDown, ActionsTypes.Shoot };

    public static ActionsTypes[] GetAvailableActions(ref GameState gs)
    {
        return AvailableActions;
    }

    public void GameOver(ref GameState gs)
    {

    }

}



