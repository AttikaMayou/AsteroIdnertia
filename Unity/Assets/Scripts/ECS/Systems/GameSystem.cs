using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using Unity.Transforms;


public class GameSystem : ComponentSystem
{



    public void UpdatePlayersViews(ref GameState gs)
    {
        var players = Entities.WithAll<PlayerComponent>().ToEntityQuery().ToEntityArray(Allocator.TempJob);

        for (int i = 0; i < gs.players.Length; i++)
        {
            EntityManager.SetComponentData(players[i], new Translation
            {
                Value = new float3(gs.players[i].position.x, 0, gs.players[i].position.y)
            });

            EntityManager.SetComponentData(players[i], new Rotation
            {
                Value = quaternion.LookRotation(new float3(gs.players[i].lookDirection.x, 0, gs.players[i].lookDirection.y), new float3(0, 1, 0))
            });

        }

    }

    protected override void OnCreate()
    {
        Entities.WithAll<PlayerComponent>().ForEach(x =>
        {
            EntityManager.AddComponentData(x, new Translation
            {
                Value = new float3(0, 0, 0)
            });

            EntityManager.AddComponentData(x, new Rotation
            {
                Value = quaternion.identity
            });
        });
    }

    public void StartGame()
    {

    }

    protected override void OnUpdate()
    {

    }
}
