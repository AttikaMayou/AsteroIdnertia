using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Random = Unity.Mathematics.Random;
using Rules = GameStateRules;

//Auteur: Margot

public struct MCTSAgent : IAgent
{
    public struct NodeMTCS
    {
        public ActionsTypes action;
        public int nc;
        public long rc;
        public int npc;
    }

    public struct SelectedNodeInfo
    {
        public long hash;
        public int nodeIndex;
    }


    public ActionsTypes Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int playerId)
    {
        var job = new MCTSAgentJob
        {
            availableActions = availableActions,
            gs = gs,
            summedScores = new NativeArray<long>(availableActions.Length, Allocator.TempJob),
            rdmAgent = new RandomAgent { rdm = new Random((uint)Time.frameCount + (uint)playerId) },
            playerId = playerId,
            gameParameters = GameParameters.Instance.Parameters
        };

        var handle = job.Schedule();
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
        /*
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
        */

        var chosenAction = availableActions[bestActionIndex];

        return chosenAction;
    }

    [BurstCompile]
    struct MCTSAgentJob : IJob
    {
        public GameState gs;

        public GameParametersStruct gameParameters;

        [ReadOnly]
        public NativeArray<ActionsTypes> availableActions;

        public RandomAgent rdmAgent;

        [WriteOnly]
        public NativeArray<long> summedScores;

        public int playerId;

        public void Execute()
        {
            var epochs = 200;
            var agent = rdmAgent;

            var gsCopy = Rules.Clone(ref gs);

            var rootHash = Rules.GetHashCode(ref gsCopy, playerId);

            //mémoire player 1 et 2 
            var memory1 = new NativeHashMap<long, NativeList<NodeMTCS>>(2048, Allocator.Temp);
            var memory2 = new NativeHashMap<long, NativeList<NodeMTCS>>(2048, Allocator.Temp);

            //Tableau de memory
            var memory = new NativeList<NativeHashMap<long, NativeList<NodeMTCS>>>(2048, Allocator.Temp);
            memory[playerId].TryAdd(rootHash, new NativeList<NodeMTCS>(availableActions.Length, Allocator.Temp));

            //remplir les memory1 et 2 avec des availablesActions
            //boucle pour les deux players
            for (int j = 0; j < 2;)
            {
                for (var i = 0; i < availableActions.Length; i++)
                {
                    memory[j][rootHash].Add(new NodeMTCS
                    {
                        action = availableActions[i],
                        nc = 0,
                        npc = 0,
                        rc = 0
                    });
                }
            }


            for (var n = 0; n < epochs; n++)
            {
                Rules.CopyTo(ref gs, ref gsCopy);
                var currentHash = rootHash;

                var selectedNodes = new NativeList<SelectedNodeInfo>(Allocator.Temp);

                //SELECT
                while (!gsCopy.players[0].isGameOver || !gsCopy.players[1].isGameOver)
                {
                    var hasUnexploredNodesTreePlayer1 = false;
                    var hasUnexploredNodesTreePlayer2 = false;

                    for (var i = 0; i < memory1[currentHash].Length; i++)
                    {
                        if (memory1[currentHash][i].nc == 0)
                        {
                            hasUnexploredNodesTreePlayer1 = true;
                            break;
                        }
                    }

                    for (var i = 0; i < memory2[currentHash].Length; i++)
                    {
                        if (memory2[currentHash][i].nc == 0)
                        {
                            hasUnexploredNodesTreePlayer2 = true;
                            break;
                        }
                    }
                    if (hasUnexploredNodesTreePlayer1 && hasUnexploredNodesTreePlayer2)
                    {
                        break;
                    }

                    var bestNodeIndexPlayer1 = -1;
                    var bestNodeScorePlayer1 = float.MinValue;
                    var bestNodeIndexPlayer2 = -1;
                    var bestNodeScorePlayer2 = float.MinValue;


                    for (var i = 0; i < memory1[currentHash].Length; i++)
                    {
                        var list = memory1[currentHash];
                        var node = list[i];
                        node.npc += 1;
                        list[i] = node;
                        memory1[currentHash] = list;

                        //faire la même chose pour le player 2
                        var score = (float)memory1[currentHash][i].rc / memory1[currentHash][i].nc
                                        + math.sqrt(2 * math.log(memory1[currentHash][i].npc) / memory1[currentHash][i].nc);

                        if (score >= bestNodeScorePlayer1)
                        {
                            bestNodeIndexPlayer1 = i;
                            bestNodeScorePlayer1 = score;
                        }
                    }

                    selectedNodes.Add(new SelectedNodeInfo
                    {
                        hash = currentHash,
                        nodeIndex = bestNodeIndexPlayer1
                    });

                    //TODO : best node index player 2





                    Rules.Step(ref gameParameters, ref gsCopy, memory1[currentHash][bestNodeIndexPlayer1].action, memory2[currentHash][bestNodeIndexPlayer2].action);
                    currentHash = Rules.GetHashCode(ref gsCopy, playerId);

                    if (!memory1.ContainsKey(currentHash))
                    {
                        memory1.TryAdd(currentHash, new NativeList<NodeMTCS>(availableActions.Length, Allocator.Temp));

                        for (var i = 0; i < availableActions.Length; i++)
                        {
                            memory1[currentHash]
                                .Add(new NodeMTCS
                                {
                                    action = availableActions[i],
                                    nc = 0,
                                    npc = 0,
                                    rc = 0
                                });
                        }
                    }
                }
                
                






                
            }
        }
    }
}
