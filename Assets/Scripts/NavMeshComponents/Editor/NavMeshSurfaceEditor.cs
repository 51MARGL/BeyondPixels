#define NAVMESHCOMPONENTS_SHOW_NAVMESHDATA_REF

using System.Linq;

using UnityEditor.IMGUI.Controls;

using UnityEditorInternal;

using UnityEngine;
using UnityEngine.AI;

namespace UnityEditor.AI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshSurface))]
    internal class NavMeshSurfaceEditor : Editor
    {
        private SerializedProperty m_AgentTypeID;
        private SerializedProperty m_BuildHeightMesh;
        private SerializedProperty m_Center;
        private SerializedProperty m_CollectObjects;
        private SerializedProperty m_DefaultArea;
        private SerializedProperty m_LayerMask;
        private SerializedProperty m_OverrideTileSize;
        private SerializedProperty m_OverrideVoxelSize;
        private SerializedProperty m_Size;
        private SerializedProperty m_TileSize;
        private SerializedProperty m_UseGeometry;
        private SerializedProperty m_VoxelSize;

#if NAVMESHCOMPONENTS_SHOW_NAVMESHDATA_REF
        private SerializedProperty m_NavMeshData;
#endif
        private class Styles
        {
            public readonly GUIContent m_LayerMask = new GUIContent("Include Layers");

            public readonly GUIContent m_ShowInputGeom = new GUIContent("Show Input Geom");
            public readonly GUIContent m_ShowVoxels = new GUIContent("Show Voxels");
            public readonly GUIContent m_ShowRegions = new GUIContent("Show Regions");
            public readonly GUIContent m_ShowRawContours = new GUIContent("Show Raw Contours");
            public readonly GUIContent m_ShowContours = new GUIContent("Show Contours");
            public readonly GUIContent m_ShowPolyMesh = new GUIContent("Show Poly Mesh");
            public readonly GUIContent m_ShowPolyMeshDetail = new GUIContent("Show Poly Mesh Detail");
        }

        private static Styles s_Styles;
        private static readonly bool s_ShowDebugOptions;
        private static Color s_HandleColor = new Color(127f, 214f, 244f, 100f) / 255;
        private static Color s_HandleColorSelected = new Color(127f, 214f, 244f, 210f) / 255;
        private static Color s_HandleColorDisabled = new Color(127f * 0.75f, 214f * 0.75f, 244f * 0.75f, 100f) / 255;
        private readonly BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        private bool editingCollider => EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner(this);

        private void OnEnable()
        {
            this.m_AgentTypeID = this.serializedObject.FindProperty("m_AgentTypeID");
            this.m_BuildHeightMesh = this.serializedObject.FindProperty("m_BuildHeightMesh");
            this.m_Center = this.serializedObject.FindProperty("m_Center");
            this.m_CollectObjects = this.serializedObject.FindProperty("m_CollectObjects");
            this.m_DefaultArea = this.serializedObject.FindProperty("m_DefaultArea");
            this.m_LayerMask = this.serializedObject.FindProperty("m_LayerMask");
            this.m_OverrideTileSize = this.serializedObject.FindProperty("m_OverrideTileSize");
            this.m_OverrideVoxelSize = this.serializedObject.FindProperty("m_OverrideVoxelSize");
            this.m_Size = this.serializedObject.FindProperty("m_Size");
            this.m_TileSize = this.serializedObject.FindProperty("m_TileSize");
            this.m_UseGeometry = this.serializedObject.FindProperty("m_UseGeometry");
            this.m_VoxelSize = this.serializedObject.FindProperty("m_VoxelSize");

#if NAVMESHCOMPONENTS_SHOW_NAVMESHDATA_REF
            this.m_NavMeshData = this.serializedObject.FindProperty("m_NavMeshData");
#endif
            NavMeshVisualizationSettings.showNavigation++;
        }

        private void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }

        private Bounds GetBounds()
        {
            var navSurface = (NavMeshSurface)this.target;
            return new Bounds(navSurface.transform.position, navSurface.size);
        }

        public override void OnInspectorGUI()
        {
            if (s_Styles == null)
            {
                s_Styles = new Styles();
            }

            this.serializedObject.Update();

            var bs = NavMesh.GetSettingsByID(this.m_AgentTypeID.intValue);

            if (bs.agentTypeID != -1)
            {
                // Draw image
                const float diagramHeight = 80.0f;
                var agentDiagramRect = EditorGUILayout.GetControlRect(false, diagramHeight);
                NavMeshEditorHelpers.DrawAgentDiagram(agentDiagramRect, bs.agentRadius, bs.agentHeight, bs.agentClimb, bs.agentSlope);
            }
            NavMeshComponentsGUIUtility.AgentTypePopup("Agent Type", this.m_AgentTypeID);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.m_CollectObjects);
            if ((CollectObjects)this.m_CollectObjects.enumValueIndex == CollectObjects.Volume)
            {
                EditorGUI.indentLevel++;

                EditMode.DoEditModeInspectorModeButton(EditMode.SceneViewEditMode.Collider, "Edit Volume",
                    EditorGUIUtility.IconContent("EditCollider"), this.GetBounds, this);
                EditorGUILayout.PropertyField(this.m_Size);
                EditorGUILayout.PropertyField(this.m_Center);

                EditorGUI.indentLevel--;
            }
            else
            {
                if (this.editingCollider)
                {
                    EditMode.QuitEditMode();
                }
            }

            EditorGUILayout.PropertyField(this.m_LayerMask, s_Styles.m_LayerMask);
            EditorGUILayout.PropertyField(this.m_UseGeometry);

            EditorGUILayout.Space();

            this.m_OverrideVoxelSize.isExpanded = EditorGUILayout.Foldout(this.m_OverrideVoxelSize.isExpanded, "Advanced");
            if (this.m_OverrideVoxelSize.isExpanded)
            {
                EditorGUI.indentLevel++;

                NavMeshComponentsGUIUtility.AreaPopup("Default Area", this.m_DefaultArea);

                // Override voxel size.
                EditorGUILayout.PropertyField(this.m_OverrideVoxelSize);

                using (new EditorGUI.DisabledScope(!this.m_OverrideVoxelSize.boolValue || this.m_OverrideVoxelSize.hasMultipleDifferentValues))
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(this.m_VoxelSize);

                    if (!this.m_OverrideVoxelSize.hasMultipleDifferentValues)
                    {
                        if (!this.m_AgentTypeID.hasMultipleDifferentValues)
                        {
                            var voxelsPerRadius = this.m_VoxelSize.floatValue > 0.0f ? (bs.agentRadius / this.m_VoxelSize.floatValue) : 0.0f;
                            EditorGUILayout.LabelField(" ", voxelsPerRadius.ToString("0.00") + " voxels per agent radius", EditorStyles.miniLabel);
                        }
                        if (this.m_OverrideVoxelSize.boolValue)
                        {
                            EditorGUILayout.HelpBox("Voxel size controls how accurately the navigation mesh is generated from the level geometry. A good voxel size is 2-4 voxels per agent radius. Making voxel size smaller will increase build time.", MessageType.None);
                        }
                    }
                    EditorGUI.indentLevel--;
                }

                // Override tile size
                EditorGUILayout.PropertyField(this.m_OverrideTileSize);

                using (new EditorGUI.DisabledScope(!this.m_OverrideTileSize.boolValue || this.m_OverrideTileSize.hasMultipleDifferentValues))
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(this.m_TileSize);

                    if (!this.m_TileSize.hasMultipleDifferentValues && !this.m_VoxelSize.hasMultipleDifferentValues)
                    {
                        var tileWorldSize = this.m_TileSize.intValue * this.m_VoxelSize.floatValue;
                        EditorGUILayout.LabelField(" ", tileWorldSize.ToString("0.00") + " world units", EditorStyles.miniLabel);
                    }

                    if (!this.m_OverrideTileSize.hasMultipleDifferentValues)
                    {
                        if (this.m_OverrideTileSize.boolValue)
                        {
                            EditorGUILayout.HelpBox("Tile size controls the how local the changes to the world are (rebuild or carve). Small tile size allows more local changes, while potentially generating more data overall.", MessageType.None);
                        }
                    }
                    EditorGUI.indentLevel--;
                }


                // Height mesh
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(this.m_BuildHeightMesh);
                }

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            this.serializedObject.ApplyModifiedProperties();

            var hadError = false;
            var multipleTargets = this.targets.Length > 1;
            foreach (NavMeshSurface navSurface in this.targets)
            {
                var settings = navSurface.GetBuildSettings();
                // Calculating bounds is potentially expensive when unbounded - so here we just use the center/size.
                // It means the validation is not checking vertical voxel limit correctly when the surface is set to something else than "in volume".
                var bounds = new Bounds(Vector3.zero, Vector3.zero);
                if (navSurface.collectObjects == CollectObjects.Volume)
                {
                    bounds = new Bounds(navSurface.center, navSurface.size);
                }

                var errors = settings.ValidationReport(bounds);
                if (errors.Length > 0)
                {
                    if (multipleTargets)
                    {
                        EditorGUILayout.LabelField(navSurface.name);
                    }

                    foreach (var err in errors)
                    {
                        EditorGUILayout.HelpBox(err, MessageType.Warning);
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    if (GUILayout.Button("Open Agent Settings...", EditorStyles.miniButton))
                    {
                        NavMeshEditorHelpers.OpenAgentSettings(navSurface.agentTypeID);
                    }

                    GUILayout.EndHorizontal();
                    hadError = true;
                }
            }

            if (hadError)
            {
                EditorGUILayout.Space();
            }

#if NAVMESHCOMPONENTS_SHOW_NAVMESHDATA_REF
            var nmdRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(nmdRect, GUIContent.none, this.m_NavMeshData);
            var rectLabel = EditorGUI.PrefixLabel(nmdRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(this.m_NavMeshData.displayName));
            EditorGUI.EndProperty();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.BeginProperty(nmdRect, GUIContent.none, this.m_NavMeshData);
                EditorGUI.ObjectField(rectLabel, this.m_NavMeshData, GUIContent.none);
                EditorGUI.EndProperty();
            }
#endif
            using (new EditorGUI.DisabledScope(Application.isPlaying || this.m_AgentTypeID.intValue == -1))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Clear"))
                {
                    NavMeshAssetManager.instance.ClearSurfaces(this.targets);
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Bake"))
                {
                    NavMeshAssetManager.instance.StartBakingSurfaces(this.targets);
                }

                GUILayout.EndHorizontal();
            }

            // Show progress for the selected targets
            var bakeOperations = NavMeshAssetManager.instance.GetBakeOperations();
            for (var i = bakeOperations.Count - 1; i >= 0; --i)
            {
                if (!this.targets.Contains(bakeOperations[i].surface))
                {
                    continue;
                }

                var oper = bakeOperations[i].bakeOperation;
                if (oper == null)
                {
                    continue;
                }

                var p = oper.progress;
                if (oper.isDone)
                {
                    SceneView.RepaintAll();
                    continue;
                }

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                {
                    var bakeData = bakeOperations[i].bakeData;
                    UnityEngine.AI.NavMeshBuilder.Cancel(bakeData);
                    bakeOperations.RemoveAt(i);
                }

                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), p, "Baking: " + (int)(100 * p) + "%");
                if (p <= 1)
                {
                    this.Repaint();
                }

                GUILayout.EndHorizontal();
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.Pickable)]
        private static void RenderBoxGizmoSelected(NavMeshSurface navSurface, GizmoType gizmoType)
        {
            RenderBoxGizmo(navSurface, gizmoType, true);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        private static void RenderBoxGizmoNotSelected(NavMeshSurface navSurface, GizmoType gizmoType)
        {
            if (NavMeshVisualizationSettings.showNavigation > 0)
            {
                RenderBoxGizmo(navSurface, gizmoType, false);
            }
            else
            {
                Gizmos.DrawIcon(navSurface.transform.position, "NavMeshSurface Icon", true);
            }
        }

        private static void RenderBoxGizmo(NavMeshSurface navSurface, GizmoType gizmoType, bool selected)
        {
            var color = selected ? s_HandleColorSelected : s_HandleColor;
            if (!navSurface.enabled)
            {
                color = s_HandleColorDisabled;
            }

            var oldColor = Gizmos.color;
            var oldMatrix = Gizmos.matrix;

            // Use the unscaled matrix for the NavMeshSurface
            var localToWorld = Matrix4x4.TRS(navSurface.transform.position, navSurface.transform.rotation, Vector3.one);
            Gizmos.matrix = localToWorld;

            if (navSurface.collectObjects == CollectObjects.Volume)
            {
                Gizmos.color = color;
                Gizmos.DrawWireCube(navSurface.center, navSurface.size);

                if (selected && navSurface.enabled)
                {
                    var colorTrans = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, color.a * 0.15f);
                    Gizmos.color = colorTrans;
                    Gizmos.DrawCube(navSurface.center, navSurface.size);
                }
            }
            else
            {
                if (navSurface.navMeshData != null)
                {
                    var bounds = navSurface.navMeshData.sourceBounds;
                    Gizmos.color = Color.grey;
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }
            }

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navSurface.transform.position, "NavMeshSurface Icon", true);
        }

        private void OnSceneGUI()
        {
            if (!this.editingCollider)
            {
                return;
            }

            var navSurface = (NavMeshSurface)this.target;
            var color = navSurface.enabled ? s_HandleColor : s_HandleColorDisabled;
            var localToWorld = Matrix4x4.TRS(navSurface.transform.position, navSurface.transform.rotation, Vector3.one);
            using (new Handles.DrawingScope(color, localToWorld))
            {
                this.m_BoundsHandle.center = navSurface.center;
                this.m_BoundsHandle.size = navSurface.size;

                EditorGUI.BeginChangeCheck();
                this.m_BoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navSurface, "Modified NavMesh Surface");
                    var center = this.m_BoundsHandle.center;
                    var size = this.m_BoundsHandle.size;
                    navSurface.center = center;
                    navSurface.size = size;
                    EditorUtility.SetDirty(this.target);
                }
            }
        }

        [MenuItem("GameObject/AI/NavMesh Surface", false, 2000)]
        public static void CreateNavMeshSurface(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            var go = NavMeshComponentsGUIUtility.CreateAndSelectGameObject("NavMesh Surface", parent);
            go.AddComponent<NavMeshSurface>();
            var view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                view.MoveToView(go.transform);
            }
        }
    }
}
