using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

//Auteur : Margot & Arthur
//Modifications : Attika

public struct RandomAgent : IAgent
{
    public Unity.Mathematics.Random rdm;

    public NativeArray<ActionsTypes> Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int id = 0)
    {
        NativeArray<ActionsTypes> actions = new NativeArray<ActionsTypes>(3, Allocator.Temp);
        //actions[0] = availableActions[1];// availableActions[1];//;[rdm.NextInt(0, 3)];
        //actions[1] = availableActions[1];//availableActions[rdm.NextInt(3, 5)];
        //actions[2] = availableActions[1];// availableActions[rdm.NextInt(5, 6)];
        rdm = new Unity.Mathematics.Random(212123);

        actions[0] = availableActions[rdm.NextInt(0, 3)];
        actions[1] = availableActions[rdm.NextInt(3, 5)];
        actions[2] = availableActions[rdm.NextInt(5, 6)];

        return actions;
    }
}