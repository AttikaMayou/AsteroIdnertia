﻿using UnityEngine;
using Unity.Collections;
//Auteur : Félix
//Modifications : Margot, Arthur et Attika

public class GameStateRules : MonoBehaviour
{
    public static void Init(ref GameParametersStruct gameParameters, ref GameState gs, PlayerManager playerManager)
    {
        // Initialisation des players
        Debug.Log("Initialization");

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

        //Taille de la liste à déterminer 
        //Initialisation d'une liste pour les astéroïdes
        gs.asteroids = new NativeList<Asteroid>(100, Allocator.Persistent);

        //Taille de la liste à déterminer
        gs.projectiles = new NativeList<Projectile>(100, Allocator.Persistent);
    }

    //Generate a random position within the world
    private static Vector2 GetRandomPosition(ref GameParametersStruct gameParameters)
    {
        //Donne une position au hasard dans le monde entre les boundaries choisies (donc dans le carré de centre (0, 0, 0) et de côté "gameParameters.Boundary")
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(0x6E624EB7u);
        //var position = new Vector2(Random.Range(-gameParameters.Boundary, gameParameters.Boundary),
        //    Random.Range(-gameParameters.Boundary, gameParameters.Boundary));

        var position = new Vector2(r.NextFloat(-gameParameters.Boundary, gameParameters.Boundary),
            r.NextFloat(-gameParameters.Boundary, gameParameters.Boundary));
        //var position = new Vector2()
        //Si la position est comprise dans l'écran, on l'éloigne
        if (position.x < 90 && position.x > -90 &&
            position.y < 90 && position.y > -90) position.y += 100f;

        return position;
    }

    private static Vector2 GetDirection(Vector2 pos)
    {
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(0x6E624EB7u);
        var target = new Vector2(r.NextFloat(-50, 50),  0);
        return target;
    }

    private static float GetRandom(float min, float max)
    {
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(0x6E624EB7u);
        return r.NextFloat(min, max);
    }

    //Generate a new asteroid and add it to asteroids list
    private static void GenerateNewAsteroid(ref GameParametersStruct gameParameters, ref GameState gs)
    {
        //Récupère une random position 
        var position = GetRandomPosition(ref gameParameters);

        //Créé un nouvel astéroïde à la position précédemment obtenue, donne une direction globalement orientée vers le centre (0, 0, 0) à +/- 50°)
        var asteroid = new Asteroid
        {
            position = position,
            direction = GetDirection(position)
        };

        //Multiplication de la direction par une valeur au hasard entre la minimum et maximum speed autorisée pour les astéroïdes
        asteroid.direction = asteroid.direction.normalized * GetRandom(gameParameters.AsteroidMinimumSpeed, gameParameters.AsteroidMaximumSpeed);// Random.Range(gameParameters.AsteroidMinimumSpeed, gameParameters.AsteroidMaximumSpeed);
        //ajout de cet astéroïde dans la liste
        gs.asteroids.Add(asteroid);
    }

    public static void Step(ref GameParametersStruct gameParameters, ref GameState gs, ActionsTypes actionPlayer1, ActionsTypes actionPlayer2)
    {

        UpdateAsteroids(ref gameParameters, ref gs);
        UpdateProjectiles(ref gs);

        HandleAgentInputs(ref gameParameters, ref gs, actionPlayer1, actionPlayer2);

        if (gs.players[0].isGameOver || gs.players[1].isGameOver)
        {
            return;
        }
        else
        {
            HandleCollisions(ref gameParameters, ref gs);
        }
        gs.currentGameStep += 1;
        if (gs.currentGameStep - gs.scoreStepDelay < 100)
        {
            return;
        }
        gs.scoreStepDelay = gs.currentGameStep;
    }

    static void UpdateAsteroids(ref GameParametersStruct gameParameters, ref GameState gs)
    {
        //facteur de spawn d'astéroïdes, qui va augmenter de façon exponentielle (ou presque)
        var spawnAsteroid = gs.currentGameStep * 1.0f;
        if (gs.currentGameStep > 100 * 60)
            spawnAsteroid *= 0.2f;//Mathf.Pow(2.7f, gs.currentGameStep * 0.00005f);
        else
            spawnAsteroid *= 0.02f;
        //itération sur ce facteur de spawn : on génère autant d'astéroïdes qu'il en manque pour en avoir "spawnAsteroid" objets dans la liste
        for (var i = 0; i < spawnAsteroid - gs.asteroids.Length; i++)
        {
            GenerateNewAsteroid(ref gameParameters, ref gs);
        }

        //Update de la position des astéroïdes déjà existants
        for (var i = 0; i < gs.asteroids.Length; i++)
        {
            var asteroid = gs.asteroids[i];
            asteroid.position += -gs.asteroids[i].direction;
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
                    //Quand un astéroïde est détruit, on en génère un remplaçant
                    GenerateNewAsteroid(ref gameParameters, ref gs);
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
            else if (i == 1)
            {
                tempChosenActionPlayer = chosenPlayer2Actions;
            }
            //Debug.LogFormat("chosenPlayer1Actions : {0} / chosenPlayer2Actions : {1} / tempChosenPlayer : {2}", chosenPlayer1Actions, chosenPlayer2Actions, tempChosenActionPlayer);

            switch (tempChosenActionPlayer)
            {
                case ActionsTypes.Nothing:
                    {
                        DecelerateVelocity(ref gameParameters, ref gs, ref oldPlayer, i);
                        DecelerateRotation(ref gameParameters, ref gs, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        break;
                    }
                case ActionsTypes.RotateRightNS:
                    {
                        RotateRightAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        DecelerateVelocity(ref gameParameters, ref gs, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        break;
                    }

                case ActionsTypes.RotateRightS:
                    {

                        RotateRightAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        DecelerateVelocity(ref gameParameters, ref gs, ref oldPlayer, i);
                        Shoot(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        break;
                    }

                case ActionsTypes.RotateLeftNS:
                    {
                        RotateLeftAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        DecelerateVelocity(ref gameParameters, ref gs, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        break;
                    }
                case ActionsTypes.RotateLeftS:
                    {
                        RotateLeftAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        DecelerateVelocity(ref gameParameters, ref gs, ref oldPlayer, i);
                        Shoot(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        break;
                    }

                case ActionsTypes.MoveUpNS:
                    {
                        MoveUpAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        DecelerateRotation(ref gameParameters, ref gs, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        break;
                    }
                case ActionsTypes.MoveUpS:
                    {
                        MoveUpAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        DecelerateRotation(ref gameParameters, ref gs, ref oldPlayer, i);
                        Shoot(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        break;
                    }
                case ActionsTypes.MoveDownNS:
                    {
                        MoveDownAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        DecelerateRotation(ref gameParameters, ref gs, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;

                        break;
                    }
                case ActionsTypes.MoveDownS:
                    {
                        MoveDownAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        DecelerateRotation(ref gameParameters, ref gs, ref oldPlayer, i);
                        Shoot(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        break;
                    }
                case ActionsTypes.NothingS:
                    {
                        DecelerateRotation(ref gameParameters, ref gs, ref oldPlayer, i);
                        DecelerateVelocity(ref gameParameters, ref gs, ref oldPlayer, i);

                        Shoot(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        break;
                    }
                case ActionsTypes.RotateRightUp:
                    {
                        RotateRightAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        MoveUpAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        break;
                    }
                case ActionsTypes.RotateLeftUp:
                    {
                        RotateLeftAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        MoveUpAgent(ref gameParameters, ref gs, rotationVelocity, velocity, ref oldPlayer, i);
                        gs.players[i] = oldPlayer;
                        break;
                    }
            }
        }

        for (int i = 0; i < 2; i++)
        {

            Player oldPlayer = gs.players[i];

            Vector2 position = oldPlayer.position;
            Vector2 velocity = oldPlayer.velocity;
            if (AllowMovement(ref gameParameters, ref gs, position + gs.players[i].velocity))
                position += gs.players[i].velocity;
            else
            {
                velocity = new Vector2(0, 0);
            }
            Vector2 lookDirection = Quaternion.Euler(0, 0, gs.players[i].rotationVelocity) * gs.players[i].lookDirection;

            velocity = Vector2.ClampMagnitude(velocity, gameParameters.MaxVelocity);
            float rotationVelocity = Mathf.Clamp(gs.players[i].rotationVelocity, -2, 2);

            gs.players[i] = createPlayer(oldPlayer.score, oldPlayer.speed, position,
                   oldPlayer.lastShootStep, oldPlayer.isGameOver, velocity, rotationVelocity, lookDirection);
        }

    }

    //Movement when rotate right
    static private void RotateRightAgent(ref GameParametersStruct gameParameters, ref GameState gs, float rotationVelocity, Vector2 velocity, ref Player oldPlayer, int i)
    {
        var targetRotation = oldPlayer.rotationVelocity - gameParameters.RotationAccelerationSpeed;

        oldPlayer.rotationVelocity = Unity.Mathematics.math.lerp(oldPlayer.rotationVelocity, targetRotation, 1 - Unity.Mathematics.math.exp(-gameParameters.RotationAccelerationSpeed));
        //oldPlayer.velocity = new Vector2(Mathf.Lerp(oldPlayer.velocity.x, 0, 1 - Mathf.Exp(-gameParameters.RotationAccelerationSpeed)),
        //    Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)));

    }

    //rotate left 
    static private void RotateLeftAgent(ref GameParametersStruct gameParameters, ref GameState gs, float rotationVelocity, Vector2 velocity, ref Player oldPlayer, int i)
    {
        var targetRotation = oldPlayer.rotationVelocity + gameParameters.RotationAccelerationSpeed;

        oldPlayer.rotationVelocity = Unity.Mathematics.math.lerp(oldPlayer.rotationVelocity, targetRotation, 1 - Mathf.Exp(-gameParameters.RotationAccelerationSpeed));
        //oldPlayer.velocity = new Vector2(Mathf.Lerp(oldPlayer.velocity.x, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)),
        //    Mathf.Lerp(gs.players[i].velocity.y, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)));
    }

    //Movement when up
    static private void MoveUpAgent(ref GameParametersStruct gameParameters, ref GameState gs, float rotationVelocity, Vector2 velocity, ref Player oldPlayer, int i)
    {
        var targetVel = oldPlayer.velocity + oldPlayer.lookDirection * gameParameters.AccelerationSpeed * 200;
        oldPlayer.velocity = Unity.Mathematics.math.lerp(oldPlayer.velocity, targetVel, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed));


    }
    //Movement when down
    static private void MoveDownAgent(ref GameParametersStruct gameParameters, ref GameState gs, float rotationVelocity, Vector2 velocity, ref Player oldPlayer, int i)
    {
        var targetVel = oldPlayer.velocity - oldPlayer.lookDirection * gameParameters.AccelerationSpeed * 200;
        oldPlayer.velocity = Unity.Mathematics.math.lerp(oldPlayer.velocity, targetVel, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed));

    }

    //Shoot
    static private void Shoot(ref GameParametersStruct gameParameters, ref GameState gs, float rotationVelocity, Vector2 velocity, ref Player oldPlayer, int i)
    {

        if (gs.currentGameStep - oldPlayer.lastShootStep > gameParameters.ShootDelay)
        {
            oldPlayer.lastShootStep = gs.currentGameStep;
            gs.projectiles.Add(new Projectile
            {
                position = oldPlayer.position,
                speed = gameParameters.ProjectileSpeed,
                direction = oldPlayer.lookDirection.normalized,
                playerID = i
            });

            gs.players[i] = oldPlayer;
        }
    }

    static public void DecelerateVelocity(ref GameParametersStruct gameParameters, ref GameState gs, ref Player oldPlayer, int i)
    {
        oldPlayer.velocity = new Vector2(Unity.Mathematics.math.lerp(oldPlayer.velocity.x, 0, 1 - Unity.Mathematics.math.exp(-gameParameters.DecelerationSpeed)),
                           Unity.Mathematics.math.lerp(oldPlayer.velocity.y, 0, 1 - Mathf.Exp(-gameParameters.DecelerationSpeed)));

        if (oldPlayer.velocity.magnitude <= 0.05f)
        {
            oldPlayer.velocity = Vector2.zero;
        }
    }

    static public void DecelerateRotation(ref GameParametersStruct gameParameters, ref GameState gs, ref Player oldPlayer, int i)
    {
        Unity.Mathematics.math.lerp(oldPlayer.rotationVelocity, 0, 1 - Mathf.Exp(-gameParameters.RotationDecelerationSpeed));
       // oldPlayer.rotationVelocity = Mathf.Lerp(oldPlayer.rotationVelocity, 0, 1 - Mathf.Exp(-gameParameters.RotationDecelerationSpeed));

       
    }

    public void GameOver(ref GameState gs)
    {

    }

    [NativeDisableParallelForRestriction]
    private static NativeArray<ActionsTypes> AvailableActions = new NativeArray<ActionsTypes>(12, Allocator.Persistent);

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
        AvailableActions[9] = ActionsTypes.NothingS;
        AvailableActions[10] = ActionsTypes.RotateLeftUp;
        AvailableActions[11] = ActionsTypes.RotateRightUp;
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

    public static bool AllowMovement(ref GameParametersStruct gameParameters, ref GameState gs, Vector2 pos)
    {
        //Block players within screenBoundaries
        if (pos.x + gameParameters.PlayerRadius * 3 > gameParameters.ScreenBordersBoundaryX.y
            || pos.x - gameParameters.PlayerRadius * 3 < gameParameters.ScreenBordersBoundaryX.x
            || pos.y + gameParameters.PlayerRadius * 3 > gameParameters.ScreenBordersBoundaryY.y
            || pos.y - gameParameters.PlayerRadius * 3 < gameParameters.ScreenBordersBoundaryY.x)
        {
            return false;
        }

        return true;
    }



}



