#if (UNITY_EDITOR)
using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HelperFunctions
{
    public static void DoForAllGameObjectsInProject(Action<GameObject> gameObjectAction)
    {
        for (int sceneIdx = 0; sceneIdx < EditorSceneManager.sceneCount; sceneIdx++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(sceneIdx);
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                gameObjectAction(rootObject);
            }
        }
    }
}

#endif