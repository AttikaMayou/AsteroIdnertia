using UnityEngine;
using Unity.Entities;


[RequiresEntityConversion]
public class ProjectileAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<ProjectileComponent>(entity);
    }
}
