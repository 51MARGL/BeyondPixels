using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.AI
{
    public enum CollectObjects
    {
        All = 0,
        Volume = 1,
        Children = 2,
    }

    [ExecuteAlways]
    [DefaultExecutionOrder(-102)]
    [AddComponentMenu("Navigation/NavMeshSurface", 30)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshSurface : MonoBehaviour
    {
        [SerializeField]
        private int m_AgentTypeID;
        public int agentTypeID { get => this.m_AgentTypeID; set => this.m_AgentTypeID = value; }

        [SerializeField]
        private CollectObjects m_CollectObjects = CollectObjects.All;
        public CollectObjects collectObjects { get => this.m_CollectObjects; set => this.m_CollectObjects = value; }

        [SerializeField]
        private Vector3 m_Size = new Vector3(10.0f, 10.0f, 10.0f);
        public Vector3 size { get => this.m_Size; set => this.m_Size = value; }

        [SerializeField]
        private Vector3 m_Center = new Vector3(0, 2.0f, 0);
        public Vector3 center { get => this.m_Center; set => this.m_Center = value; }

        [SerializeField]
        private LayerMask m_LayerMask = ~0;
        public LayerMask layerMask { get => this.m_LayerMask; set => this.m_LayerMask = value; }

        [SerializeField]
        private NavMeshCollectGeometry m_UseGeometry = NavMeshCollectGeometry.RenderMeshes;
        public NavMeshCollectGeometry useGeometry { get => this.m_UseGeometry; set => this.m_UseGeometry = value; }

        [SerializeField]
        private int m_DefaultArea;
        public int defaultArea { get => this.m_DefaultArea; set => this.m_DefaultArea = value; }

        [SerializeField]
        private bool m_IgnoreNavMeshAgent = true;
        public bool ignoreNavMeshAgent { get => this.m_IgnoreNavMeshAgent; set => this.m_IgnoreNavMeshAgent = value; }

        [SerializeField]
        private bool m_IgnoreNavMeshObstacle = true;
        public bool ignoreNavMeshObstacle { get => this.m_IgnoreNavMeshObstacle; set => this.m_IgnoreNavMeshObstacle = value; }

        [SerializeField]
        private bool m_OverrideTileSize;
        public bool overrideTileSize { get => this.m_OverrideTileSize; set => this.m_OverrideTileSize = value; }
        [SerializeField]
        private int m_TileSize = 256;
        public int tileSize { get => this.m_TileSize; set => this.m_TileSize = value; }
        [SerializeField]
        private bool m_OverrideVoxelSize;
        public bool overrideVoxelSize { get => this.m_OverrideVoxelSize; set => this.m_OverrideVoxelSize = value; }
        [SerializeField]
        private float m_VoxelSize;
        public float voxelSize { get => this.m_VoxelSize; set => this.m_VoxelSize = value; }

        // Currently not supported advanced options
        [SerializeField]
        private bool m_BuildHeightMesh;
        public bool buildHeightMesh { get => this.m_BuildHeightMesh; set => this.m_BuildHeightMesh = value; }

        // Reference to whole scene navmesh data asset.
        [UnityEngine.Serialization.FormerlySerializedAs("m_BakedNavMeshData")]
        [SerializeField]
        private NavMeshData m_NavMeshData;
        public NavMeshData navMeshData { get => this.m_NavMeshData; set => this.m_NavMeshData = value; }

        // Do not serialize - runtime only state.
        private NavMeshDataInstance m_NavMeshDataInstance;
        private Vector3 m_LastPosition = Vector3.zero;
        private Quaternion m_LastRotation = Quaternion.identity;
        private static readonly List<NavMeshSurface> s_NavMeshSurfaces = new List<NavMeshSurface>();

        public static List<NavMeshSurface> activeSurfaces => s_NavMeshSurfaces;

        private void OnEnable()
        {
            Register(this);
            this.AddData();
        }

        private void OnDisable()
        {
            this.RemoveData();
            Unregister(this);
        }

        public void AddData()
        {
#if UNITY_EDITOR
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(this);
            var isPrefab = isInPreviewScene || EditorUtility.IsPersistent(this);
            if (isPrefab)
            {
                //Debug.LogFormat("NavMeshData from {0}.{1} will not be added to the NavMesh world because the gameObject is a prefab.",
                //    gameObject.name, name);
                return;
            }
#endif
            if (this.m_NavMeshDataInstance.valid)
            {
                return;
            }

            if (this.m_NavMeshData != null)
            {
                this.m_NavMeshDataInstance = NavMesh.AddNavMeshData(this.m_NavMeshData, this.transform.position, this.transform.rotation);
                this.m_NavMeshDataInstance.owner = this;
            }

            this.m_LastPosition = this.transform.position;
            this.m_LastRotation = this.transform.rotation;
        }

        public void RemoveData()
        {
            this.m_NavMeshDataInstance.Remove();
            this.m_NavMeshDataInstance = new NavMeshDataInstance();
        }

        public NavMeshBuildSettings GetBuildSettings()
        {
            var buildSettings = NavMesh.GetSettingsByID(this.m_AgentTypeID);
            if (buildSettings.agentTypeID == -1)
            {
                Debug.LogWarning("No build settings for agent type ID " + this.agentTypeID, this);
                buildSettings.agentTypeID = this.m_AgentTypeID;
            }

            if (this.overrideTileSize)
            {
                buildSettings.overrideTileSize = true;
                buildSettings.tileSize = this.tileSize;
            }
            if (this.overrideVoxelSize)
            {
                buildSettings.overrideVoxelSize = true;
                buildSettings.voxelSize = this.voxelSize;
            }
            return buildSettings;
        }

        public void BuildNavMesh()
        {
            var sources = this.CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(this.m_Center, Abs(this.m_Size));
            if (this.m_CollectObjects == CollectObjects.All || this.m_CollectObjects == CollectObjects.Children)
            {
                sourcesBounds = this.CalculateWorldBounds(sources);
            }

            var data = NavMeshBuilder.BuildNavMeshData(this.GetBuildSettings(),
                    sources, sourcesBounds, this.transform.position, this.transform.rotation);

            if (data != null)
            {
                data.name = this.gameObject.name;
                this.RemoveData();
                this.m_NavMeshData = data;
                if (this.isActiveAndEnabled)
                {
                    this.AddData();
                }
            }
        }

        public AsyncOperation UpdateNavMesh(NavMeshData data)
        {
            var sources = this.CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(this.m_Center, Abs(this.m_Size));
            if (this.m_CollectObjects == CollectObjects.All || this.m_CollectObjects == CollectObjects.Children)
            {
                sourcesBounds = this.CalculateWorldBounds(sources);
            }

            return NavMeshBuilder.UpdateNavMeshDataAsync(data, this.GetBuildSettings(), sources, sourcesBounds);
        }

        private static void Register(NavMeshSurface surface)
        {
#if UNITY_EDITOR
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(surface);
            var isPrefab = isInPreviewScene || EditorUtility.IsPersistent(surface);
            if (isPrefab)
            {
                //Debug.LogFormat("NavMeshData from {0}.{1} will not be added to the NavMesh world because the gameObject is a prefab.",
                //    surface.gameObject.name, surface.name);
                return;
            }
#endif
            if (s_NavMeshSurfaces.Count == 0)
            {
                NavMesh.onPreUpdate += UpdateActive;
            }

            if (!s_NavMeshSurfaces.Contains(surface))
            {
                s_NavMeshSurfaces.Add(surface);
            }
        }

        private static void Unregister(NavMeshSurface surface)
        {
            s_NavMeshSurfaces.Remove(surface);

            if (s_NavMeshSurfaces.Count == 0)
            {
                NavMesh.onPreUpdate -= UpdateActive;
            }
        }

        private static void UpdateActive()
        {
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
            {
                s_NavMeshSurfaces[i].UpdateDataIfTransformChanged();
            }
        }

        private void AppendModifierVolumes(ref List<NavMeshBuildSource> sources)
        {
#if UNITY_EDITOR
            var myStage = StageUtility.GetStageHandle(this.gameObject);
            if (!myStage.IsValid())
            {
                return;
            }
#endif
            // Modifiers
            List<NavMeshModifierVolume> modifiers;
            if (this.m_CollectObjects == CollectObjects.Children)
            {
                modifiers = new List<NavMeshModifierVolume>(this.GetComponentsInChildren<NavMeshModifierVolume>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifierVolume.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((this.m_LayerMask & (1 << m.gameObject.layer)) == 0)
                {
                    continue;
                }

                if (!m.AffectsAgentType(this.m_AgentTypeID))
                {
                    continue;
                }
#if UNITY_EDITOR
                if (!myStage.Contains(m.gameObject))
                {
                    continue;
                }
#endif
                var mcenter = m.transform.TransformPoint(m.center);
                var scale = m.transform.lossyScale;
                var msize = new Vector3(m.size.x * Mathf.Abs(scale.x), m.size.y * Mathf.Abs(scale.y), m.size.z * Mathf.Abs(scale.z));

                var src = new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.ModifierBox,
                    transform = Matrix4x4.TRS(mcenter, m.transform.rotation, Vector3.one),
                    size = msize,
                    area = m.area
                };
                sources.Add(src);
            }
        }

        private List<NavMeshBuildSource> CollectSources()
        {
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();

            List<NavMeshModifier> modifiers;
            if (this.m_CollectObjects == CollectObjects.Children)
            {
                modifiers = new List<NavMeshModifier>(this.GetComponentsInChildren<NavMeshModifier>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifier.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((this.m_LayerMask & (1 << m.gameObject.layer)) == 0)
                {
                    continue;
                }

                if (!m.AffectsAgentType(this.m_AgentTypeID))
                {
                    continue;
                }

                var markup = new NavMeshBuildMarkup
                {
                    root = m.transform,
                    overrideArea = m.overrideArea,
                    area = m.area,
                    ignoreFromBuild = m.ignoreFromBuild
                };
                markups.Add(markup);
            }

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                if (this.m_CollectObjects == CollectObjects.All)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        null, this.m_LayerMask, this.m_UseGeometry, this.m_DefaultArea, markups, this.gameObject.scene, sources);
                }
                else if (this.m_CollectObjects == CollectObjects.Children)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        this.transform, this.m_LayerMask, this.m_UseGeometry, this.m_DefaultArea, markups, this.gameObject.scene, sources);
                }
                else if (this.m_CollectObjects == CollectObjects.Volume)
                {
                    var localToWorld = Matrix4x4.TRS(this.transform.position, this.transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, new Bounds(this.m_Center, this.m_Size));

                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        worldBounds, this.m_LayerMask, this.m_UseGeometry, this.m_DefaultArea, markups, this.gameObject.scene, sources);
                }
            }
            else
#endif
            {
                if (this.m_CollectObjects == CollectObjects.All)
                {
                    NavMeshBuilder.CollectSources(null, this.m_LayerMask, this.m_UseGeometry, this.m_DefaultArea, markups, sources);
                }
                else if (this.m_CollectObjects == CollectObjects.Children)
                {
                    NavMeshBuilder.CollectSources(this.transform, this.m_LayerMask, this.m_UseGeometry, this.m_DefaultArea, markups, sources);
                }
                else if (this.m_CollectObjects == CollectObjects.Volume)
                {
                    var localToWorld = Matrix4x4.TRS(this.transform.position, this.transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, new Bounds(this.m_Center, this.m_Size));
                    NavMeshBuilder.CollectSources(worldBounds, this.m_LayerMask, this.m_UseGeometry, this.m_DefaultArea, markups, sources);
                }
            }

            if (this.m_IgnoreNavMeshAgent)
            {
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshAgent>() != null));
            }

            if (this.m_IgnoreNavMeshObstacle)
            {
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshObstacle>() != null));
            }

            this.AppendModifierVolumes(ref sources);

            return sources;
        }

        private static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        private static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
        {
            var absAxisX = Abs(mat.MultiplyVector(Vector3.right));
            var absAxisY = Abs(mat.MultiplyVector(Vector3.up));
            var absAxisZ = Abs(mat.MultiplyVector(Vector3.forward));
            var worldPosition = mat.MultiplyPoint(bounds.center);
            var worldSize = absAxisX * bounds.size.x + absAxisY * bounds.size.y + absAxisZ * bounds.size.z;
            return new Bounds(worldPosition, worldSize);
        }

        private Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
        {
            // Use the unscaled matrix for the NavMeshSurface
            var worldToLocal = Matrix4x4.TRS(this.transform.position, this.transform.rotation, Vector3.one);
            worldToLocal = worldToLocal.inverse;

            var result = new Bounds();
            foreach (var src in sources)
            {
                switch (src.shape)
                {
                    case NavMeshBuildSourceShape.Mesh:
                        {
                            var m = src.sourceObject as Mesh;
                            result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, m.bounds));
                            break;
                        }
                    case NavMeshBuildSourceShape.Terrain:
                        {
                            // Terrain pivot is lower/left corner - shift bounds accordingly
                            var t = src.sourceObject as TerrainData;
                            result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(0.5f * t.size, t.size)));
                            break;
                        }
                    case NavMeshBuildSourceShape.Box:
                    case NavMeshBuildSourceShape.Sphere:
                    case NavMeshBuildSourceShape.Capsule:
                    case NavMeshBuildSourceShape.ModifierBox:
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(Vector3.zero, src.size)));
                        break;
                }
            }
            // Inflate the bounds a bit to avoid clipping co-planar sources
            result.Expand(0.1f);
            return result;
        }

        private bool HasTransformChanged()
        {
            if (this.m_LastPosition != this.transform.position)
            {
                return true;
            }

            if (this.m_LastRotation != this.transform.rotation)
            {
                return true;
            }

            return false;
        }

        private void UpdateDataIfTransformChanged()
        {
            if (this.HasTransformChanged())
            {
                this.RemoveData();
                this.AddData();
            }
        }

#if UNITY_EDITOR
        private bool UnshareNavMeshAsset()
        {
            // Nothing to unshare
            if (this.m_NavMeshData == null)
            {
                return false;
            }

            // Prefab parent owns the asset reference
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(this);
            var isPersistentObject = EditorUtility.IsPersistent(this);
            if (isInPreviewScene || isPersistentObject)
            {
                return false;
            }

            // An instance can share asset reference only with its prefab parent
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(this) as NavMeshSurface;
            if (prefab != null && prefab.navMeshData == this.navMeshData)
            {
                return false;
            }

            // Don't allow referencing an asset that's assigned to another surface
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
            {
                var surface = s_NavMeshSurfaces[i];
                if (surface != this && surface.m_NavMeshData == this.m_NavMeshData)
                {
                    return true;
                }
            }

            // Asset is not referenced by known surfaces
            return false;
        }

        private void OnValidate()
        {
            if (this.UnshareNavMeshAsset())
            {
                Debug.LogWarning("Duplicating NavMeshSurface does not duplicate the referenced navmesh data", this);
                this.m_NavMeshData = null;
            }

            var settings = NavMesh.GetSettingsByID(this.m_AgentTypeID);
            if (settings.agentTypeID != -1)
            {
                // When unchecking the override control, revert to automatic value.
                const float kMinVoxelSize = 0.01f;
                if (!this.m_OverrideVoxelSize)
                {
                    this.m_VoxelSize = settings.agentRadius / 3.0f;
                }

                if (this.m_VoxelSize < kMinVoxelSize)
                {
                    this.m_VoxelSize = kMinVoxelSize;
                }

                // When unchecking the override control, revert to default value.
                const int kMinTileSize = 16;
                const int kMaxTileSize = 1024;
                const int kDefaultTileSize = 256;

                if (!this.m_OverrideTileSize)
                {
                    this.m_TileSize = kDefaultTileSize;
                }
                // Make sure tilesize is in sane range.
                if (this.m_TileSize < kMinTileSize)
                {
                    this.m_TileSize = kMinTileSize;
                }

                if (this.m_TileSize > kMaxTileSize)
                {
                    this.m_TileSize = kMaxTileSize;
                }
            }
        }
#endif
    }
}
