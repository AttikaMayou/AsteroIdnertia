using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Rules = GameStateRules;
using Random = Unity.Mathematics.Random;
using Unity.Burst;
using Unity.Mathematics;

public struct NodeASatr
{
    public float framesToTarget;
    public int action;
    public int previousAction;
}

public class AStarV2 : IAgent
{
    public ActionsTypes Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int playerId)
    {
        var job = new AStarJob
        {
            availableActions = availableActions,
            gs = gs,
            currentDepth = 0,
            maxDepth = 300,
            gameParameters = GameParameters.Instance.Parameters,
            projectilePos = new float2(500f, 500f),
            playerPos = gs.players[playerId == 0 ? 1 : 0].position,
            playerId = playerId,
            nodes = new NativeList<NodeASatr>(0, Allocator.TempJob),
            selectedNodes = new NativeList<int>(0, Allocator.TempJob),
            maxFrames = 500
        };

        var handle = job.Schedule();
        handle.Complete();

        ActionsTypes chosenAction = availableActions[job.indexChoosenAction];
        return chosenAction;
    }

    [BurstCompile]
    struct AStarJob : IJob
    {
        public GameState gs;

        public GameParametersStruct gameParameters;

        [ReadOnly]
        public NativeArray<ActionsTypes> availableActions;

        public NativeList<NodeASatr> nodes;
        public NativeList<int> selectedNodes;

        //nb max itérations
        public int currentDepth;
        public int maxDepth;
        public int maxFrames;
        public int currentFrame;

        public float2 projectilePos;
        public float2 playerPos;
        public int playerId;

        public NodeASatr bestStepAction;

        [WriteOnly]
        public int indexChoosenAction; //Action à return

        public void Execute()
        {
            var gsCopy = Rules.Clone(ref gs);

            //Init
            bestStepAction.action = -1;
            bestStepAction.framesToTarget = 50000;
            bestStepAction.previousAction = -1;
            
            Rules.Step(ref gameParameters, ref gs, ActionsTypes.MoveUpS, ActionsTypes.Nothing);

            while (!gs.players[0].isGameOver && !gs.players[1].isGameOver && currentFrame <= maxFrames)
            {
                //reset for next frame
                if(currentDepth >= maxDepth)
                {
                    selectedNodes.Clear();
                    nodes.Clear();
                    bestStepAction.action = -1;
                    bestStepAction.framesToTarget = 50000;
                    bestStepAction.previousAction = -1;
                }

                //Ajout des nouveaux noeuds dans la liste
                for (var i = 0; i < availableActions.Length; i++)
                {
                    //Copy GameState
                    Rules.CopyTo(ref gs, ref gsCopy);
                    Rules.Step(ref gameParameters, ref gsCopy, availableActions[i], ActionsTypes.Nothing);

                    NodeASatr tmpNode = new NodeASatr();
                    if(gsCopy.projectiles.Length != 0)
                        tmpNode.framesToTarget = CalculateFrames(gsCopy.players[playerId == 0 ? 1 : 0].position, gsCopy.projectiles[gsCopy.projectiles.Length - 1].position);//à vérifier
                    else
                        tmpNode.framesToTarget = CalculateFrames(gsCopy.players[playerId == 0 ? 1 : 0].position, new float2(500f, 500f));//à vérifier

                    tmpNode.action = i;
                    tmpNode.previousAction = bestStepAction.action;
                    nodes.Add(tmpNode);
                }

                //Choix de la meilleur action
                for (var i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].framesToTarget < bestStepAction.framesToTarget)
                    {
                        bestStepAction = nodes[i];
                    }
                }

                //Fait la meilleur action
                Rules.Step(ref gameParameters, ref gsCopy, availableActions[bestStepAction.action], ActionsTypes.Nothing);

                //Retire la meilleur action de la liste
                selectedNodes.Add(bestStepAction.action);
                nodes.RemoveAtSwapBack(bestStepAction.action);

                //return first action of the path
                for (int i = selectedNodes.Length - 1; i > 0; i--)
                {
                    if (selectedNodes[i - 1] == -1)
                    {
                        indexChoosenAction = selectedNodes[i];
                        break;
                    }
                }
                indexChoosenAction = 0;

                currentDepth++;
                currentFrame++;
            }

            
        }   

        public float CalculateFrames(float2 playerPos, float2 projectilePos)
        {
            float distProjectilePlayer = math.distance(playerPos, projectilePos);
            return gs.currentGameStep + distProjectilePlayer / gameParameters.ProjectileSpeed;
        }
    }
}
