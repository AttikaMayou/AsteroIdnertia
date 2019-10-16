using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;

//Auteur : Félix
//Modifications : Margot, Arthur et Attika

public class GameStateRules : MonoBehaviour
{
    public static void Init(ref GameParametersStruct gameParameters, ref GameState gs, PlayerManager playerManager)
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

        gs.scoreStepDelay = -gameParameters.StepScoreDelay;

        gs.players = new NativeList<Player>(2, Allocator.Persistent);

        var player = new Player
        {
            score = 0,
            speed = gameParameters.InitialPlayerSpeed,
            position = new Vector2(-22.8f, 0f),
            lastShootStep = -gameParameters.ShootDelay,
            isGameOver = false,
            lookDirection = new Vector2(0, 1)
        };
        gs.players.Add(player);

        var player2 = new Player
        {
            score = 0,
            speed = gameParameters.InitialPlayerSpeed,
            position = new Vector2(21.4f, 0f),
            lastShootStep = -gameParameters.ShootDelay,
            isGameOver = false,
            lookDirection = new Vector2(0, 1)
        };
        gs.players.Add(player2);

        var allAsteroids = playerManager.asteroidsView;

        var positions = GetAsteroidsInitialPositions(ref gameParameters, allAsteroids);

        gs.asteroids = new NativeList<Asteroid>(allAsteroids.Count, Allocator.Persistent);

        for (var i = 0; i < allAsteroids.Count; i++)
        {
            var asteroid = new Asteroid
            {
                position = positions[i],
                direction = positions[i] - new Vector2(Random.Range(-30f, 30.0f), 0),
                initialPosition = positions[i]
            };
            asteroid.direction = asteroid.direction.normalized * Random.Range(gameParameters.AsteroidMinimumSpeed, gameParameters.AsteroidMaximumSpeed);
            gs.asteroids.Add(asteroid);

            allAsteroids[i].position = positions[i];
        }

        // Taille de la liste à déterminer
        gs.projectiles = new NativeList<Projectile>(100, Allocator.Persistent);
    }

    //Generate random position for all asteroids at initialization
    private static Vector2[] GetAsteroidsInitialPositions(ref GameParametersStruct gameParameters, List<Transform> asteroids)
    {
        var positions = new Vector2[asteroids.Count];

        for (var i = 0; i < asteroids.Count; i++)
        {
            positions[i] = GetRandomPosition(ref gameParameters);
        }

        return positions;
    }

    //Generate a random position within the world
    private static Vector2 GetRandomPosition(ref GameParametersStruct gameParameters)
    {
        var position = new Vector2(Random.Range(-gameParameters.Boundary, gameParameters.Boundary), 
            Random.Range(-gameParameters.Boundary, gameParameters.Boundary));
        return position;
    }

    private static void GenerateNewAsteroid(ref GameParametersStruct gameParameters, ref GameState gs)
    {
        var position = GetRandomPosition(ref gameParameters);

        var asteroid = new Asteroid
        {
            position = position,
            direction = position - new Vector2(Random.Range(-30f, 30.0f), 0),
            initialPosition = position
        };

        asteroid.direction = asteroid.direction.normalized * Random.Range(gameParameters.AsteroidMinimumSpeed, gameParameters.AsteroidMaximumSpeed);
        gs.asteroids.Add(asteroid);
    }

    public static void Step(ref GameParametersStruct gameParameters, ref GameState gs, ActionsTypes actionPlayer1, ActionsTypes actionPlayer2)
    {
        //if (gs.players[0].isGameOver && gs.players[1].isGameOver)
        //{
        //    // TODO : Game Over Logic => ??
        //    throw new System.Exception("This player is in Game Over State");
        //}

        UpdateAsteroidsPosition(ref gs);
        UpdateProjectiles(ref gs);

        HandleAgentInputs(ref gameParameters, ref gs, actionPlayer1, actionPlayer2);

        if(gs.players[0].isGameOver || gs.players[1].isGameOver)
        {
            return;
        }
        else
        {
             HandleCollisions(ref gameParameters, ref gs);
        }
        gs.currentGameStep += 1;
        if(gs.currentGameStep - gs.scoreStepDelay < 100)
        {
            return;
        }
        gs.scoreStepDelay = gs.currentGameStep;

        Player oldPlayer1 = gs.players[0];
        gs.players[0] = createPlayer(oldPlayer1.score += gameParameters.StepScore, oldPlayer1.speed, oldPlayer1.position,
            oldPlayer1.lastShootStep, oldPlayer1.isGameOver, oldPlayer1.velocity, oldPlayer1.rotationVelocity, oldPlayer1.lookDirection);

        Player oldPlayer2 = gs.players[1];
        gs.players[1] = createPlayer(oldPlayer2.score += gameParameters.StepScore, oldPlayer2.speed, oldPlayer2.position,
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

    static void HandleCollisions(ref GameParametersStruct gameParameters, ref GameState gs)
    {
        for (int j = 0; j < gs.players.Length; j++)
        {
            //Collision entre asteroids et player 
            for (var i = 0; i < gs.asteroids.Length; i++)
            {
                //Destroy asteroids when they are on world boundaries
                if (gs.asteroids[i].position.x > gameParameters.Boundary
                || gs.asteroids[i].position.x < -gameParameters.Boundary
                || gs.asteroids[i].position.y > gameParameters.Boundary
                || gs.asteroids[i].position.y < -gameParameters.Boundary)
                {
                    gs.asteroids.RemoveAtSwapBack(i);
                    i--;
                    continue;
                }

                var sqrDistance = (gs.asteroids[i].position - gs.players[j].position).sqrMagnitude;

                if (!(sqrDistance
                      <= Mathf.Pow(gameParameters.AsteroidRadius + gameParameters.PlayerRadius,
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
                  <= Mathf.Pow(gameParameters.ProjectileRadius + gameParameters.PlayerRadius,
                      2)))
                {
                    continue;
                }

                if (gs.projectiles[i].playerID == k) continue;

                Player oldPlayer = gs.players[(int)gs.projectiles[i].playerID];
                gs.players[(int)gs.projectiles[i].playerID] = createPlayer(oldPlayer.score += gameParameters.EnemyDestroyScore, oldPlayer.speed, oldPlayer.position,
                     oldPlayer.lastShootStep, true, oldPlayer.velocity, oldPlayer.rotationVelocity, oldPlayer.lookDirection);

                gs.projectiles.RemoveAtSwapBack(i);
                i--;

                

                return;
            }

            //Destroy projectiles when they are on world boundaries
            if (gs.projectiles[i].position.x > gameParameters.Boundary
                || gs.projectiles[i].position.x < -gameParameters.Boundary
                || gs.projectiles[i].position.y > gameParameters.Boundary
                || gs.projectiles[i].position.y < -gameParameters.Boundary)
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
                  <= Mathf.Pow(gameParameters.ProjectileRadius + gameParameters.AsteroidRadius,
                      2)))
                {
                    continue;
                }

                Player oldPlayer = gs.players[(int)gs.projectiles[i].playerID];
                gs.players[(int)gs.projectiles[i].playerID] = createPlayer(oldPlayer.score += gameParameters.AsteroidDestructionScore, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, false, oldPlayer.velocity, oldPlayer.rotationVelocity, oldPlayer.lookDirection);

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

    static void HandleAgentInputs(ref GameParametersStruct gameParameters, ref GameState gs, ActionsTypes chosenPlayer1Actions, ActionsTypes chosenPlayer2Actions)
    {
        for (var i = 0; i < gs.players.Length; i++)
        {
            float rotationVelocity = 0.0f;
            Vector2 velocity = new Vector2();
            Player oldPlayer = gs.players[i];

            ActionsTypes tempChosenActionPlayer = 0;

            if (i == 0)
            {
                tempChosenActionPlayer = chosenPlayer1Actions;
            }
            else if(i == 1)
            {
                tempChosenActionPlayer = chosenPlayer2Actions;
            }
            //Debug.LogFormat("chosenPlayer1Actions : {0} / chosenPlayer2Actions : {1} / tempChosenPlayer : {2}", chosenPlayer1Actions, chosenPlayer2Actions, tempChosenActionPlayer);

            switch (tempChosenActionPlayer)
            {
                case ActionsTypes.Nothing:
                    {
                        velocity = new Vector2(Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)),
                           Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)));
                        rotationVelocity = Mathf.Lerp(gs.players[i].rotationVelocity, 0, 1 - Mathf.Exp(-gameParameters.RotationDecelerationSpeed));

                        gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                        oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, oldPlayer.lookDirection);

                        break;
                    }
                case ActionsTypes.RotateRightNS:
                    {
                        var targetRotation = gs.players[i].rotationVelocity - gameParameters.RotationAccelerationSpeed * 200;

                        rotationVelocity = Mathf.Lerp(gs.players[i].rotationVelocity, targetRotation, 1 - Mathf.Exp(-gameParameters.RotationDecelerationSpeed));
                        velocity = new Vector2(Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)),
                            Mathf.Lerp(gs.players[i % 2].velocity.y, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)));

                        gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                        oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, oldPlayer.lookDirection);

                        break;
                    }

                case ActionsTypes.RotateRightS:
                    {
                        var targetRotation = gs.players[i].rotationVelocity - gameParameters.RotationAccelerationSpeed * 200;

                        rotationVelocity = Mathf.Lerp(gs.players[i].rotationVelocity, targetRotation, 1 - Mathf.Exp(-gameParameters.RotationDecelerationSpeed));
                        velocity = new Vector2(Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)),
                            Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)));

                        //RotateRightAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref  oldPlayer, i);
                        //gs.players[i] = oldPlayer;
                        //Shoot(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);

                        if (gs.currentGameStep - gs.players[i].lastShootStep > gameParameters.ShootDelay)
                        {
                            long lastShootStep = gs.currentGameStep;
                            gs.projectiles.Add(new Projectile
                            {
                                position = gs.players[i].position,
                                speed = gameParameters.ProjectileSpeed,
                                direction = gs.players[i].lookDirection.normalized,
                                playerID = i
                            });

                            Player oldPlayerTmp = gs.players[i];
                            gs.players[i] = createPlayer(oldPlayerTmp.score, oldPlayerTmp.speed, oldPlayerTmp.position,
                            lastShootStep, oldPlayerTmp.isGameOver, oldPlayerTmp.velocity, oldPlayerTmp.rotationVelocity, oldPlayerTmp.lookDirection);
                        }
                        break;
                    }

                case ActionsTypes.RotateLeftNS:
                    {
                        var targetRotation = gs.players[i].rotationVelocity + gameParameters.RotationAccelerationSpeed * 200;

                        rotationVelocity = Mathf.Lerp(gs.players[i].rotationVelocity, targetRotation, 1 - Mathf.Exp(-gameParameters.RotationDecelerationSpeed));
                        velocity = new Vector2(Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)),
                            Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)));

                        gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, oldPlayer.lookDirection);
                        break;
                    }
                case ActionsTypes.RotateLeftS:
                    {
                        RotateLeftAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        Shoot(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        break;
                    }

                case ActionsTypes.MoveUpNS:
                    {
                        var targetVel = gs.players[i].velocity + gs.players[i].lookDirection * gameParameters.AccelerationSpeed * 200;
                        velocity = Vector2.Lerp(gs.players[i].velocity, targetVel, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed));

                        gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                    oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, oldPlayer.rotationVelocity, oldPlayer.lookDirection);
                        break;
                    }
                case ActionsTypes.MoveUpS:
                    {
                        RotateLeftAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        Shoot(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        break;
                    }
                case ActionsTypes.MoveDownNS:
                    {
                        var targetVel = gs.players[i].velocity - gs.players[i].lookDirection * gameParameters.AccelerationSpeed * 200;
                        velocity = Vector2.Lerp(gs.players[i].velocity, targetVel, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed));

                        gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
                        oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, oldPlayer.rotationVelocity, oldPlayer.lookDirection);
                        break;
                    }
                case ActionsTypes.MoveDownS:
                    {
                        MoveDownAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        Shoot(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        break;
                    }
            }
        }

        for (int i = 0; i < 2; i++)
        {

            Player oldPlayer = gs.players[i];

            Vector2 position = oldPlayer.position;
            position += gs.players[i].velocity;
            //Debug.LogFormat("position oldplayer{0} :{1}, velocity{2}", i, oldPlayer.position, gs.players[i].velocity);
            Vector2 lookDirection = Quaternion.Euler(0, 0, gs.players[i].rotationVelocity) * gs.players[i].lookDirection;

            Vector2 velocity = Vector2.ClampMagnitude(gs.players[i].velocity, gameParameters.MaxVelocity);
            float rotationVelocity = Mathf.Clamp(gs.players[i].rotationVelocity, -2, 2);

            gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, position,
                   oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, lookDirection);
        }

    }

    //Movement when rotate right
    static private void RotateRightAgent(ref GameParametersStruct gameParameters, ref GameState gs, float rotationVelocity, Vector2 velocity, ref Player oldPlayer, int i)
    {
        var targetRotation = gs.players[i].rotationVelocity - gameParameters.RotationAccelerationSpeed * 200;

        rotationVelocity = Mathf.Lerp(gs.players[i].rotationVelocity, targetRotation, 1 - Mathf.Exp(-gameParameters.RotationDecelerationSpeed));
        velocity = new Vector2(Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)),
            Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)));

        gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
        oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, oldPlayer.lookDirection);

    }
    //rotate left 
    static private void RotateLeftAgent(ref GameParametersStruct gameParameters, ref GameState gs, float rotationVelocity, Vector2 velocity, ref Player oldPlayer,  int i)
    {
        var targetRotation = gs.players[i].rotationVelocity + gameParameters.RotationAccelerationSpeed * 200;

        rotationVelocity = Mathf.Lerp(gs.players[i].rotationVelocity, targetRotation, 1 - Mathf.Exp(-gameParameters.RotationDecelerationSpeed));
        velocity = new Vector2(Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)),
            Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)));

        gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
        oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, oldPlayer.lookDirection);
    }
    
    //Movement when up
    static private void MoveUpAgent(ref GameParametersStruct gameParameters, ref GameState gs, float rotationVelocity, Vector2 velocity, ref Player oldPlayer, int i)
    {
        var targetVel = gs.players[i].velocity + gs.players[i].lookDirection * gameParameters.AccelerationSpeed * 200;
        velocity = Vector2.Lerp(gs.players[i].velocity, targetVel, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed));

        gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
        oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, oldPlayer.rotationVelocity, oldPlayer.lookDirection);
  
    }
    //Movement when down
    static private void MoveDownAgent(ref GameParametersStruct gameParameters, ref GameState gs, float rotationVelocity, Vector2 velocity, ref Player oldPlayer, int i)
    {
        var targetVel = gs.players[i].velocity - gs.players[i].lookDirection * gameParameters.AccelerationSpeed * 200;
        velocity = Vector2.Lerp(gs.players[i].velocity, targetVel, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed));

        gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
        oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, oldPlayer.rotationVelocity, oldPlayer.lookDirection);
    }

    //Shoot
    static private void Shoot(ref GameParametersStruct gameParameters, ref GameState gs, float rotationVelocity, Vector2 velocity, ref Player oldPlayer, int i)
    {
        rotationVelocity = Mathf.Lerp(gs.players[i].rotationVelocity, 0, 1 - Mathf.Exp(-gameParameters.RotationDecelerationSpeed));
        velocity = new Vector2(Mathf.Lerp(gs.players[i].velocity.x, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)),
            Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)));

        gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, oldPlayer.position,
        oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, oldPlayer.lookDirection);

        if (gs.currentGameStep - gs.players[i].lastShootStep > gameParameters.ShootDelay)
        {
            long lastShootStep = gs.currentGameStep;
            gs.projectiles.Add(new Projectile
            {
                position = gs.players[i].position,
                speed = gameParameters.ProjectileSpeed,
                direction = gs.players[i].lookDirection.normalized,
                playerID = i
            });

            Player oldPlayerTmp = gs.players[i];
            gs.players[i] = createPlayer(oldPlayerTmp.score, oldPlayerTmp.speed, oldPlayerTmp.position,
            lastShootStep, oldPlayerTmp.isGameOver, oldPlayerTmp.velocity, oldPlayerTmp.rotationVelocity, oldPlayerTmp.lookDirection);
        }
    }

    public void GameOver(ref GameState gs)
    {

    }
    [NativeDisableParallelForRestriction]
    private static NativeArray<ActionsTypes> AvailableActions = new NativeArray<ActionsTypes>(9, Allocator.Persistent);

    public static NativeArray<ActionsTypes> GetAvailableActions(ref GameState gs)
    {
        AvailableActions[0] = ActionsTypes.Nothing;
        AvailableActions[1] = ActionsTypes.RotateRightNS;
        AvailableActions[2] = ActionsTypes.RotateRightS;
        AvailableActions[3] = ActionsTypes.RotateLeftNS;
        AvailableActions[4] = ActionsTypes.RotateLeftS;
        AvailableActions[5] = ActionsTypes.MoveDownNS;
        AvailableActions[6] = ActionsTypes.MoveDownS;
        AvailableActions[7] = ActionsTypes.MoveUpNS;
        AvailableActions[8] = ActionsTypes.MoveUpS;
        return AvailableActions;
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



