using UnityEngine;
using Unity.Collections;

//Auteur : Margot & Arthur
//Modifications : Attika

public struct RandomAgent : IAgent
{
    public Unity.Mathematics.Random rdm;

    public ActionsTypes Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int id = 0)
    {
        return availableActions[rdm.NextInt(0, availableActions.Length)];
    }
}