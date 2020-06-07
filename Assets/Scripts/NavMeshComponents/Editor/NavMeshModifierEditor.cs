using UnityEngine.AI;

namespace UnityEditor.AI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshModifier))]
    internal class NavMeshModifierEditor : Editor
    {
        private SerializedProperty m_AffectedAgents;
        private SerializedProperty m_Area;
        private SerializedProperty m_IgnoreFromBuild;
        private SerializedProperty m_OverrideArea;

        private void OnEnable()
        {
            this.m_AffectedAgents = this.serializedObject.FindProperty("m_AffectedAgents");
            this.m_Area = this.serializedObject.FindProperty("m_Area");
            this.m_IgnoreFromBuild = this.serializedObject.FindProperty("m_IgnoreFromBuild");
            this.m_OverrideArea = this.serializedObject.FindProperty("m_OverrideArea");

            NavMeshVisualizationSettings.showNavigation++;
        }

        private void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.m_IgnoreFromBuild);

            EditorGUILayout.PropertyField(this.m_OverrideArea);
            if (this.m_OverrideArea.boolValue)
            {
                EditorGUI.indentLevel++;
                NavMeshComponentsGUIUtility.AreaPopup("Area Type", this.m_Area);
                EditorGUI.indentLevel--;
            }

            NavMeshComponentsGUIUtility.AgentMaskPopup("Affected Agents", this.m_AffectedAgents);
            EditorGUILayout.Space();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
