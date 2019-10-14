using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAgent : MonoBehaviour
{
    public int Act(ref GameState gs, ActionsTypes[] availableActions)
    {
        return availableActions[Random.Range(0, 4)];
    }
}
