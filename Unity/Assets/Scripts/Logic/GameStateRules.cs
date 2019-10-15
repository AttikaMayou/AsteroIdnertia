using UnityEngine;
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
            speed = GameParameters.Instance.InitialPlayerSpeed,
            position = new Vector2(-22.8f, 0f),
            lastShootStep = -GameParameters.Instance.ShootDelay,
            isGameOver = false,
            lookDirection = new Vector2(0, 1)
        };
        gs.players[0] = player;

        var player2 = new Player
        {
            score = 0,
            speed = GameParameters.Instance.InitialPlayerSpeed,
            position = new Vector2(21.4f, 0f),
            lastShootStep = -GameParameters.Instance.ShootDelay,
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
            asteroid.direction = asteroid.direction.normalized * Random.Range(GameParameters.Instance.AsteroidMinimumSpeed, GameParameters.Instance.AsteroidMaximumSpeed);
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
        var position = new Vector2(Random.Range(GameParameters.Instance.MinimalBoundary, GameParameters.Instance.MaximalBoundary), Random.Range(GameParameters.Instance.MinimalBoundary, GameParameters.Instance.MaximalBoundary));
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

        asteroid.direction = asteroid.direction.normalized * Random.Range(GameParameters.Instance.AsteroidMinimumSpeed, GameParameters.Instance.AsteroidMinimumSpeed);
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
        if(gs.players[0].isGameOver || gs.players[1].isGameOver)
        {
            return;
        }
        else
        {
            HandleCollisions(ref gs);
        }
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
            asteroid.position += -gs.asteroids[i].direction/* * 0.005f*/;
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
            projectile.position += gs.projectiles[i].speed * gs.projectiles[i].direction;
            gs.projectiles[i] = projectile;
        }
    }

    static void HandleCollisions(ref GameState gs)
    {
        for (int j = 0; j < gs.players.Length; j++)
        {
            //Collision entre asteroids et player 
            for (var i = 0; i < gs.asteroids.Length; i++)
            {
                //Destroy asteroids when they are on world boundaries
                if (gs.asteroids[i].position.x > GameParameters.Instance.MaximalBoundary
                || gs.asteroids[i].position.x < GameParameters.Instance.MinimalBoundary
                || gs.asteroids[i].position.y > GameParameters.Instance.MaximalBoundary
                || gs.asteroids[i].position.y < GameParameters.Instance.MinimalBoundary)
                {
                    gs.asteroids.RemoveAtSwapBack(i);
                    i--;
                    continue;
                }

                var sqrDistance = (gs.asteroids[i].position - gs.players[j].position).sqrMagnitude;

                if (!(sqrDistance
                      <= Mathf.Pow(GameParameters.Instance.AsteroidRadius + GameParameters.Instance.PlayerRadius,
                          2)))
                {
                    continue;
                }

                gs.players[j].isGameOver = true;

                return;
            }
        }

        for (var i = 0; i < gs.projectiles.Length; i++)
        {
            //Collisions entre projectiles et players
            for (int k = 0; k < gs.players.Length; k++)
            {
                var sqrDistance = (gs.projectiles[i].position - gs.players[0].position).sqrMagnitude;

                if(!(sqrDistance
                  <= Mathf.Pow(GameParameters.Instance.ProjectileRadius + GameParameters.Instance.PlayerRadius,
                      2)))
                {
                    continue;
                }

                if (gs.projectiles[i].playerID == k) continue;

                gs.projectiles.RemoveAtSwapBack(i);
                i--;
                gs.players[k].isGameOver = true;
                return;
            }

            //Destroy projectiles when they are on world boundaries
            if (gs.projectiles[i].position.x > GameParameters.Instance.MaximalBoundary 
                || gs.projectiles[i].position.x < GameParameters.Instance.MinimalBoundary
                || gs.projectiles[i].position.y > GameParameters.Instance.MaximalBoundary
                || gs.projectiles[i].position.y < GameParameters.Instance.MinimalBoundary)
            {
                gs.projectiles.RemoveAtSwapBack(i);
                i--;
                continue;
            }

            //Collision entre asteroids et projectile 
            for (var j = 0; j < gs.asteroids.Length; j++)
            {
                var sqrDistance = (gs.projectiles[i].position - gs.asteroids[j].position).sqrMagnitude;
                // Asteroid Radius est dépendant de projectile.size
                if (!(sqrDistance
                  <= Mathf.Pow(GameParameters.Instance.ProjectileRadius + GameParameters.Instance.AsteroidRadius,
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
                        gs.players[i].velocity.x = Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed));
                        gs.players[i].velocity.y = Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed));
                        
                        gs.players[i].rotationVelocity = Mathf.Lerp(gs.players[i].rotationVelocity, 0, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));
                        break;
                    }
                case ActionsTypes.RotateLeft:
                    {
                        //gs.players[i].position += Vector2.left * gs.players[i].speed;
                        var targetRotation = gs.players[i].rotationVelocity + GameParameters.Instance.RotationAccelerationSpeed;
                        gs.players[i].rotationVelocity = Mathf.Lerp(gs.players[i].rotationVelocity, targetRotation, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));

                        gs.players[i].velocity.x = Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-GameParameters.Instance.AccelerationSpeed));
                        gs.players[i].velocity.y = Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-GameParameters.Instance.AccelerationSpeed));
                        //var targetVel = gs.players[i].velocity.x - GameState.ACCELERATION_SPEED * 200;
                        //gs.players[i].velocity.x = Mathf.Lerp(gs.players[i].velocity.x, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        break;
                    }

                case ActionsTypes.RotateRight:
                    {
                        //gs.players[i].position += Vector2.right * gs.players[i].speed;
                        var targetRotation = gs.players[i].rotationVelocity - GameParameters.Instance.RotationAccelerationSpeed;
                        gs.players[i].rotationVelocity = Mathf.Lerp(gs.players[i].rotationVelocity, targetRotation, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));

                        gs.players[i].velocity.x = Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));
                        gs.players[i].velocity.y = Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));
                        //var targetVel = gs.players[i].velocity.x + GameState.ACCELERATION_SPEED * 200;
                        //gs.players[i].velocity.x = Mathf.Lerp(gs.players[i].velocity.x, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
                        break;

                        //var targetVel = gs.player.velocity + GameState.ACCELERATION_SPEED * 10;
                        //gs.player.velocity = (long)Mathf.Lerp(gs.player.velocity, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));

                    }
                case ActionsTypes.MoveUp:
                    {
                        var targetVel = gs.players[i].velocity + gs.players[i].lookDirection * GameParameters.Instance.RotationAccelerationSpeed;
                        //var target = ;
                        gs.players[i].velocity = Vector2.Lerp(gs.players[i].velocity, targetVel, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));
                        break;
                    }
                case ActionsTypes.MoveDown:
                    {
                        var targetVel = gs.players[i].velocity - gs.players[i].lookDirection * GameParameters.Instance.RotationAccelerationSpeed;
                        gs.players[i].velocity = Vector2.Lerp(gs.players[i].velocity, targetVel, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));
                        break;
                    }
                case ActionsTypes.Shoot:
                    {
                        gs.players[i].rotationVelocity = Mathf.Lerp(gs.players[i].rotationVelocity, 0, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));
                        gs.players[i].velocity.x = Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));
                        gs.players[i].velocity.y = Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));
                        if (gs.currentGameStep - gs.players[i].lastShootStep < GameParameters.Instance.ShootDelay)
                        {
                            break;
                        }

                        gs.players[i].lastShootStep = gs.currentGameStep;
                        gs.projectiles.Add(new Projectile
                        {
                            position = gs.players[i].position,
                            speed = GameParameters.Instance.ProjectileSpeed,
                            direction = gs.players[i].lookDirection.normalized,
                            playerID = i
                        });
                        break;
                        // Shoot Logic
                    }

            }
            //  gs.players[i].velocity = Mathf.Clamp(gs.players[i].velocity.x, -GameState.MAX_VELOCITY, GameState.MAX_VELOCITY);
            gs.players[i].position += gs.players[i].velocity;

            //gs.players[i].lookDirection = Quaternion.Euler(0, 0, gs.players[i].rotationVelocity) * gs.players[i].lookDirection;
            gs.players[i].lookDirection = Quaternion.Euler(0, 0, gs.players[i].rotationVelocity) * gs.players[i].lookDirection;
            
            gs.players[i].velocity = Vector2.ClampMagnitude(gs.players[i].velocity, GameParameters.Instance.MaxVelocity);
            gs.players[i].rotationVelocity = Mathf.Clamp(gs.players[i].rotationVelocity, -2, 2);
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



