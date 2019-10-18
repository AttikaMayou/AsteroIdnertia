using Unity.Collections;
using Unity.Jobs;
using Rules = GameStateRules;
using Random = Unity.Mathematics.Random;
using Unity.Burst;
using Unity.Mathematics;

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
            maxDepth = 500,
            gameParameters = GameParameters.Instance.Parameters,
            projectilePos = new float2(500f, 500f),
            playerPos = gs.players[playerId == 0 ? 1 : 0].position,
            playerId = playerId,
            nodes = new NativeList<NodeAStar>(0, Allocator.TempJob),
            selectedNodes = new NativeList<int>(0, Allocator.TempJob),
            indexChoosenAction = 0
    };

        var handle = job.Schedule();
        handle.Complete();

        ActionsTypes chosenAction = availableActions[job.indexChoosenAction];
        return chosenAction;
    }

    //[BurstCompile]
    struct AStarJob : IJob
    {
        public GameState gs;

        public GameParametersStruct gameParameters;

        [ReadOnly]
        public NativeArray<ActionsTypes> availableActions;

        public NativeList<NodeAStar> nodes;
        public NativeList<int> selectedNodes;

        //nb max itérations
        public int currentDepth;
        public int maxDepth;

        public float2 projectilePos;
        public float2 playerPos;
        public int playerId;
        public float2 enemyPlayerPos;

        public NodeAStar bestStepAction;

        [WriteOnly]
        public int indexChoosenAction; //Action à return

        public void Execute()
        {
            var gsCopy = Rules.Clone(ref gs);
            enemyPlayerPos = gsCopy.players[playerId == 0 ? 1 : 0].position;

            //Init best action
            bestStepAction = new NodeAStar();
            bestStepAction.action = -1;
            bestStepAction.framesToTarget = 50000;
            bestStepAction.previousAction = -1;
            bestStepAction.currentGs = gsCopy;

            //Boucle qui cherche le meilleur chemin
            while (!gs.players[0].isGameOver && !gs.players[1].isGameOver && currentDepth <= maxDepth)
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
                    for (int j = gs.projectiles.Length - 1; j >= 0; j--)
                    {
                        if (j == 0 && gs.projectiles[j].playerID != playerId)
                        {
                            projectilePos = new float2(500f, 500f);
                            break;
                        }

                        if (gs.projectiles[j].playerID != playerId)
                        {
                            continue;
                        }
                        projectilePos = gs.projectiles[gs.projectiles.Length - 1].position;
                        break;
                    }

                    //Création et ajout du nouveau noeud
                    NodeAStar tmpNode = new NodeAStar();
                    tmpNode.framesToTarget = CalculateFrames(enemyPlayerPos, projectilePos);//à vérifier
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
                if (CalculateFrames(enemyPlayerPos, projectilePos) == 0)
                {
                    //return first action of the path
                    for (int i = selectedNodes.Length - 1; i > 0; i--)
                    {
                        if (selectedNodes[i - 1] == -1)
                        {
                            indexChoosenAction = selectedNodes[i];
                            break;
                        }
                    }
                }
            }
        }

         ////Init projectile
         //           for (int i = gs.projectiles.Length - 1; i >= 0; i--)
         //           {
         //               if (i == 0 && gs.projectiles[i].playerID != playerId)
         //               {
         //                   projectilePos = new float2(500f, 500f);
         //                   break;
         //               }

         //               if (gs.projectiles[i].playerID != playerId)
         //               {
         //                   continue;
         //               }
         //               projectilePos = gs.projectiles[gs.projectiles.Length - 1].position;
         //               break;
         //           }

public float CalculateFrames(float2 playerPos, float2 projectilePos)
        {
            float distProjectilePlayer = math.distance(playerPos, projectilePos);
            return gs.currentGameStep + distProjectilePlayer / gameParameters.ProjectileSpeed;
        }
    }
}
