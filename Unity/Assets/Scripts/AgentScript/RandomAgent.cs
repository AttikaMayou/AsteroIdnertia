using UnityEngine;
using Unity.Collections;

//Auteur : Margot & Arthur
//Modifications : Attika

public struct RandomAgent : IAgent
{
    public Unity.Mathematics.Random rdm;

    public NativeArray<ActionsTypes> Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int id = 0)
    { 
        NativeArray<ActionsTypes> tempRandomAgentActions = new NativeArray<ActionsTypes>(3, Allocator.Temp);
        tempRandomAgentActions[0] =  availableActions[Random.Range(0, 3)];
        tempRandomAgentActions[1] = availableActions[Random.Range(3, 5)];
        tempRandomAgentActions[2] = availableActions[Random.Range(5, 6)];

        return tempRandomAgentActions;
    }
}