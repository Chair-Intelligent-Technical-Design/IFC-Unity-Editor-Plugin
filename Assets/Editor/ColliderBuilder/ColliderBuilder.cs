using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

/// <summary>
/// Generate the colliders for the IFC
/// </summary>
public class ColliderBuilder : MonoBehaviour
{
    private static readonly string[] ifcClassesWalls = { "IfcWall", "IfcWallStandardCase", "IfcBeam", "IfcColumn", "IfcCurtainWall", "IfcWindow", "IfcPlate" };

    private static readonly string[] ifcClassesGround = { "IfcRoof", "IfcSlab", "IfcStair", "IfcStairFlight", "IfcSite", "IfcRamp", "IfcRampFlight" };

    private static readonly string[] ifcClassesDoor = { "IfcDoor", "IfcDoorStandardCase"};

    private static readonly int maxLayers = 31;

    private static readonly string wallLayer = "Walls", groundLayer = "Grounds", doorLayer = "Doors";
    private static readonly int wallLayerNumber = 29, groundLayerNumber = 30, doorLayerNumber = 31;

    /// <summary>
    /// Adds the colliders to the building depending on the ifc type
    /// </summary>
    /// <param name="building"></param>
    public void BuildColliders(GameObject building)
    {
        //generate layers
        this.AddLayer(wallLayer, wallLayerNumber);
        this.AddLayer(groundLayer, groundLayerNumber);
        this.AddLayer(doorLayer, doorLayerNumber);

        this.BuildCollidersRecursive(building);
    }

    private void BuildCollidersRecursive(GameObject building)
    {
        for (int idx = 0; idx < building.transform.childCount; idx++)
        {
            GameObject child = building.transform.GetChild(idx).gameObject;
            IfcProductData productData = child.GetComponent<IfcProductData>();
            if (child.transform.childCount > 0)
            {
                BuildCollidersRecursive(child);
            }
            else if (productData != null)
            {
                ColliderType colliderType = this.HasCollider(productData.IfcClass);
                switch (colliderType)
                {
                    case ColliderType.None:
                        continue;
                    case ColliderType.Door:
                        child.layer = doorLayerNumber;
                        break;
                    case ColliderType.Wall:
                        child.layer = wallLayerNumber;
                        break;
                    case ColliderType.Ground:
                        child.layer = groundLayerNumber;
                        break;
                }

                child.AddComponent<MeshCollider>();
            }
        }
    }

    private enum ColliderType
    {
        None, Wall, Ground, Door
    }

    private ColliderType HasCollider(string ifcClassName)
    {
        if (ifcClassesWalls.Contains(ifcClassName)) return ColliderType.Wall;
        else if (ifcClassesGround.Contains(ifcClassName)) return ColliderType.Ground;
        else if (ifcClassesDoor.Contains(ifcClassName)) return ColliderType.Door;
        else return ColliderType.None;
    }

    /// <summary>
    /// Checks if the value exists in the property.
    /// </summary>
    /// <returns><c>true</c>, if exists was propertyed, <c>false</c> otherwise.</returns>
    /// <param name="property">Property.</param>
    /// <param name="start">Start.</param>
    /// <param name="end">End.</param>
    /// <param name="value">Value.</param>
    private static bool PropertyExists(SerializedProperty property, int start, int end, string value)
    {
        for (int i = start; i < end; i++)
        {
            SerializedProperty t = property.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(value))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// add a new layer with the given number
    /// </summary>
    /// <param name="layerName"></param>
    /// <param name="layerNumber"></param>
    /// <returns></returns>
    private bool AddLayer(string layerName, int layerNumber)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        if (!PropertyExists(layersProp, 0, maxLayers, layerName))
        {
            SerializedProperty sp = layersProp.GetArrayElementAtIndex(layerNumber);
            if (sp.stringValue == "")
            {
                // Assign string value to layer
                sp.stringValue = layerName;
                Debug.Log("Layer: " + layerName + " has been added");
                // Save settings
                tagManager.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("Layer number " + layerNumber + " is already used. Please manually add the layer " + layerName);
                return false;
            }
        }
        else
        {
            Debug.Log("Layer " + layerName + " exists already.");
        }
        return true;
    }
}
