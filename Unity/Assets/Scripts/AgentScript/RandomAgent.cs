using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auteur : Margot & Arthur

public class RandomAgent : IAgent
{
    public ActionsTypes[] Act(ref GameState gs, ActionsTypes[] availableActions, int id = 0)
    {
        Debug.Log(Random.Range(5, 6));
        return new ActionsTypes[]
        {
            availableActions[Random.Range(0, 3)],
            availableActions[Random.Range(3, 5)],
            availableActions[Random.Range(5, 6)]
        };
    }
}