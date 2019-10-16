using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Random = Unity.Mathematics.Random;
using Rules = GameStateRules;

//Auteur : Félix et Attika

public class RandomRollOut : IAgent
{
    public NativeArray<ActionsTypes> Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int playerId)
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

        int[] bestActionIndex = new int[] { -1, -1, -1 };
        int indexFound = 0;
        var currBestActionIdex = -1;
        var bestScoreValue = long.MinValue;
        var i = 0;

        while(bestActionIndex[2] == -1)
        {
            if (bestScoreValue > job.summedScores[i] || !isValidIndex(i))
            {
                i++;
                if (i >= job.summedScores.Length) i = 0;
                continue;
            }

            bestScoreValue = job.summedScores[i];
            currBestActionIdex = i;
            bestActionIndex[indexFound] = currBestActionIdex;
            indexFound += 1;
            i++;
            if (i >= job.summedScores.Length) i = 0;
        }

        /*for ( i = 0; i < job.summedScores.Length; i++)
        {
            if (bestScoreValue > job.summedScores[i] || !isValidIndex(i))
                continue;

            bestScoreValue = job.summedScores[i];
            currBestActionIdex = i;

            var bestActionId = i;
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

            if(bestActionId != -1)
                bestActionIndex[indexFound] = bestActionId;
            indexFound += 1;
        }*/

        bool isValidIndex(int index)
        {
            for (var j = 0; j < bestActionIndex.Length; j++)
            {
                if (bestActionIndex[j] == index)
                    return false;
            }
            return true;
        }

        job.summedScores.Dispose();

        NativeArray<ActionsTypes> tempReturnBestActionIndex = new NativeArray<ActionsTypes>(3, Allocator.Temp);
        tempReturnBestActionIndex[0] = availableActions[bestActionIndex[0]];
        tempReturnBestActionIndex[1] = availableActions[bestActionIndex[1]];
        tempReturnBestActionIndex[2] = availableActions[bestActionIndex[2]];
        return tempReturnBestActionIndex;
    }

   // [BurstCompile]
    struct RandomRolloutJob : IJobParallelFor
    {
        public GameState gs;

        [ReadOnly]
        public NativeArray<ActionsTypes> availableActions;

        public RandomAgent rdmAgent;

        //[WriteOnly]
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
                    agent.Act(ref gsCopy, availableActions, 0),
                    agent.Act(ref gsCopy, availableActions, 1));

                var currentDepth = 0;
                var maxIteration = 150;
                while(!gsCopy.players[0].isGameOver || !gsCopy.players[1].isGameOver)
                {
                    Rules.Step(ref gsCopy,
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
