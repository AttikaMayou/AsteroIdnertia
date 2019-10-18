﻿using Unity.Collections;
using Unity.Jobs;
using Random = Unity.Mathematics.Random;
using Rules = GameStateRules;
using UnityEngine;
using Unity.Mathematics;

//Auteur : Attika
public class MultiNiveauAgent : IAgent
{
    public ActionsTypes Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int playerId)
    {
        var longJob = new LongTermJob
        {
            availableActions = availableActions,
            gs = gs,
            rdmAgent = new RandomAgent { rdm = new Random((uint)Time.frameCount + (uint)playerId) },
            intermediateSteps = new NativeList<float2>(10, Allocator.TempJob),
            playerId = playerId,
            gameParameters = GameParameters.Instance.Parameters,
            summedScores = new NativeArray<float>(availableActions.Length, Allocator.TempJob),
            playerPos = gs.players[playerId == 0 ? 0 : 1].position

        };

        var handleLong = longJob.Schedule(availableActions.Length, 1);
        handleLong.Complete();

        NativeList<float2> stepsToDo = new NativeList<float2>(longJob.intermediateSteps.Length, Allocator.Temp);
        stepsToDo.AddRange(longJob.intermediateSteps);

        ActionsTypes choosenAction = availableActions[0];
        return choosenAction;
    }

    struct LongTermJob : IJobParallelFor
    {
        public GameState gs;

        public GameParametersStruct gameParameters;

        [ReadOnly]
        public NativeArray<ActionsTypes> availableActions;

        public RandomAgent rdmAgent;

        [WriteOnly]
        public NativeList<float2> intermediateSteps;

        public NativeArray<float> summedScores;

        public int playerId;
        public float2 projectilePos;
        public float2 enemyPos;
        public float2 playerPos;

        public void Execute(int index)
        {
            var epochs = 100;
            var agent = rdmAgent;

            var gsCopy = Rules.Clone(ref gs);

            for (var n = 0; n < epochs; n++)
            {
                Rules.CopyTo(ref gs, ref gsCopy);
                Rules.Step(ref gameParameters, ref gsCopy, availableActions[index], 0);

                var currentDepth = 0;
                var maxIteration = 1000;
                enemyPos = gsCopy.players[playerId == 0 ? 1 : 0].position;

                while (currentDepth < maxIteration || !gsCopy.players[1].isGameOver)
                {
                    Rules.Step(ref gameParameters, ref gsCopy,
                    agent.Act(ref gsCopy, availableActions, 0),
                    agent.Act(ref gsCopy, availableActions, 1));
                    currentDepth++;
                }

                //Init projectile
                for (int i = gs.projectiles.Length - 1; i >= 0; i--)
                {
                    if (gs.projectiles[i].playerID == playerId)
                        projectilePos = gs.projectiles[gs.projectiles.Length - 1].position;
                    else if (i == 0)
                        projectilePos = new float2(500f, 500f);
                }

                summedScores[index] = CalculateFrames(enemyPos, projectilePos);
                gsCopy.projectiles.Dispose();
                gsCopy.asteroids.Dispose();
            }

            int bestActionIndex = -1;
            var bestScore = float.MinValue;

            for (var i = 0; i < summedScores.Length; i++)
            {
                if (bestScore > summedScores[i])
                {
                    continue;
                }
                //  Debug.Log(availableActions[i] + " : " + job.summedScores[i]);
                bestScore = summedScores[i];
                bestActionIndex = i;
            }
            // Check if all value are same
            var value = summedScores[0];
            bool same = true;
            for (int i = 1; i < summedScores.Length; i++)
            {
                same = summedScores[i] == value ? true : false;
                if (!same)
                {
                    break;
                }
            }

            ActionsTypes chosenAction;
            if (!same)
                chosenAction = availableActions[bestActionIndex];
            else
                chosenAction = rdmAgent.Act(ref gs, availableActions);
            summedScores.Dispose();
        }

        public float CalculateFrames(float2 playerPos, float2 projectilePos)
        {
            float distance = math.distance(playerPos, projectilePos);
            return gs.currentGameStep + distance / gameParameters.ProjectileSpeed;
        }
    }
}
