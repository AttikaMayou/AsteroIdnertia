using UnityEngine;
using Unity.Collections;

//Auteur : Margot & Arthur
//Modifications : Attika

public struct RandomAgent : IAgent
{
    public Unity.Mathematics.Random rdm;

    public NativeList<ActionsTypes> Act(ref GameState gs, NativeList<ActionsTypes> availableActions, int id = 0)
    {
        return new NativeList<ActionsTypes>
        {
            //TODO : use rdm instead of Random.Range
            availableActions[Random.Range(0, 3)],
            availableActions[Random.Range(3, 5)],
            availableActions[Random.Range(5, 6)]
        };
    }
}