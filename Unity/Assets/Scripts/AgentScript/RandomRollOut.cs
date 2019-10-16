using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Random = Unity.Mathematics.Random;
using Rules = GameStateRules;

//Auteur : Félix et Attika
//Modifications : Margot

public struct RandomRollOut : IAgent
{
    public ActionsTypes Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int playerId)
    {
        var job = new RandomRolloutJob
        {
            availableActions = availableActions,
            gs = gs,
            summedScores = new NativeArray<long>(availableActions.Length, Allocator.TempJob),
            rdmAgent = new RandomAgent { rdm = new Random((uint)Time.frameCount) },
            playerId = playerId,
            gameParameters = GameParameters.Instance.Parameters
        };

        var handle = job.Schedule(availableActions.Length, 2);
        handle.Complete();

        int bestActionIndex = -1;
        var bestScore = long.MinValue;

        for (var i = 0; i < job.summedScores.Length; i++)
        {
            if (bestScore > job.summedScores[i])
            {
                continue;
            }

            bestScore = job.summedScores[i];
            bestActionIndex = i;
        }

        ActionsTypes chosenAction = availableActions[bestActionIndex];
        job.summedScores.Dispose();

        return chosenAction;
    }

    [BurstCompile]
    struct RandomRolloutJob : IJobParallelFor
    {
        public GameState gs;

        public GameParametersStruct gameParameters;

        [ReadOnly]
        public NativeArray<ActionsTypes> availableActions;

        public RandomAgent rdmAgent;

        [WriteOnly]
        public NativeArray<long> summedScores;

        public int playerId;

        public void Execute(int index)
        {
            var epochs = 5;
            var agent = rdmAgent;

            var gsCopy = Rules.Clone(ref gs);

            for(var n = 0; n < epochs; n++)
            {
                Rules.CopyTo(ref gs, ref gsCopy);
                Rules.Step(ref gameParameters, ref gsCopy, availableActions[index], 0);

                //Rules.Step(ref gameParameters, ref gsCopy, availableActions[index], 0);
                    //agent.Act(ref gsCopy, availableActions[0], 0),
                    //agent.Act(ref gsCopy, availableActions, 1));

                var currentDepth = 0;
                var maxIteration = 500;
                while(!gsCopy.players[0].isGameOver || !gsCopy.players[1].isGameOver)
                {
                    Rules.Step(ref gameParameters, ref gsCopy,
                    agent.Act(ref gsCopy, availableActions, 0),
                    agent.Act(ref gsCopy, availableActions, 1));
                    currentDepth++;
                    if(currentDepth > maxIteration)
                    {
                        break;
                    }
                }
                
                summedScores[index] += gsCopy.players[playerId].score;
                gsCopy.projectiles.Dispose();
                gsCopy.asteroids.Dispose();
            }
        }
    }
}
