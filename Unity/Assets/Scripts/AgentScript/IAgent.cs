using UnityEngine;
using Unity.Collections;

public interface IAgent
{
    NativeArray<ActionsTypes> Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int playerId);
}
