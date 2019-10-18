using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;

[RequiresEntityConversion]
public class AsteroidAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<AsteroidComponent>(entity);
        dstManager.AddComponent<Translation>(entity);
    }
}
