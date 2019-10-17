using UnityEngine;
using Unity.Entities;

public struct Spawner : IComponentData
{
    public Entity projectilePrefab;
    public Entity asteroidPrefab;
    
}