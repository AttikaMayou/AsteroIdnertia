using UnityEngine;
using Unity.Collections;

public interface IAgent
{
    NativeList<ActionsTypes> Act(ref GameState gs, NativeList<ActionsTypes> availableActions, int playerId);
}
