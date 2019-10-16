using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auteur : Margot & Arthur
//Modifications : Attika

public struct RandomAgent : IAgent
{
    public Unity.Mathematics.Random rdm;

    public ActionsTypes[] Act(ref GameState gs, ActionsTypes[] availableActions, int id = 0)
    {
        return new ActionsTypes[]
        {
            //TODO : use rdm instead of Random.Range
            availableActions[Random.Range(0, 3)],
            availableActions[Random.Range(3, 5)],
            availableActions[Random.Range(5, 6)]
        };
    }
}