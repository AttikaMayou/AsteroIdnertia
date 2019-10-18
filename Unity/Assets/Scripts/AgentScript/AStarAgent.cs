using Unity.Collections;
using Unity.Jobs;
using Rules = GameStateRules;
using Random = Unity.Mathematics.Random;
using Unity.Burst;
using Unity.Mathematics;
using System.Collections.Generic;

//Auteur : Arthur
//Modifications : Attika

public struct NodeAStar
{
    public float framesToTarget;
    public int action;
    public int previousAction;
    public GameState currentGs;
}

public class AStarAgent : IAgent
{
    public ActionsTypes Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int playerId)
    {
        var job = new AStarJob
        {
            availableActions = availableActions,
            gs = gs,
            currentDepth = 0,
            maxDepth = 200,
            gameParameters = GameParameters.Instance.Parameters,
            playerPos = gs.players[playerId == 0 ? 1 : 0].position,
            playerId = playerId,
            indexChoosenAction = new NativeArray<int>(2048, Allocator.TempJob)
    };

        var handle = job.Schedule();
        handle.Complete();

        ActionsTypes chosenAction = availableActions[job.indexChoosenAction[0]];
        return chosenAction;
    }

    //[BurstCompile]
    struct AStarJob : IJob
    {
        public GameState gs;

        public GameParametersStruct gameParameters;

        [ReadOnly]
        public NativeArray<ActionsTypes> availableActions;

        //nb max itérations
        public int currentDepth;
        public int maxDepth;

        public float2 projectilePos;
        public float2 playerPos;
        public int playerId;
        public float2 enemyPlayerPos;


        [WriteOnly]
        public NativeArray<int> indexChoosenAction; //Action à return

        public Unity.Mathematics.Random rdm;

        public void Execute()
        {
            var gsCopy = Rules.Clone(ref gs);
            enemyPlayerPos = gsCopy.players[playerId == 0 ? 1 : 0].position;

            //Init best action
            var bestStepAction = new NodeAStar();
            bestStepAction.action = -1;
            bestStepAction.framesToTarget = 50000;
            bestStepAction.previousAction = -1;
            bestStepAction.currentGs = gsCopy;

            var nodes = new List<NodeAStar>();
            var selectedNodes = new List<int>();

            //Boucle qui cherche le meilleur chemin
            while (!gsCopy.players[0].isGameOver && !gsCopy.players[1].isGameOver && currentDepth <= maxDepth)
            {
                //Ajout des nouveaux noeuds dans la liste
                for (var i = 0; i < availableActions.Length; i++)
                {
                    //Copy GameState
                    Rules.CopyTo(ref bestStepAction.currentGs, ref gsCopy);

                    if (playerId == 0)
                        Rules.Step(ref gameParameters, ref gsCopy, availableActions[i], ActionsTypes.Nothing);
                    else
                        Rules.Step(ref gameParameters, ref gsCopy, ActionsTypes.Nothing, availableActions[i]);

                    //Updtate used projectile
                    projectilePos = gsCopy.players[playerId].position;
                    for (int j = gsCopy.projectiles.Length - 1; j >= 0; j--)
                    {
                        if (gsCopy.projectiles[j].playerID != playerId)
                        {
                            continue;
                        }
                        projectilePos = gsCopy.projectiles[gsCopy.projectiles.Length - 1].position;
                        break;
                    }

                    //Création et ajout du nouveau noeud
                    NodeAStar tmpNode = new NodeAStar();
                    tmpNode.framesToTarget = CalculateFrames(enemyPlayerPos, projectilePos, gsCopy);//à vérifier
                    tmpNode.action = i;
                    tmpNode.previousAction = bestStepAction.action;
                    tmpNode.currentGs = gsCopy;
                    nodes.Add(tmpNode);
                }

                //Choix de la meilleur action
                for (var i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].framesToTarget < bestStepAction.framesToTarget)
                    {
                        bestStepAction = nodes[i];
                    }
                }

                //Fait la meilleur action
                if (playerId == 0)
                    Rules.Step(ref gameParameters, ref gsCopy, availableActions[bestStepAction.action], ActionsTypes.Nothing);
                else
                    Rules.Step(ref gameParameters, ref gsCopy, ActionsTypes.Nothing, availableActions[bestStepAction.action]);

                
                selectedNodes.Add(bestStepAction.action);
                //Retire la meilleure action de la liste
                nodes.RemoveAtSwapBack(bestStepAction.action);

                //Increment current number of iteration
                currentDepth++;

                //end
                if (CalculateFrames(enemyPlayerPos, projectilePos, bestStepAction.currentGs) == 0)
                {
                    //return first action of the path
                    for (int i = selectedNodes.Count - 1; i > 0; i--)
                    {
                        if (selectedNodes[i - 1] == -1)
                        {
                            indexChoosenAction[0] = selectedNodes[i];
                            break;
                        }
                    }
                }
            }
        }

public float CalculateFrames(float2 playerPos, float2 projectilePos, GameState currentGs)
        {
            float distProjectilePlayer = math.distance(playerPos, projectilePos);
            return currentGs.currentGameStep + distProjectilePlayer / gameParameters.ProjectileSpeed;
        }
    }
}
