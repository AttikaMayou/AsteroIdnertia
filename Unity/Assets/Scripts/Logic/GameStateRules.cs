using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;

public class GameStateRules : MonoBehaviour
{
    public static void Init(ref GameState gs, PlayerManager playerManager)
    {
        Debug.Log("Initialization");

        var allAsteroids = playerManager.asteroidsView;

        var positions = GetAsteroidsInitialPositions(ref gs, allAsteroids);

        gs.asteroids = new NativeList<Asteroid>(allAsteroids.Count, Allocator.Persistent);

        for (var i = 0; i < allAsteroids.Count; i++)
        {
            var asteroid = new Asteroid
            {
                position = positions[i],
                //TODO : Setup random direction :
                speed = gs.player.position - new Vector2(0.0f, -10.0f),
                size = Random.Range(1.0f, 5.0f),
                
                
            };
            asteroid.speed = asteroid.speed.normalized;
            asteroid.speed *= -0.5f;
            gs.asteroids.Add(asteroid);
        }

        // Taille de la liste à déterminer
        gs.projectiles = new NativeList<Projectile>(100, Allocator.Persistent);

        // Initialisation des players

        var player = new Player
        {
            score = 0,
            speed = GameState.INITIAL_PLAYER_SPEED,
            position = new Vector2(-22.8f, 0f),
            lastShootStep = -GameState.SHOOT_DELAY,
            isGameOver = false,
        };
        gs.player = player;
        
    }

    //Generate random position for asteroids at initialization
    private static Vector3[] GetAsteroidsInitialPositions(ref GameState gs, List<Transform> asteroids)
    {
        //Take negatives from these floats to get left player boundaries and positives ones to get right player boundaries
        var leftBoundary = 30.0f; //* gs.player == GameSystemScript.player1 ? -1 : 1;
        var rightBoundary = 10.0f; // * gs.player == GameSystemScript.player2 ? -1 : 1;

        //Adjust maximum with number of asteroids (the more there are, the higher maximum should be)
        var minimalX = 50.0f;
        var maximalX = 150.0f;

        var positions = new Vector3[asteroids.Count];

        for(var i = 0; i < asteroids.Count; i++)
        {
            positions[i] = new Vector3(Random.Range(leftBoundary, rightBoundary), 0.0f, Random.Range(minimalX, maximalX));
        }

        return positions;
    }


    public static void Step(ref GameState gs, ActionsTypes action)
    {
        if (gs.player.isGameOver)
        {
            // TODO : Game Over Logic
            throw new System.Exception("This player is in Game Over State");
        }

        UpdateAsteroidsPosition(ref gs);
        UpdateProjectiles(ref gs);
        HandleAgentInputs(ref gs, action);
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
            asteroid.position += gs.asteroids[i].speed;
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
        for (var i = 0; i < gs.projectiles.Length; i++)
        {
            var sqrDistance = (gs.projectiles[i].position - gs.player.position).sqrMagnitude;

            if (!(sqrDistance
                  <= Mathf.Pow(GameState.PROJECTILE_RADIUS + GameState.PLAYER_RADIUS,
                      2)))
            {
                continue;
            }

            //gs.isGameOver = true;
            // TODO : gérer 
            return;
        }

        for(var i = 0; i < gs.projectiles.Length; i++)
        {
            if(gs.projectiles[i].position.y > 10)
            {
                gs.projectiles.RemoveAtSwapBack(i);
                i--;
                continue;
            }

            for(var j = 0; j < gs.asteroids.Length; j++)
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

        if(gs.asteroids.Length == 0)
        {
            //gs.isGameOver = true;
        }
    }

    static void HandleAgentInputs(ref GameState gs, ActionsTypes action)
    {
        switch (action)
        {
            case ActionsTypes.Nothing:
                {
                    gs.player.velocity = (long)Mathf.Lerp(gs.player.velocity, 0, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                    break;
                }

            case ActionsTypes.MoveLeft:
                {
                     gs.player.position += Vector2.left * gs.player.speed;
                    //var targetVel = gs.player.velocity - GameState.ACCELERATION_SPEED * 10;
                    //gs.player.velocity = (long)Mathf.Lerp(gs.player.velocity, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                    break;
                }

            case ActionsTypes.MoveRight:
                {
                    gs.player.position += Vector2.right * gs.player.speed;
                    //var targetVel = gs.player.velocity + GameState.ACCELERATION_SPEED * 10;
                    //gs.player.velocity = (long)Mathf.Lerp(gs.player.velocity, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                    break;
                }

            case ActionsTypes.Shoot:
                {
                    gs.player.velocity = (long)Mathf.Lerp(gs.player.velocity, 0, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                    if (gs.currentGameStep - gs.player.lastShootStep < GameState.SHOOT_DELAY)
                    {
                        break;
                    }

                    gs.player.lastShootStep = gs.currentGameStep;
                    gs.projectiles.Add(new Projectile
                    {
                        position = gs.player.position + Vector2.up * 1.5f,
                        speed = GameState.PROJECTILE_SPEED * Vector2.up
                    });
                    break;
                    // Shoot Logic
                }
        }
        gs.player.velocity = (long)Mathf.Clamp(gs.player.velocity, -GameState.MAX_VELOCITY, GameState.MAX_VELOCITY);
        gs.player.position.x += gs.player.velocity;
    }

    private static readonly ActionsTypes[] AvailableActions = new[] { ActionsTypes.Nothing, ActionsTypes.MoveLeft, ActionsTypes.MoveRight, ActionsTypes.Shoot };

    public static ActionsTypes[] GetAvailableActions(ref GameState gs)
    {
        return AvailableActions;
    }



}



