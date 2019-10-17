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
            currentDepth = 0,
            maxDepth = 1,
            gameParameters = GameParameters.Instance.Parameters,
            playerPos = gs.players[playerId == 0 ? 1 : 0].position,
            projectilePos = new float2(500f, 500f),
            bestFrame = 500
        };

        var handle = job.Schedule(availableActions.Length, 2);
        handle.Complete();

        ActionsTypes chosenAction = availableActions[job.indexChoosenAction];
        return chosenAction;

        ////Lancer le job
        //ActionsTypes chosenAction = availableActions[bestActionIndex];
        //job.summedScores.Dispose();

        //return chosenAction;
    }

    //public struct Node

    [BurstCompile]
    struct AStarJob : IJobParallelFor
    {
        public GameState gs;

        public GameParametersStruct gameParameters;

        [ReadOnly]
        public NativeArray<ActionsTypes> availableActions;
        
        public int currentDepth;
        public float2 playerPos;
        public int playerId;
        //max frame count 
        public float bestFrame;

        public float2 projectilePos;

        public int stepBestAction;

        [WriteOnly]
        public int indexChoosenAction;

        public int maxDepth;

        public void Execute(int index)
        {
            var gsCopy = Rules.Clone(ref gs);

            //Copy GameState
            Rules.CopyTo(ref gs, ref gsCopy);

            while (!gsCopy.players[0].isGameOver || !gsCopy.players[1].isGameOver || currentDepth != maxDepth)
            {
                for (var n = 0; n < availableActions.Length; n++)
                {
                    //Fait la première action
                    Rules.Step(ref gameParameters, ref gsCopy, availableActions[n], ActionsTypes.Nothing);
                    Debug.Log("dans le job step for");
                    //Calcul du nb de frame 
                    float Frames = CalculateFrames();

                    if (bestFrame > Frames)
                    {
                        stepBestAction = n;
                        bestFrame = Frames;
                    }
                }

                Rules.Step(ref gameParameters, ref gsCopy, availableActions[stepBestAction], ActionsTypes.Nothing);

                if (stepBestAction == 2 || stepBestAction == 4 || stepBestAction == 6 || stepBestAction == 8)
                {
                    if (gsCopy.projectiles[gsCopy.projectiles.Length - 1].playerID == playerId || gsCopy.projectiles[gsCopy.projectiles.Length - 2].playerID == playerId)
                    {
                        projectilePos = gsCopy.projectiles[gsCopy.projectiles.Length - 1].position;
                    }
                }

                currentDepth++;
            }

            gsCopy.projectiles.Dispose();
        }

        public float CalculateFrames()
        {
            float distProjectilePlayer = math.distance(playerPos, projectilePos);
            return gs.currentGameStep + distProjectilePlayer/gameParameters.ProjectileSpeed;
        }
    }
}
