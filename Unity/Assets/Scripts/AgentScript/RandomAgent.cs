using UnityEngine;
using Unity.Collections;
//using Unity.Mathematics;

//Auteur : Margot & Arthur
//Modifications : Attika

public struct RandomAgent : IAgent
{
    //public Unity.Mathematics.Random rdm;

    public NativeArray<ActionsTypes> Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int id = 0)
    {
        NativeArray<ActionsTypes> actions = new NativeArray<ActionsTypes>(3, Allocator.Temp);
        //rdm = new Unity.Mathematics.Random(250);

        actions[0] = availableActions[Random.Range(0, 3)]; //rdm.NextInt
        actions[1] = availableActions[Random.Range(3, 5)];
        actions[2] = availableActions[Random.Range(5, 6)];

        return actions;
    }
}