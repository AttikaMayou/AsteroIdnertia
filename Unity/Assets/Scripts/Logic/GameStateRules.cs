using UnityEngine;
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

        var player = new Player
        {
            score = 0,
            speed = GameState.INITIAL_PLAYER_SPEED,
            position = Vector2.zero,
            lastShootStep = -GameState.SHOOT_DELAY,
            isGameOver = false
        };
        gs.player = player;
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
        for (var i = 0; i < gs.asteroids.Length; i++)
        {
            var asteroid = gs.asteroids[i];
            asteroid.position += gs.asteroids[i].speed;
            gs.asteroids[i] = asteroid;
        }
    }

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
                    break;
                }
            case ActionsTypes.MoveLeft:
                {
                    gs.player.position += Vector2.left * gs.player.speed * Vector2.left;
                    break;
                }

            case ActionsTypes.MoveRight:
                {
                    gs.player.position += Vector2.right * gs.player.speed * Vector2.left;
                    break;
                }
            case ActionsTypes.Shoot:
                {
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
    }

    private static readonly ActionsTypes[] AvailableActions = new[] { ActionsTypes.Nothing, ActionsTypes.MoveLeft, ActionsTypes.MoveRight, ActionsTypes.Shoot };

    public static ActionsTypes[] GetAvailableActions(ref GameState gs)
    {
        return AvailableActions;
    }



}



