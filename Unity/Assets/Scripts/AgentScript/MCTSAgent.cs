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
        Debug.Log("chosenAction:" + chosenAction);
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
            var epochs = 20;
            var agent = rdmAgent;

            var gsCopy = Rules.Clone(ref gs);

            var rootHash = Rules.GetHashCode(ref gsCopy, 0);

            // CREATION DE LA MEMOIRE (Arbre)
            var memory = new NativeHashMap<long, NativeList<NodeMCTS>>(2048, Allocator.Temp);
            memory.TryAdd(rootHash, new NativeList<NodeMCTS>(availableActions.Length, Allocator.Temp));

            for (var i = 0; i < availableActions.Length; i++)
            {
                memory[rootHash]
                    .Add(new NodeMCTS
                    {
                        action = availableActions[i],
                        nc = 0,
                        npc = 0,
                        rc = 0
                    });
            }

            for (var n = 0; n < epochs; n++)
            {
                Rules.CopyTo(ref gs, ref gsCopy);
                var currentHash = rootHash;

                var selectedNodes = new NativeList<SelectedNodeInfo>(Allocator.Temp);

                //SELECT
                while (!gsCopy.players[0].isGameOver|| !gsCopy.players[1].isGameOver)
                {
                    var hasUnexploredNodes = false;

                    for (var i = 0; i < memory[currentHash].Length; i++)
                    {
                        if (memory[currentHash][i].nc == 0)
                        {
                            hasUnexploredNodes = true;
                            break;
                        }
                    }

                    if (hasUnexploredNodes)
                    {
                        break;
                    }

                    var bestNodeIndex = -1;
                    var bestNodeScore = float.MinValue;

                    for (var i = 0; i < memory[currentHash].Length; i++)
                    {
                        var list = memory[currentHash];
                        var node = list[i];
                        node.npc += 1;
                        list[i] = node;
                        memory[currentHash] = list;

                        var score = (float)memory[currentHash][i].rc / memory[currentHash][i].nc
                                    + math.sqrt(2 * math.log(memory[currentHash][i].npc) / memory[currentHash][i].nc);

                        if (score >= bestNodeScore)
                        {
                            bestNodeIndex = i;
                            bestNodeScore = score;
                        }
                    }

                    selectedNodes.Add(new SelectedNodeInfo
                    {
                        hash = currentHash,
                        nodeIndex = bestNodeIndex
                    });
                    Rules.Step(ref gameParameters, ref gsCopy, memory[currentHash][bestNodeIndex].action, availableActions[7]);
                    currentHash = Rules.GetHashCode(ref gsCopy, 0);

                    if (!memory.ContainsKey(currentHash))
                    {
                        memory.TryAdd(currentHash, new NativeList<NodeMCTS>(availableActions.Length, Allocator.Temp));

                        for (var i = 0; i < availableActions.Length; i++)
                        {
                            memory[currentHash]
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

                    for (var i = 0; i < memory[currentHash].Length; i++)
                    {
                        if (memory[currentHash][i].nc == 0)
                        {
                            unexploredActions.Add(i);
                        }
                    }

                    var chosenNodeIndex = agent.rdm.NextInt(0, unexploredActions.Length);

                    selectedNodes.Add(new SelectedNodeInfo
                    {
                        hash = currentHash,
                        nodeIndex = unexploredActions[chosenNodeIndex]
                    });
                    Rules.Step(ref gameParameters, ref gsCopy, memory[currentHash][unexploredActions[chosenNodeIndex]].action, availableActions[7]);
                    currentHash = Rules.GetHashCode(ref gsCopy, 0);

                    if (!memory.ContainsKey(currentHash))
                    {
                        memory.TryAdd(currentHash, new NativeList<NodeMCTS>(availableActions.Length, Allocator.Temp));

                        for (var i = 0; i < availableActions.Length; i++)
                        {
                            memory[currentHash]
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
                    var chosenActionIndex = agent.rdm.NextInt(0, availableActions.Length);
                    Rules.Step(ref gameParameters, ref gsCopy, (ActionsTypes)chosenActionIndex, availableActions[7]);
                }


                //BACKPROPAGATE
                for (var i = 0; i < selectedNodes.Length; i++)
                {
                    var list = memory[selectedNodes[i].hash];
                    var node = list[selectedNodes[i].nodeIndex];

                    //node.rc += gsCopy.score;
                    node.nc += 1;

                    list[selectedNodes[i].nodeIndex] = node;

                    memory[selectedNodes[i].hash] = list;
                }
            }

            for (var i = 0; i < memory[rootHash].Length; i++)
            {
                summedScores[i] = memory[rootHash][i].nc;
            }
        }
    }
}