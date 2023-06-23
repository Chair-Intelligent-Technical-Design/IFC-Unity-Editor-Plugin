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
    public static readonly ModelInitializer ActiveInitializer;

    static ModelInitializer()
    {
        ModelInitializer.ActiveInitializer = new ModelInitializer();
    }

    internal ModelInitializer()
    {
        EditorApplication.delayCall += this.DelayCallHandler;
    }

    private void DelayCallHandler()
    {
        if (Config.CurrentConfig.AutomaticTextureMapGeneration)
            this.GenerateTextureMaps();
    }

    /// <summary>
    /// Generates the texture maps for all gameobjects with geometry in the active scene
    /// </summary>
    public void GenerateTextureMaps()
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
        EditorApplication.delayCall -= this.GenerateTextureMaps;
        //EditorApplication.update -= this.UpdateHandler;
    }
}

#endif