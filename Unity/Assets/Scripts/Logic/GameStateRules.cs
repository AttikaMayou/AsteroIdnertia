using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;

//Auteur : Félix
//Modifications : Margot, Arthur et Attika

public class GameStateRules : MonoBehaviour
{
    public static void Init(ref GameState gs, PlayerManager playerManager)
    {
        // Initialisation des players
        Debug.Log("Initialization");

        //TODO : Debug initialization of AvailableActions (declared line 403)
        /*AvailableActions = new NativeArray<ActionsTypes> {
            AvailableActions[0] = ActionsTypes.Nothing,
            ActionsTypes.RotateLeft,
            ActionsTypes.RotateRight,
            ActionsTypes.MoveUp,
            ActionsTypes.MoveDown,
            ActionsTypes.Shoot
        };*/

        gs.scoreStepDelay = -GameParameters.Instance.StepScoreDelay;

        gs.players = new NativeList<Player>(2, Allocator.Persistent);

        var player = new Player
        {
            score = 0,
            speed = GameParameters.Instance.InitialPlayerSpeed,
            position = new Vector2(-22.8f, 0f),
            lastShootStep = -GameParameters.Instance.ShootDelay,
            isGameOver = false,
            lookDirection = new Vector2(0, 1)
        };
        gs.players.Add(player);

        var player2 = new Player
        {
            score = 0,
            speed = GameParameters.Instance.InitialPlayerSpeed,
            position = new Vector2(21.4f, 0f),
            lastShootStep = -GameParameters.Instance.ShootDelay,
            isGameOver = false,
            lookDirection = new Vector2(0, 1)
        };
        gs.players.Add(player2);

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
        var position = new Vector2(Random.Range(-GameParameters.Instance.Boundary, GameParameters.Instance.Boundary), 
            Random.Range(-GameParameters.Instance.Boundary, GameParameters.Instance.Boundary));
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

        asteroid.direction = asteroid.direction.normalized * Random.Range(GameParameters.Instance.AsteroidMinimumSpeed, GameParameters.Instance.AsteroidMaximumSpeed);
        gs.asteroids.Add(asteroid);
    }

    public static void Step(ref GameState gs, NativeArray<ActionsTypes> actionPlayer1, NativeArray<ActionsTypes> actionPlayer2)
    {
        //if (gs.players[0].isGameOver && gs.players[1].isGameOver)
        //{
        //    // TODO : Game Over Logic => ??
        //    throw new System.Exception("This player is in Game Over State");
        //}

        UpdateAsteroidsPosition(ref gs);
        UpdateProjectiles(ref gs);
        NativeArray<ActionsTypes> tempActionsTypeArray = new NativeArray<ActionsTypes>(6, Allocator.Temp);
        tempActionsTypeArray[0] = actionPlayer1[0];
        tempActionsTypeArray[1] = actionPlayer2[0];
        tempActionsTypeArray[2] = actionPlayer1[1];
        tempActionsTypeArray[3] = actionPlayer2[1];
        tempActionsTypeArray[4] = actionPlayer1[2];
        tempActionsTypeArray[5] = actionPlayer2[2];


        HandleAgentInputs(ref gs, tempActionsTypeArray);
        if(gs.players[0].isGameOver || gs.players[1].isGameOver)
        {
            return;
        }
        else
        {
             HandleCollisions(ref gs);
        }
        gs.currentGameStep += 1;
        if(gs.currentGameStep - gs.scoreStepDelay < GameParameters.Instance.StepScoreDelay)
        {
            return;
        }
        gs.scoreStepDelay = gs.currentGameStep;

        Player oldPlayer1 = gs.players[0];
        gs.players[0] = createPlayer(oldPlayer1.score += GameParameters.Instance.StepScore, oldPlayer1.speed, oldPlayer1.position,
            oldPlayer1.lastShootStep, oldPlayer1.isGameOver, oldPlayer1.velocity, oldPlayer1.rotationVelocity, oldPlayer1.lookDirection);

        Player oldPlayer2 = gs.players[1];
        gs.players[1] = createPlayer(oldPlayer2.score += GameParameters.Instance.StepScore, oldPlayer2.speed, oldPlayer2.position,
            oldPlayer2.lastShootStep, oldPlayer2.isGameOver, oldPlayer2.velocity, oldPlayer2.rotationVelocity, oldPlayer2.lookDirection);
    }

    static void UpdateAsteroidsPosition(ref GameState gs)
    {
        for (var i = 0; i < gs.asteroids.Length; i++)
        {
            var asteroid = gs.asteroids[i];
            asteroid.position += -gs.asteroids[i].direction * 0.005f;
            gs.asteroids[i] = asteroid;
        }
    }

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
                if (gs.asteroids[i].position.x > GameParameters.Instance.Boundary
                || gs.asteroids[i].position.x < -GameParameters.Instance.Boundary
                || gs.asteroids[i].position.y > GameParameters.Instance.Boundary
                || gs.asteroids[i].position.y < -GameParameters.Instance.Boundary)
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

                Player oldPlayer = gs.players[j];
                gs.players[j] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, true, oldPlayer.velocity, oldPlayer.rotationVelocity, oldPlayer.lookDirection);

                return;
            }
        }

        for (var i = 0; i < gs.projectiles.Length; i++)
        {
            //Collisions entre projectiles et players
            for (int k = 0; k < gs.players.Length; k++)
            {
                var sqrDistance = (gs.projectiles[i].position - gs.players[k].position).sqrMagnitude;

                if (!(sqrDistance
                  <= Mathf.Pow(GameParameters.Instance.ProjectileRadius + GameParameters.Instance.PlayerRadius,
                      2)))
                {
                    continue;
                }

                if (gs.projectiles[i].playerID == k) continue;

                gs.projectiles.RemoveAtSwapBack(i);
                i--;

                Player oldPlayer = gs.players[(int)gs.projectiles[i].playerID];
                gs.players[(int)gs.projectiles[i].playerID] = createPlayer(oldPlayer.score += GameParameters.Instance.EnemyDestroyScore, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, true, oldPlayer.velocity, oldPlayer.rotationVelocity, oldPlayer.lookDirection);

                return;
            }

            //Destroy projectiles when they are on world boundaries
            if (gs.projectiles[i].position.x > GameParameters.Instance.Boundary
                || gs.projectiles[i].position.x < -GameParameters.Instance.Boundary
                || gs.projectiles[i].position.y > GameParameters.Instance.Boundary
                || gs.projectiles[i].position.y < -GameParameters.Instance.Boundary)
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

                Player oldPlayer = gs.players[(int)gs.projectiles[i].playerID];
                gs.players[(int)gs.projectiles[i].playerID] = createPlayer(oldPlayer.score += GameParameters.Instance.AsteroidDestructionScore, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, true, oldPlayer.velocity, oldPlayer.rotationVelocity, oldPlayer.lookDirection);

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

    static void HandleAgentInputs(ref GameState gs, NativeArray<ActionsTypes> actionPlayers)
    {

        Debug.Log(gs.players.Length);
        //Switch between Rotation
        for (var i = 0; i < actionPlayers.Length; i++)
        {
            Vector2 velocity = new Vector2();
            float rotationVelocity = 0.0f;
            Player oldPlayer = gs.players[i % 2];
            switch (actionPlayers[i])
            {
                case ActionsTypes.Nothing:
                    {
                        velocity = new Vector2(Mathf.Lerp(gs.players[i % 2].velocity.x, 0, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed)),
                            Mathf.Lerp(gs.players[i % 2].velocity.y, 0, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed)));
                        rotationVelocity = Mathf.Lerp(gs.players[i % 2].rotationVelocity, 0, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));

                        gs.players[i % 2] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, oldPlayer.lookDirection);

                        break;
                    }
                case ActionsTypes.RotateLeft:
                    {
                        var targetRotation = gs.players[i % 2].rotationVelocity + GameParameters.Instance.RotationAccelerationSpeed * 200;

                        rotationVelocity = Mathf.Lerp(gs.players[i % 2].rotationVelocity, targetRotation, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));
                        velocity = new Vector2(Mathf.Lerp(gs.players[i % 2].velocity.x, 0, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed)),
                            Mathf.Lerp(gs.players[i % 2].velocity.y, 0, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed)));
                       
                        gs.players[i % 2] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, oldPlayer.lookDirection);

                        break;
                    }

                case ActionsTypes.RotateRight:
                    {
                        var targetRotation = gs.players[i % 2].rotationVelocity - GameParameters.Instance.RotationAccelerationSpeed * 200;

                        rotationVelocity = Mathf.Lerp(gs.players[i % 2].rotationVelocity, targetRotation, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));
                        velocity = new Vector2(Mathf.Lerp(gs.players[i % 2].velocity.x, 0, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed)),
                            Mathf.Lerp(gs.players[i % 2].velocity.y, 0, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed)));

                        gs.players[i % 2] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, oldPlayer.lookDirection);

                        break;
                    }

                case ActionsTypes.MoveUp:
                    {
                        var targetVel = gs.players[i % 2].velocity + gs.players[i % 2].lookDirection * GameParameters.Instance.AccelerationSpeed * 200;
                        velocity = Vector2.Lerp(gs.players[i % 2].velocity, targetVel, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed));

                        gs.players[i % 2] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, oldPlayer.rotationVelocity, oldPlayer.lookDirection);

                        break;
                    }
                case ActionsTypes.MoveDown:
                    {
                        var targetVel = gs.players[i % 2].velocity - gs.players[i % 2].lookDirection * GameParameters.Instance.AccelerationSpeed * 200;
                       velocity = Vector2.Lerp(gs.players[i % 2].velocity, targetVel, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed));

                        gs.players[i % 2] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, oldPlayer.rotationVelocity, oldPlayer.lookDirection);

                        break;
                    }

                case ActionsTypes.Shoot:
                    {
                        rotationVelocity = Mathf.Lerp(gs.players[i % 2].rotationVelocity, 0, 1 - Mathf.Exp(-GameParameters.Instance.RotationDecelerationSpeed));
                        velocity = new Vector2(Mathf.Lerp(gs.players[i % 2].velocity.x, 0, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed)),
                            Mathf.Lerp(gs.players[i % 2].velocity.y, 0, 1 - Mathf.Exp(-GameParameters.Instance.DecelerationSpeed)));

                        gs.players[i % 2] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, oldPlayer.lookDirection);

                        if (gs.currentGameStep - gs.players[i % 2].lastShootStep < GameParameters.Instance.ShootDelay)
                        {
                            break;
                        }

                        long lastShootStep = gs.currentGameStep;
                        gs.projectiles.Add(new Projectile
                        {
                            position = gs.players[i % 2].position,
                            speed = GameParameters.Instance.ProjectileSpeed,
                            direction = gs.players[i % 2].lookDirection.normalized,
                            playerID = i % 2
                        });

                        Player oldPlayerTmp = gs.players[i % 2];
                        gs.players[i % 2] = createPlayer(oldPlayerTmp.score, oldPlayerTmp.speed, oldPlayerTmp.position,
                    lastShootStep, oldPlayerTmp.isGameOver, oldPlayerTmp.velocity, oldPlayerTmp.rotationVelocity, oldPlayerTmp.lookDirection);

                        break;
                        // Shoot Logic
                    }
                case ActionsTypes.NoShoot:
                    {
                        break;
                    }
            }
        }

        for (int i = 0; i < 2; i++)
        {
            Player oldPlayer = gs.players[i];

            Vector2 position = oldPlayer.position;
            position += gs.players[i].velocity;

            Vector2 lookDirection = Quaternion.Euler(0, 0, gs.players[i].rotationVelocity) * gs.players[i].lookDirection;

            Vector2 velocity = Vector2.ClampMagnitude(gs.players[i].velocity, GameParameters.Instance.MaxVelocity);
            float rotationVelocity = Mathf.Clamp(gs.players[i].rotationVelocity, -2, 2);

            gs.players[i % 2] = createPlayer(oldPlayer.score, oldPlayer.speed, position,
                   oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, lookDirection);
        }
    }

    [NativeDisableParallelForRestriction]
    private static NativeArray<ActionsTypes> AvailableActions = new NativeArray<ActionsTypes>(7, Allocator.Persistent);
    
    public static NativeArray<ActionsTypes> GetAvailableActions(ref GameState gs)
    {
        AvailableActions[0] = ActionsTypes.Nothing;
        AvailableActions[1] = ActionsTypes.RotateRight;
        AvailableActions[2] = ActionsTypes.RotateLeft;
        AvailableActions[3] = ActionsTypes.MoveDown;
        AvailableActions[4] = ActionsTypes.MoveUp;
        AvailableActions[5] = ActionsTypes.Shoot;
        AvailableActions[6] = ActionsTypes.NoShoot;
        return AvailableActions;
    }

    public void GameOver(ref GameState gs)
    {

    }


    public static GameState Clone(ref GameState gs)
    {
        GameState clone = new GameState();
        clone.asteroids = new NativeList<Asteroid>(gs.asteroids.Length, Allocator.Temp);
        clone.asteroids.AddRange(gs.asteroids);
        clone.projectiles = new NativeList<Projectile>(gs.projectiles.Length, Allocator.Temp);
        clone.projectiles.AddRange(gs.projectiles);
        clone.players = new NativeList<Player>(gs.players.Length, Allocator.Temp);
        clone.players.AddRange(gs.players);
        clone.currentGameStep = gs.currentGameStep;

        return clone;
    }

    public static void CopyTo(ref GameState gs, ref GameState gsCopy)
    {
        gsCopy.asteroids.Clear();
        gsCopy.asteroids.AddRange(gs.asteroids);
        gsCopy.projectiles.Clear();
        gsCopy.projectiles.AddRange(gs.projectiles);
        gsCopy.players = new NativeList<Player>(gs.players.Length, Allocator.Temp);
        gsCopy.players.AddRange(gs.players);
        gsCopy.currentGameStep = gs.currentGameStep;
    }
    
    public static Player createPlayer(long sore, float speed, Vector2 position, long lastShootStep, bool isGameOver, Vector2 velocity, float rotationVelocity, Vector2 lookDirection)
    {
        return new Player
        {
            score = sore,
            speed = speed,
            position = position,
            lastShootStep = lastShootStep,
            isGameOver = isGameOver,
            velocity = velocity,
            rotationVelocity = rotationVelocity,
            lookDirection = lookDirection
        };
    }

}



