using UnityEngine;
using Unity.Entities;
using Unity.Transforms;


[RequiresEntityConversion]
public class ProjectileAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<ProjectileComponent>(entity);
        dstManager.AddComponent<Translation>(entity);
        dstManager.AddComponent<Rotation>(entity);
    }
}
