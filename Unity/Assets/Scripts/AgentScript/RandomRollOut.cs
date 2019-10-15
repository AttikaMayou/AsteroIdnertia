using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

//Auteur : Félix

public class RandomRollOut : IAgent
{
    public ActionsTypes[] Act(ref GameState gs, ActionsTypes[] availableActions, int playerId)
    {
        //TODO: implementer la fonction
        var epochs = 20;
        var agent = new RandomAgent();

        //TODO : Change to Long
        var summedScores = new NativeArray<float>(availableActions.Length, Allocator.Temp);

        for (var i = 0; i < availableActions.Length; i++)
        {
            for (var n = 0; n < epochs; n++)
            {
                var gsCopy = GameStateRules.Clone(ref gs);
                GameStateRules.Step(ref gsCopy,
                agent.Act(ref gsCopy, GameStateRules.GetAvailableActions(ref gs), 0),
                agent.Act(ref gsCopy, GameStateRules.GetAvailableActions(ref gs), 0));

                var currentDepth = 0;
                var maxIteration = 150;
                while (!gsCopy.players[playerId].isGameOver)
                {
                    GameStateRules.Step(ref gsCopy, agent.Act(ref gs, GameStateRules.GetAvailableActions(ref gs), 0),
                        agent.Act(ref gs, GameStateRules.GetAvailableActions(ref gs), 1));
                    currentDepth++;
                    if (currentDepth > maxIteration)
                    {
                        break;
                    }
                }

                summedScores[i] += gsCopy.players[playerId].score;
                gsCopy.projectiles.Dispose();
                gsCopy.asteroids.Dispose();

            }
        }
        int[] bestIndex = new int[3];
        for (int i = 0; i < 3; i++)
        {
            var bestActionIndex = -1;
            var bestScore = float.MinValue;
            for (int j = 0; j < summedScores.Length; j++)
            {
                if (bestScore > summedScores[j] || !IsValidIndex(j))
                    continue;

                bestScore = summedScores[j];
                bestActionIndex = j;

            }

            bestIndex[i] = bestActionIndex;
        }

        bool IsValidIndex(int i)
        {
            for (int j = 0; j < bestIndex.Length; j++)
            {
                if (bestIndex[j] == i)
                {
                    return false;
                }
            }
            return true;
        }


        return new ActionsTypes[] {
            availableActions[bestIndex[0]],
            availableActions[bestIndex[1]],
            availableActions[bestIndex[2]]
            };
    }
}
