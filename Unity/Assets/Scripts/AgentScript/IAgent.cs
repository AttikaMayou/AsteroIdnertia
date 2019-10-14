using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAgent
{
    ActionsTypes Act(ref GameState gs, ActionsTypes[] availableActions);
}
