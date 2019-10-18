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
            rdmAgent = new RandomAgent { rdm = new Random((uint)Time.frameCount + (uint)playerId) },
            playerId = playerId,
            gameParameters = GameParameters.Instance.Parameters
        };

        var handle = job.Schedule(availableActions.Length, 1);
        handle.Complete();

        int bestActionIndex = -1;
        var bestScore = long.MinValue;

        for (var i = 0; i < job.summedScores.Length; i++)
        {
            if (bestScore > job.summedScores[i])
            {
                continue;
            }
          //  Debug.Log(availableActions[i] + " : " + job.summedScores[i]);
            bestScore = job.summedScores[i];
            bestActionIndex = i;
        }
        // Check if all value are same
        var value = job.summedScores[0];
        bool same = true;
        for (int i = 1; i < job.summedScores.Length; i++)
        {
            same = job.summedScores[i] == value ? true : false;
            if (!same)
            {
                break;
            }
        }

        ActionsTypes chosenAction; 
        if (!same)
            chosenAction = availableActions[bestActionIndex];
        else
        {
            chosenAction = job.rdmAgent.Act(ref gs, availableActions);
        }
        if (playerId == 0)
            Debug.Log(chosenAction);
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
            var epochs = 100;
            var agent = rdmAgent;

            var gsCopy = Rules.Clone(ref gs);

            for (var n = 0; n < epochs; n++)
            {
                Rules.CopyTo(ref gs, ref gsCopy);
                Rules.Step(ref gameParameters, ref gsCopy, availableActions[index], 0);

                //Rules.Step(ref gameParameters, ref gsCopy, availableActions[index], 0);
                //agent.Act(ref gsCopy, availableActions[0], 0),
                //agent.Act(ref gsCopy, availableActions, 1));

                var currentDepth = 0;
                var maxIteration = 200;
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
    }
}
