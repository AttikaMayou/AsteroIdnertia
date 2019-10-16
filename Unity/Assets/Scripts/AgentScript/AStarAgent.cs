using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Rules = GameStateRules;
using Random = Unity.Mathematics.Random;
using Unity.Burst;
using Unity.Mathematics;

public class AStarAgent : IAgent
{
    public ActionsTypes Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int playerId)
    {
        var job = new AStarJob
        {
            availableActions = availableActions,
            gs = gs,
            score = 0,
            playerId = playerId,
            agent = new AStarAgent(),
            gameParameters = GameParameters.Instance.Parameters,
            asteroids = gs.asteroids
        };

        //Lancer le job

        return actionsToDo;
    }

    [BurstCompile]
    struct AStarJob : IJobParallelFor
    {
        public GameState gs;

        public GameParametersStruct gameParameters;

        [ReadOnly]
        public NativeArray<ActionsTypes> availableActions;

        public AStarAgent agent;

        public long score;

        public int playerId;

        public float2 playerPos;

        public float2 projectilePos;

        public NativeList<Asteroid> asteroids;

        public void Execute(int index)
        {
            var gsCopy = Rules.Clone(ref gs);

            for (var n = 0; n < availableActions.Length; n++)
            {
                //Copy GameState
                Rules.CopyTo(ref gs, ref gsCopy);

                //Fait la première action (Shoot)
                Rules.Step(ref gameParameters, ref gsCopy, ActionsTypes.MoveUpS, ActionsTypes.MoveUpS);

                //Calcul du nb de frame 
                float Distance = CalculateFrames();
                //Choix
                while (!gsCopy.players[0].isGameOver || !gsCopy.players[1].isGameOver)
                {
                    Rules.Step(ref gameParameters, ref gsCopy,
                    agent.Act(ref gsCopy, availableActions, 0),
                    agent.Act(ref gsCopy, availableActions, 1));
                    currentDepth++;
                    if (currentDepth > maxIteration)
                    {
                        break;
                    }
                }

                summedScores[index] += gsCopy.players[playerId].score;
                gsCopy.projectiles.Dispose();
                gsCopy.asteroids.Dispose();
            }
        }

        public float CalculateFrames()
        {
            float distProjectilePlayer = math.distance(playerPos, projectilePos);
            float frames = gs.currentGameStep + distProjectilePlayer/gameParameters.ProjectileSpeed;

            return frames;
        }
    }
}
