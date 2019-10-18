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

        players.Dispose();
    }

    public void UpdateAsteroidsViews(ref GameState gs)
    {
        var asteroids = Entities.WithAll<AsteroidComponent>().ToEntityQuery().ToEntityArray(Allocator.TempJob);
        var s = new NativeList<Entity>(asteroids.Length, Allocator.Temp);
        s.AddRange(asteroids);

        var asteroidsToSpawn = gs.asteroids.Length - s.Length;
        Debug.Log(asteroidsToSpawn);
        var spawner = Entities.WithAll<Spawner>().ToEntityQuery().ToComponentDataArray<Spawner>(Allocator.TempJob);

        for(int i = 0; i < asteroidsToSpawn; i++)
        {
            var entity = EntityManager.Instantiate(spawner[0].asteroidPrefab);
            // s = Entities.WithAll<AsteroidComponent>().ToEntityQuery().ToEntityArray(Allocator.TempJob);
            s.Add(entity);
        }

        for(int i = 0; i < -asteroidsToSpawn; i++)
        {
            EntityManager.DestroyEntity(s[s.Length - 1]);
            s.RemoveAtSwapBack(s.Length - 1);
            //s = Entities.WithAll<AsteroidComponent>().ToEntityQuery().ToEntityArray(Allocator.TempJob);
        }

        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == null)
                continue;
            float3 newPos = new float3(gs.asteroids[i].position.x, 0, gs.asteroids[i].position.y);
            EntityManager.SetComponentData(s[i], new Translation
            {
                Value = newPos
            });
           
        }

        asteroids.Dispose();
        spawner.Dispose();
        s.Dispose();
        
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
