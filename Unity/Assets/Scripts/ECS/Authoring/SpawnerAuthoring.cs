using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[RequiresEntityConversion]
public class SpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject projectilePrefab;
    public GameObject asteroidsPrefab;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawner = new Spawner
        {
            projectilePrefab = conversionSystem.GetPrimaryEntity(projectilePrefab),
            asteroidPrefab = conversionSystem.GetPrimaryEntity(asteroidsPrefab)
        };
        dstManager.AddComponent<Spawner>(entity);
        dstManager.SetComponentData(entity, spawner);
    }
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(projectilePrefab);
        referencedPrefabs.Add(asteroidsPrefab);
    }
}




