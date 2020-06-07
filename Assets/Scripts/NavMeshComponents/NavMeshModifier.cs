using System.Collections.Generic;

namespace UnityEngine.AI
{
    [ExecuteInEditMode]
    [AddComponentMenu("Navigation/NavMeshModifier", 32)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshModifier : MonoBehaviour
    {
        [SerializeField]
        private bool m_OverrideArea;
        public bool overrideArea { get => this.m_OverrideArea; set => this.m_OverrideArea = value; }

        [SerializeField]
        private int m_Area;
        public int area { get => this.m_Area; set => this.m_Area = value; }

        [SerializeField]
        private bool m_IgnoreFromBuild;
        public bool ignoreFromBuild { get => this.m_IgnoreFromBuild; set => this.m_IgnoreFromBuild = value; }

        // List of agent types the modifier is applied for.
        // Special values: empty == None, m_AffectedAgents[0] =-1 == All.
        [SerializeField]
        private readonly List<int> m_AffectedAgents = new List<int>(new int[] { -1 });    // Default value is All

        private static readonly List<NavMeshModifier> s_NavMeshModifiers = new List<NavMeshModifier>();

        public static List<NavMeshModifier> activeModifiers => s_NavMeshModifiers;

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
