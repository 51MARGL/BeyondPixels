using UnityEngine;
using UnityEngine.AI;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.NavMesh
{
    public class NavMeshComponent : MonoBehaviour
    {
        public NavMeshSurface NavMeshSurface;
        public GameObject NavMeshGround;
        public GameObject NavMeshWall;
    }
}
