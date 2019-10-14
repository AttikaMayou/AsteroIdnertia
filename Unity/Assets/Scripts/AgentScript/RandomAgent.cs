using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAgent : IAgent
{
    public ActionsTypes Act(ref GameState gs, ActionsTypes[] availableActions)
    {
        return availableActions[Random.Range(0, availableActions.Length)];
    }
}
