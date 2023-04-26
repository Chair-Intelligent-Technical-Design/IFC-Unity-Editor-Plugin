#if (UNITY_EDITOR)
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Xbim.Ifc;

/// <summary>
/// Handles the initialization ofter loading a project with an IFC file
/// </summary>
[InitializeOnLoad]
internal class ModelInitializer : IDisposable
{
    /// <summary>
    /// static instance of an initializer
    /// </summary>
    private static readonly ModelInitializer activeInitializer;

    static ModelInitializer()
    {
        ModelInitializer.activeInitializer = new ModelInitializer();
    }

    internal ModelInitializer()
    {
        EditorApplication.delayCall += this.DelayCallHandler;
    }

    private void UpdateHandler()
    {
        Debug.Log("UpdateHandler called.");
    }

    private void DelayCallHandler()
    {
        //link IFC entities
        IfcEntityLinker entityLinker = new IfcEntityLinker();
        for (int sceneIdx = 0; sceneIdx < EditorSceneManager.sceneCount; sceneIdx++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(sceneIdx);
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject root in rootObjects)
            {
                IfcFileAssociation fileAssoc = root.GetComponent<IfcFileAssociation>();
                if (fileAssoc != null
                    && fileAssoc.IfcModel == null)
                {
                    entityLinker.LinkEntitiesByProductData(root, IfcStore.Open(Application.streamingAssetsPath + "/" + fileAssoc.IfcFile));
                    //generate UV maps
                    UvMapGenerator uvGenerator = new UvMapGenerator();
                    uvGenerator.CalculateUvMaps(root);
                }
            }
        }
    }

    public void Dispose()
    {
        EditorApplication.delayCall -= this.DelayCallHandler;
        //EditorApplication.update -= this.UpdateHandler;
    }
}

#endif