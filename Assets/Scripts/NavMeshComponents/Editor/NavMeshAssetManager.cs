using System.Collections.Generic;
using System.IO;

using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.AI;

namespace UnityEditor.AI
{
    public class NavMeshAssetManager : ScriptableSingleton<NavMeshAssetManager>
    {
        internal struct AsyncBakeOperation
        {
            public NavMeshSurface surface;
            public NavMeshData bakeData;
            public AsyncOperation bakeOperation;
        }

        private readonly List<AsyncBakeOperation> m_BakeOperations = new List<AsyncBakeOperation>();
        internal List<AsyncBakeOperation> GetBakeOperations()
        {
            return this.m_BakeOperations;
        }

        private struct SavedPrefabNavMeshData
        {
            public NavMeshSurface surface;
            public NavMeshData navMeshData;
        }

        private readonly List<SavedPrefabNavMeshData> m_PrefabNavMeshDataAssets = new List<SavedPrefabNavMeshData>();

        private static string GetAndEnsureTargetPath(NavMeshSurface surface)
        {
            // Create directory for the asset if it does not exist yet.
            var activeScenePath = surface.gameObject.scene.path;

            var targetPath = "Assets";
            if (!string.IsNullOrEmpty(activeScenePath))
            {
                targetPath = Path.Combine(Path.GetDirectoryName(activeScenePath), Path.GetFileNameWithoutExtension(activeScenePath));
            }
            else
            {
                var prefabStage = PrefabStageUtility.GetPrefabStage(surface.gameObject);
                var isPartOfPrefab = prefabStage != null && prefabStage.IsPartOfPrefabContents(surface.gameObject);
                if (isPartOfPrefab && !string.IsNullOrEmpty(prefabStage.prefabAssetPath))
                {
                    var prefabDirectoryName = Path.GetDirectoryName(prefabStage.prefabAssetPath);
                    if (!string.IsNullOrEmpty(prefabDirectoryName))
                    {
                        targetPath = prefabDirectoryName;
                    }
                }
            }
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            return targetPath;
        }

        private static void CreateNavMeshAsset(NavMeshSurface surface)
        {
            var targetPath = GetAndEnsureTargetPath(surface);

            var combinedAssetPath = Path.Combine(targetPath, "NavMesh-" + surface.name + ".asset");
            combinedAssetPath = AssetDatabase.GenerateUniqueAssetPath(combinedAssetPath);
            AssetDatabase.CreateAsset(surface.navMeshData, combinedAssetPath);
        }

        private NavMeshData GetNavMeshAssetToDelete(NavMeshSurface navSurface)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(navSurface) && !PrefabUtility.IsPartOfModelPrefab(navSurface))
            {
                // Don't allow deleting the asset belonging to the prefab parent
                var parentSurface = PrefabUtility.GetCorrespondingObjectFromSource(navSurface) as NavMeshSurface;
                if (parentSurface && navSurface.navMeshData == parentSurface.navMeshData)
                {
                    return null;
                }
            }

            // Do not delete the NavMeshData asset referenced from a prefab until the prefab is saved
            var prefabStage = PrefabStageUtility.GetPrefabStage(navSurface.gameObject);
            var isPartOfPrefab = prefabStage != null && prefabStage.IsPartOfPrefabContents(navSurface.gameObject);
            if (isPartOfPrefab && this.IsCurrentPrefabNavMeshDataStored(navSurface))
            {
                return null;
            }

            return navSurface.navMeshData;
        }

        private void ClearSurface(NavMeshSurface navSurface)
        {
            var hasNavMeshData = navSurface.navMeshData != null;
            this.StoreNavMeshDataIfInPrefab(navSurface);

            var assetToDelete = this.GetNavMeshAssetToDelete(navSurface);
            navSurface.RemoveData();

            if (hasNavMeshData)
            {
                SetNavMeshData(navSurface, null);
                EditorSceneManager.MarkSceneDirty(navSurface.gameObject.scene);
            }

            if (assetToDelete)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(assetToDelete));
            }
        }

        public void StartBakingSurfaces(UnityEngine.Object[] surfaces)
        {
            // Remove first to avoid double registration of the callback
            EditorApplication.update -= this.UpdateAsyncBuildOperations;
            EditorApplication.update += this.UpdateAsyncBuildOperations;

            foreach (NavMeshSurface surf in surfaces)
            {
                this.StoreNavMeshDataIfInPrefab(surf);

                var oper = new AsyncBakeOperation
                {
                    bakeData = InitializeBakeData(surf)
                };
                oper.bakeOperation = surf.UpdateNavMesh(oper.bakeData);
                oper.surface = surf;

                this.m_BakeOperations.Add(oper);
            }
        }

        private static NavMeshData InitializeBakeData(NavMeshSurface surface)
        {
            var emptySources = new List<NavMeshBuildSource>();
            var emptyBounds = new Bounds();
            return UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(surface.GetBuildSettings(), emptySources, emptyBounds
                , surface.transform.position, surface.transform.rotation);
        }

        private void UpdateAsyncBuildOperations()
        {
            foreach (var oper in this.m_BakeOperations)
            {
                if (oper.surface == null || oper.bakeOperation == null)
                {
                    continue;
                }

                if (oper.bakeOperation.isDone)
                {
                    var surface = oper.surface;
                    var delete = this.GetNavMeshAssetToDelete(surface);
                    if (delete != null)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(delete));
                    }

                    surface.RemoveData();
                    SetNavMeshData(surface, oper.bakeData);

                    if (surface.isActiveAndEnabled)
                    {
                        surface.AddData();
                    }

                    CreateNavMeshAsset(surface);
                    EditorSceneManager.MarkSceneDirty(surface.gameObject.scene);
                }
            }
            this.m_BakeOperations.RemoveAll(o => o.bakeOperation == null || o.bakeOperation.isDone);
            if (this.m_BakeOperations.Count == 0)
            {
                EditorApplication.update -= this.UpdateAsyncBuildOperations;
            }
        }

        public bool IsSurfaceBaking(NavMeshSurface surface)
        {
            if (surface == null)
            {
                return false;
            }

            foreach (var oper in this.m_BakeOperations)
            {
                if (oper.surface == null || oper.bakeOperation == null)
                {
                    continue;
                }

                if (oper.surface == surface)
                {
                    return true;
                }
            }

            return false;
        }

        public void ClearSurfaces(UnityEngine.Object[] surfaces)
        {
            foreach (NavMeshSurface s in surfaces)
            {
                this.ClearSurface(s);
            }
        }

        private static void SetNavMeshData(NavMeshSurface navSurface, NavMeshData navMeshData)
        {
            var so = new SerializedObject(navSurface);
            var navMeshDataProperty = so.FindProperty("m_NavMeshData");
            navMeshDataProperty.objectReferenceValue = navMeshData;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void StoreNavMeshDataIfInPrefab(NavMeshSurface surfaceToStore)
        {
            var prefabStage = PrefabStageUtility.GetPrefabStage(surfaceToStore.gameObject);
            var isPartOfPrefab = prefabStage != null && prefabStage.IsPartOfPrefabContents(surfaceToStore.gameObject);
            if (!isPartOfPrefab)
            {
                return;
            }

            // check if data has already been stored for this surface
            foreach (var storedAssetInfo in this.m_PrefabNavMeshDataAssets)
            {
                if (storedAssetInfo.surface == surfaceToStore)
                {
                    return;
                }
            }

            if (this.m_PrefabNavMeshDataAssets.Count == 0)
            {
                PrefabStage.prefabSaving -= this.DeleteStoredNavMeshDataAssetsForOwnedSurfaces;
                PrefabStage.prefabSaving += this.DeleteStoredNavMeshDataAssetsForOwnedSurfaces;

                PrefabStage.prefabStageClosing -= this.ForgetUnsavedNavMeshDataChanges;
                PrefabStage.prefabStageClosing += this.ForgetUnsavedNavMeshDataChanges;
            }

            var isDataOwner = true;
            if (PrefabUtility.IsPartOfPrefabInstance(surfaceToStore) && !PrefabUtility.IsPartOfModelPrefab(surfaceToStore))
            {
                var basePrefabSurface = PrefabUtility.GetCorrespondingObjectFromSource(surfaceToStore) as NavMeshSurface;
                isDataOwner = basePrefabSurface == null || surfaceToStore.navMeshData != basePrefabSurface.navMeshData;
            }
            this.m_PrefabNavMeshDataAssets.Add(new SavedPrefabNavMeshData { surface = surfaceToStore, navMeshData = isDataOwner ? surfaceToStore.navMeshData : null });
        }

        private bool IsCurrentPrefabNavMeshDataStored(NavMeshSurface surface)
        {
            if (surface == null)
            {
                return false;
            }

            foreach (var storedAssetInfo in this.m_PrefabNavMeshDataAssets)
            {
                if (storedAssetInfo.surface == surface)
                {
                    return storedAssetInfo.navMeshData == surface.navMeshData;
                }
            }

            return false;
        }

        private void DeleteStoredNavMeshDataAssetsForOwnedSurfaces(GameObject gameObjectInPrefab)
        {
            // Debug.LogFormat("DeleteStoredNavMeshDataAsset() when saving prefab {0}", gameObjectInPrefab.name);

            var surfaces = gameObjectInPrefab.GetComponentsInChildren<NavMeshSurface>(true);
            foreach (var surface in surfaces)
            {
                this.DeleteStoredPrefabNavMeshDataAsset(surface);
            }
        }

        private void DeleteStoredPrefabNavMeshDataAsset(NavMeshSurface surface)
        {
            for (var i = this.m_PrefabNavMeshDataAssets.Count - 1; i >= 0; i--)
            {
                var storedAssetInfo = this.m_PrefabNavMeshDataAssets[i];
                if (storedAssetInfo.surface == surface)
                {
                    var storedNavMeshData = storedAssetInfo.navMeshData;
                    if (storedNavMeshData != null && storedNavMeshData != surface.navMeshData)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(storedNavMeshData);
                        AssetDatabase.DeleteAsset(assetPath);
                    }

                    this.m_PrefabNavMeshDataAssets.RemoveAt(i);
                    break;
                }
            }

            if (this.m_PrefabNavMeshDataAssets.Count == 0)
            {
                PrefabStage.prefabSaving -= this.DeleteStoredNavMeshDataAssetsForOwnedSurfaces;
                PrefabStage.prefabStageClosing -= this.ForgetUnsavedNavMeshDataChanges;
            }
        }

        private void ForgetUnsavedNavMeshDataChanges(PrefabStage prefabStage)
        {
            // Debug.Log("On prefab closing - forget about this object's surfaces and stop caring about prefab saving");

            if (prefabStage == null)
            {
                return;
            }

            var allSurfacesInPrefab = prefabStage.prefabContentsRoot.GetComponentsInChildren<NavMeshSurface>(true);
            NavMeshSurface surfaceInPrefab = null;
            var index = 0;
            do
            {
                if (allSurfacesInPrefab.Length > 0)
                {
                    surfaceInPrefab = allSurfacesInPrefab[index];
                }

                for (var i = this.m_PrefabNavMeshDataAssets.Count - 1; i >= 0; i--)
                {
                    var storedPrefabInfo = this.m_PrefabNavMeshDataAssets[i];
                    if (storedPrefabInfo.surface == null)
                    {
                        // Debug.LogFormat("A surface from the prefab got deleted after it has baked a new NavMesh but it hasn't saved it. Now the unsaved asset gets deleted. ({0})", storedPrefabInfo.navMeshData);

                        // surface got deleted, thus delete its initial NavMeshData asset
                        if (storedPrefabInfo.navMeshData != null)
                        {
                            var assetPath = AssetDatabase.GetAssetPath(storedPrefabInfo.navMeshData);
                            AssetDatabase.DeleteAsset(assetPath);
                        }

                        this.m_PrefabNavMeshDataAssets.RemoveAt(i);
                    }
                    else if (surfaceInPrefab != null && storedPrefabInfo.surface == surfaceInPrefab)
                    {
                        //Debug.LogFormat("The surface {0} from the prefab was storing the original navmesh data and now will be forgotten", surfaceInPrefab);

                        var baseSurface = PrefabUtility.GetCorrespondingObjectFromSource(surfaceInPrefab) as NavMeshSurface;
                        if (baseSurface == null || surfaceInPrefab.navMeshData != baseSurface.navMeshData)
                        {
                            var assetPath = AssetDatabase.GetAssetPath(surfaceInPrefab.navMeshData);
                            AssetDatabase.DeleteAsset(assetPath);

                            //Debug.LogFormat("The surface {0} from the prefab has baked new NavMeshData but did not save this change so the asset has been now deleted. ({1})",
                            //    surfaceInPrefab, assetPath);
                        }

                        this.m_PrefabNavMeshDataAssets.RemoveAt(i);
                    }
                }
            } while (++index < allSurfacesInPrefab.Length);

            if (this.m_PrefabNavMeshDataAssets.Count == 0)
            {
                PrefabStage.prefabSaving -= this.DeleteStoredNavMeshDataAssetsForOwnedSurfaces;
                PrefabStage.prefabStageClosing -= this.ForgetUnsavedNavMeshDataChanges;
            }
        }
    }
}
