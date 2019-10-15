using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auteur : Margot & Arthur

public class RandomAgent : IAgent
{
    public ActionsTypes[] Act(ref GameState gs, ActionsTypes[] availableActions, int id = 0)
    {
        return new ActionsTypes[]
        {
            availableActions[Random.Range(0, 2)],
            availableActions[Random.Range(3, 4)],
            availableActions[Random.Range(5, availableActions.Length)]
        };
    }
}
