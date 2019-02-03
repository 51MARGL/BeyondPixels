using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public struct CollisionInfo : IComponentData
    {
        public Entity Sender;
        public Entity Other;
        public EventType EventType;
    }

    public enum EventType
    {
        TriggerEnter = 0,
        TriggerExit = 1,
        TriggerStay = 2,
        CollisionEnter = 3,
        CollisionExit = 4,
        CollisionStay = 5
    }

    public struct AggroRangeCollisionComponent : IComponentData { }

    public struct SpellCollisionComponent : IComponentData
    {
        public Entity SpellEntity;
        public Vector2 ImpactPoint;
        public float ImpactTime;
    }
}