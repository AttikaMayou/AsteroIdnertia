using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auteur : Margot & Arthur

public class RandomAgent : IAgent
{
    public ActionsTypes Act(ref GameState gs, ActionsTypes[] availableActions)
    {
        return availableActions[Random.Range(0, availableActions.Length)];
    }
}
