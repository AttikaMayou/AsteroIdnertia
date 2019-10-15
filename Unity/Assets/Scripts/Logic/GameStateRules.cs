using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;

public class GameStateRules : MonoBehaviour
{
    public static void Init(ref GameState gs, PlayerManager playerManager)
    {
        // Very Bad !!
        //var allAsteroids = GameObject.FindGameObjectsWithTag(asteroidsTags);

        //TODO : get asteroids from inspector 
        /*if(allAsteroids == null || allAsteroids.Length == 0)
        {
            throw new System.Exception("There is no asteroids with tag : " + asteroidsTags);
        }
        */

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

        var positions = GetAsteroidsInitialPositions(ref gs, allAsteroids);

        gs.asteroids = new NativeList<Asteroid>(allAsteroids.Count, Allocator.Persistent);

        for (var i = 0; i < allAsteroids.Count; i++)
        {
            var asteroid = new Asteroid
            {
                position = positions[i],
                //TODO : Setup random direction :
                direction = gs.players[0].position - new Vector2(0.0f, -10.0f),
                size = Random.Range(1.0f, 5.0f)
            };
            gs.asteroids.Add(asteroid);
        }

        // Taille de la liste à déterminer
        gs.projectiles = new NativeList<Projectile>(100, Allocator.Persistent);
    }

    //Generate random position for asteroids at initialization
    private static Vector2[] GetAsteroidsInitialPositions(ref GameState gs, List<Transform> asteroids)
    {
        //Take negatives from these floats to get left player boundaries and positives ones to get right player boundaries
        var leftBoundary = 30.0f; //* gs.player == GameSystemScript.player1 ? -1 : 1;
        var rightBoundary = 10.0f; // * gs.player == GameSystemScript.player2 ? -1 : 1;

        //Adjust maximum with number of asteroids (the more there are, the higher maximum should be)
        var minimalX = 50.0f;
        var maximalX = 150.0f;

        var positions = new Vector2[asteroids.Count];

        for(var i = 0; i < asteroids.Count; i++)
        {
            positions[i] = new Vector2(Random.Range(leftBoundary, rightBoundary), Random.Range(minimalX, maximalX));
        }

        return positions;
    }


    public static void Step(ref GameState gs, ActionsTypes actionPlayer1, ActionsTypes actionPlayer2)
    {
        if (gs.players[0].isGameOver)
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
            var sqrDistance = (gs.projectiles[i].position - gs.players[0].position).sqrMagnitude;

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

    static void HandleAgentInputs(ref GameState gs, ActionsTypes[] actionPlayers)
    {
        for (var i = 0; i < actionPlayers.Length; i++)
        {
            //Player 1
            switch (actionPlayers[i])
            {
                case ActionsTypes.Nothing:
                    {
                        gs.players[i].velocity = (long)Mathf.Lerp(gs.players[i].velocity, 0, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        break;
                    }
                case ActionsTypes.MoveLeft:
                    {
                        gs.players[i].position += Vector2.left * gs.players[i].speed;
                        //var targetVel = gs.player.velocity - GameState.ACCELERATION_SPEED * 10;
                        //gs.player.velocity = (long)Mathf.Lerp(gs.player.velocity, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        break;
                    }

                case ActionsTypes.MoveRight:
                    {
                        gs.players[i].position += Vector2.right * gs.players[i].speed;
                        //var targetVel = gs.player.velocity + GameState.ACCELERATION_SPEED * 10;
                        //gs.player.velocity = (long)Mathf.Lerp(gs.player.velocity, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        break;
                    }
                case ActionsTypes.Shoot:
                    {
                        gs.players[i].velocity = (long)Mathf.Lerp(gs.players[i].velocity, 0, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        if (gs.currentGameStep - gs.players[i].lastShootStep < GameState.SHOOT_DELAY)
                        {
                            break;
                        }

                        gs.players[i].lastShootStep = gs.currentGameStep;
                        gs.projectiles.Add(new Projectile
                        {
                            position = gs.players[i].position + Vector2.up * 1.5f,
                            speed = GameState.PROJECTILE_SPEED * Vector2.up
                        });
                        break;
                        // Shoot Logic
                    }
            }
            gs.players[i].velocity = (long)Mathf.Clamp(gs.players[i].velocity, -GameState.MAX_VELOCITY, GameState.MAX_VELOCITY);
            gs.players[i].position.x += gs.players[i].velocity;
        }
    }

    private static readonly ActionsTypes[] AvailableActions = new[] { ActionsTypes.Nothing, ActionsTypes.MoveLeft, ActionsTypes.MoveRight, ActionsTypes.Shoot };

    public static ActionsTypes[] GetAvailableActions(ref GameState gs)
    {
        return AvailableActions;
    }



}



