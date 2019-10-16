using UnityEngine;
using Unity.Collections;
//using Unity.Mathematics;

//Auteur : Margot & Arthur
//Modifications : Attika

public struct RandomAgent : IAgent
{
    public Unity.Mathematics.Random rdm;

    public NativeArray<ActionsTypes> Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int id = 0)
    {
        NativeArray<ActionsTypes> actions = new NativeArray<ActionsTypes>(3, Allocator.Temp);

        actions[0] = availableActions[rdm.NextInt(0, 2)];
        actions[1] = availableActions[rdm.NextInt(0, 4)];
        actions[2] = availableActions[rdm.NextInt(0, 7)];

        return actions;
    }
}