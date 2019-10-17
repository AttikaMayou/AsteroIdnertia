using Unity.Collections;
using Unity.Jobs;
using Random = Unity.Mathematics.Random;
using Rules = GameStateRules;
using UnityEngine;

public class MLAgent : IAgent
{
    public ActionsTypes Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int playerId)
    {
        var longJob = new LongTermJob
        {
            availableActions = availableActions,
            gs = gs,
            rdmAgent = new RandomAgent { rdm = new Random((uint)Time.frameCount + (uint)playerId) },
        };

        ActionsTypes choosenAction = availableActions[0];
        return choosenAction;
    }

    struct LongTermJob : IJobParallelFor
    {
        public GameState gs;

        public GameParametersStruct gameParameters;

        [ReadOnly]
        public NativeArray<ActionsTypes> availableActions;

        public RandomAgent rdmAgent;

        //[WriteOnly]
        //Type de retour = les étapes intermédiaires == quelle structure ??
        //En faire une structure justement ?

        public int playerId;

        public void Execute(int index)
        {
            throw new System.NotImplementedException();
        }
    }
}
