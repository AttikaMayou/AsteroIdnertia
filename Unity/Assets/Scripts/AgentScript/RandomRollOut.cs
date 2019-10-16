using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Random = Unity.Mathematics.Random;
using Rules = GameStateRules;

//Auteur : Félix
//Modifications : Attika

public class RandomRollOut : IAgent
{
    public NativeList<ActionsTypes> Act(ref GameState gs, NativeList<ActionsTypes> availableActions, int playerId)
    {
        var job = new RandomRolloutJob
        {
            availableActions = availableActions,
            gs = gs,
            summedScores = new NativeArray<long>(availableActions.Length, Allocator.TempJob),
            rdmAgent = new RandomAgent { rdm = new Random((uint)Time.frameCount) },
            playerId = playerId
        };

        var handle = job.Schedule(availableActions.Length, 1);
        handle.Complete();

        int[] bestActionIndex = new int[3];
        for(var i = 0; i < job.summedScores.Length; i++)
        {
            var bestActionId = -1;
            var bestScore = long.MinValue;
            for (var j = 0; j < job.summedScores.Length; j++)
            {
                if (bestScore > job.summedScores[j] || !isValidIndex(j))
                {
                    continue;
                }

                bestScore = job.summedScores[i];
                bestActionId = i;
            }

            bestActionIndex[i] = bestActionId;
        }

        bool isValidIndex(int i)
        {
            for(var j = 0; j < bestActionIndex.Length; j++)
            {
                if (bestActionIndex[j] == i)
                    return false;
            }
            return true;
        }

        job.summedScores.Dispose();

        return new NativeList<ActionsTypes>
        {
            availableActions[bestActionIndex[0]],
            availableActions[bestActionIndex[1]],
            availableActions[bestActionIndex[2]]
        };
    }

    [BurstCompile]
    struct RandomRolloutJob : IJobParallelFor
    {
        public GameState gs;

        [ReadOnly]
        public NativeList<ActionsTypes> availableActions;

        public RandomAgent rdmAgent;

        [WriteOnly]
        public NativeArray<long> summedScores;

        public int playerId;

        public void Execute(int index)
        {
            var epochs = 100;
            var agent = rdmAgent;

            var gsCopy = Rules.Clone(ref gs);

            for(var n = 0; n < epochs; n++)
            {
                Rules.CopyTo(ref gs, ref gsCopy);
                Rules.Step(ref gsCopy, 
                    agent.Act(ref gsCopy, Rules.GetAvailableActions(ref gsCopy), 0),
                    agent.Act(ref gsCopy, Rules.GetAvailableActions(ref gsCopy), 1));

                var currentDepth = 0;
                var maxIteration = 150;
                while(!gsCopy.players[0].isGameOver || !gsCopy.players[1].isGameOver)
                {
                    Rules.Step(ref gsCopy,
                    agent.Act(ref gsCopy, Rules.GetAvailableActions(ref gsCopy), 0),
                    agent.Act(ref gsCopy, Rules.GetAvailableActions(ref gsCopy), 1));
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
