using UnityEditor.IMGUI.Controls;

using UnityEditorInternal;

using UnityEngine;
using UnityEngine.AI;

namespace UnityEditor.AI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshModifierVolume))]
    internal class NavMeshModifierVolumeEditor : Editor
    {
        private SerializedProperty m_AffectedAgents;
        private SerializedProperty m_Area;
        private SerializedProperty m_Center;
        private SerializedProperty m_Size;
        private static Color s_HandleColor = new Color(187f, 138f, 240f, 210f) / 255;
        private static Color s_HandleColorDisabled = new Color(187f * 0.75f, 138f * 0.75f, 240f * 0.75f, 100f) / 255;
        private readonly BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        private bool editingCollider => EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner(this);

        private void OnEnable()
        {
            this.m_AffectedAgents = this.serializedObject.FindProperty("m_AffectedAgents");
            this.m_Area = this.serializedObject.FindProperty("m_Area");
            this.m_Center = this.serializedObject.FindProperty("m_Center");
            this.m_Size = this.serializedObject.FindProperty("m_Size");

            NavMeshVisualizationSettings.showNavigation++;
        }

        private void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }

        private Bounds GetBounds()
        {
            var navModifier = (NavMeshModifierVolume)this.target;
            return new Bounds(navModifier.transform.position, navModifier.size);
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditMode.DoEditModeInspectorModeButton(EditMode.SceneViewEditMode.Collider, "Edit Volume",
                EditorGUIUtility.IconContent("EditCollider"), this.GetBounds, this);

            EditorGUILayout.PropertyField(this.m_Size);
            EditorGUILayout.PropertyField(this.m_Center);

            NavMeshComponentsGUIUtility.AreaPopup("Area Type", this.m_Area);
            NavMeshComponentsGUIUtility.AgentMaskPopup("Affected Agents", this.m_AffectedAgents);
            EditorGUILayout.Space();

            this.serializedObject.ApplyModifiedProperties();
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        private static void RenderBoxGizmo(NavMeshModifierVolume navModifier, GizmoType gizmoType)
        {
            var color = navModifier.enabled ? s_HandleColor : s_HandleColorDisabled;
            var colorTrans = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, color.a * 0.15f);

            var oldColor = Gizmos.color;
            var oldMatrix = Gizmos.matrix;

            Gizmos.matrix = navModifier.transform.localToWorldMatrix;

            Gizmos.color = colorTrans;
            Gizmos.DrawCube(navModifier.center, navModifier.size);

            Gizmos.color = color;
            Gizmos.DrawWireCube(navModifier.center, navModifier.size);

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navModifier.transform.position, "NavMeshModifierVolume Icon", true);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        private static void RenderBoxGizmoNotSelected(NavMeshModifierVolume navModifier, GizmoType gizmoType)
        {
            if (NavMeshVisualizationSettings.showNavigation > 0)
            {
                var color = navModifier.enabled ? s_HandleColor : s_HandleColorDisabled;
                var oldColor = Gizmos.color;
                var oldMatrix = Gizmos.matrix;

                Gizmos.matrix = navModifier.transform.localToWorldMatrix;

                Gizmos.color = color;
                Gizmos.DrawWireCube(navModifier.center, navModifier.size);

                Gizmos.matrix = oldMatrix;
                Gizmos.color = oldColor;
            }

            Gizmos.DrawIcon(navModifier.transform.position, "NavMeshModifierVolume Icon", true);
        }

        private void OnSceneGUI()
        {
            if (!this.editingCollider)
            {
                return;
            }

            var vol = (NavMeshModifierVolume)this.target;
            var color = vol.enabled ? s_HandleColor : s_HandleColorDisabled;
            using (new Handles.DrawingScope(color, vol.transform.localToWorldMatrix))
            {
                this.m_BoundsHandle.center = vol.center;
                this.m_BoundsHandle.size = vol.size;

                EditorGUI.BeginChangeCheck();
                this.m_BoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(vol, "Modified NavMesh Modifier Volume");
                    var center = this.m_BoundsHandle.center;
                    var size = this.m_BoundsHandle.size;
                    vol.center = center;
                    vol.size = size;
                    EditorUtility.SetDirty(this.target);
                }
            }
        }

        [MenuItem("GameObject/AI/NavMesh Modifier Volume", false, 2001)]
        public static void CreateNavMeshModifierVolume(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            var go = NavMeshComponentsGUIUtility.CreateAndSelectGameObject("NavMesh Modifier Volume", parent);
            go.AddComponent<NavMeshModifierVolume>();
            var view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                view.MoveToView(go.transform);
            }
        }
    }
}
