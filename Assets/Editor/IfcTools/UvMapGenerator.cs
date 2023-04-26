#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UvMapGenerator
{
    /// <summary>
    /// generates the UV maps for all entities under the given game object
    /// </summary>
    /// <param name="gameObject"></param>
    public void CalculateUvMaps(GameObject gameObject)
    {
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        if (filter != null)
        {
            Mesh elementMesh = filter.sharedMesh;
            Unwrapping.GenerateSecondaryUVSet(elementMesh);
            elementMesh.uv = elementMesh.uv2;
        }

        // Generate the texture maps
        foreach (Transform child in gameObject.transform)
        {
            this.CalculateUvMaps(child.gameObject);
        }
    }
}

#endif