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
    public struct NodeMCTS
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

        var chosenAction = availableActions[bestActionIndex];
        Debug.Log("chosenAction:" + availableActions[bestActionIndex]);
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

            var rootHashPlayer1 = Rules.GetHashCode(ref gsCopy, 0);
            //var rootHashPlayer2 = Rules.GetHashCode(ref gsCopy, 1);

            //mémoire player 1 et 2 
            var memory1 = new NativeHashMap<long, NativeList<NodeMCTS>>(2048, Allocator.Temp);
            var memory2 = new NativeHashMap<long, NativeList<NodeMCTS>>(2048, Allocator.Temp);

            //Tableau de memory
            var memory = new NativeList<NativeHashMap<long, NativeList<NodeMCTS>>>(2048, Allocator.Temp);

            //remplir le tableau de memory 
            for (int j = 0; j < 2; j++)
            {
                memory[j].TryAdd(rootHashPlayer1, new NativeList<NodeMCTS>(availableActions.Length, Allocator.Temp));
            }

            //remplir les memory1 et 2 avec des availablesActions
            //boucle pour les deux players
            for (int j = 0; j < 2;)
            {
                for (var i = 0; i < availableActions.Length; i++)
                {
                    memory[j][rootHashPlayer1].Add(new NodeMCTS
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
                var currentHash = rootHashPlayer1;

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
                }
                /*
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
                    memory1.TryAdd(currentHash, new NativeList<NodeMCTS>(availableActions.Length, Allocator.Temp));

                    for (var i = 0; i < availableActions.Length; i++)
                    {
                        memory1[currentHash]
                            .Add(new NodeMCTS
                            {
                                action = availableActions[i],
                                nc = 0,
                                npc = 0,
                                rc = 0
                            });
                    }
                }
            }


            //EXPAND
            if (!gsCopy.players[0].isGameOver || !gsCopy.players[1].isGameOver)
            {
                var unexploredActions = new NativeList<int>(Allocator.Temp);

                for (var i = 0; i < memory1[currentHash].Length; i++)
                {
                    if (memory1[currentHash][i].nc == 0)
                    {
                        unexploredActions.Add(i);
                    }
                }

                var chosenNodeIndexPlayer1 = agent.rdm.NextInt(0, unexploredActions.Length);

                selectedNodes.Add(new SelectedNodeInfo
                {
                    hash = currentHash,
                    nodeIndex = unexploredActions[chosenNodeIndexPlayer1]
                });

                Rules.Step(ref gameParameters, ref gsCopy, memory1[currentHash][unexploredActions[chosenNodeIndexPlayer1]].action, memory2[currentHash][unexploredActions[chosenNodeIndexPlayer1]].action);
                currentHash = Rules.GetHashCode(ref gsCopy, playerId);

                if (!memory1.ContainsKey(currentHash))
                {
                    memory1.TryAdd(currentHash, new NativeList<NodeMCTS>(availableActions.Length, Allocator.Temp));

                    for (var i = 0; i < availableActions.Length; i++)
                    {
                        memory1[currentHash]
                            .Add(new NodeMCTS
                            {
                                action = availableActions[i],
                                nc = 0,
                                npc = 0,
                                rc = 0
                            });
                    }
                }
            }

            //SIMULATE
            while (!gsCopy.players[0].isGameOver || !gsCopy.players[1].isGameOver)
            {
                ActionsTypes chosenActionIndexPlayer1 = (ActionsTypes)agent.rdm.NextInt(0, availableActions.Length);
                ActionsTypes chosenActionIndexPlayer2 = (ActionsTypes)agent.rdm.NextInt(0, availableActions.Length);

                Rules.Step(ref gameParameters, ref gsCopy, chosenActionIndexPlayer1, chosenActionIndexPlayer2);
            }

            //BACKPROPAGATE
            //pour les deux joueurs
            for (int j = 0; j < 2; j++)
            {
                for (var i = 0; i < selectedNodes.Length; i++)
                {
                    var list = memory1[selectedNodes[i].hash];
                    var node = list[selectedNodes[i].nodeIndex];

                    node.rc += gsCopy.players[playerId].score;
                    node.nc += 1;

                    list[selectedNodes[i].nodeIndex] = node;

                    memory1[selectedNodes[i].hash] = list;
                }
            }
            for (var i = 0; i < memory1[rootHash].Length; i++)
            {
                summedScores[i] = memory1[rootHash][i].nc;
            }
        }*/
            }
        }
        
    }
}
