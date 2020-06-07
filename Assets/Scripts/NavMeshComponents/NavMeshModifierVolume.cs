using System.Collections.Generic;

namespace UnityEngine.AI
{
    [ExecuteInEditMode]
    [AddComponentMenu("Navigation/NavMeshModifierVolume", 31)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshModifierVolume : MonoBehaviour
    {
        [SerializeField]
        private Vector3 m_Size = new Vector3(4.0f, 3.0f, 4.0f);
        public Vector3 size { get => this.m_Size; set => this.m_Size = value; }

        [SerializeField]
        private Vector3 m_Center = new Vector3(0, 1.0f, 0);
        public Vector3 center { get => this.m_Center; set => this.m_Center = value; }

        [SerializeField]
        private int m_Area;
        public int area { get => this.m_Area; set => this.m_Area = value; }

        // List of agent types the modifier is applied for.
        // Special values: empty == None, m_AffectedAgents[0] =-1 == All.
        [SerializeField]
        private readonly List<int> m_AffectedAgents = new List<int>(new int[] { -1 });    // Default value is All

        private static readonly List<NavMeshModifierVolume> s_NavMeshModifiers = new List<NavMeshModifierVolume>();

        public static List<NavMeshModifierVolume> activeModifiers => s_NavMeshModifiers;

        private void OnEnable()
        {
            if (!s_NavMeshModifiers.Contains(this))
            {
                s_NavMeshModifiers.Add(this);
            }
        }

        private void OnDisable()
        {
            s_NavMeshModifiers.Remove(this);
        }

        public bool AffectsAgentType(int agentTypeID)
        {
            if (this.m_AffectedAgents.Count == 0)
            {
                return false;
            }

            if (this.m_AffectedAgents[0] == -1)
            {
                return true;
            }

            return this.m_AffectedAgents.IndexOf(agentTypeID) != -1;
        }
    }
}
