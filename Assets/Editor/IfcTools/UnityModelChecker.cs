#if (UNITY_EDITOR)
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class UnityModelChecker
{
    /// <summary>
    /// checks all gameobjects in all scenes for correct IFC links
    /// </summary>
    public void CheckIfcLinking()
    {
        HelperFunctions.DoForAllGameObjectsInProject(CheckIfcLinking);
    }

    /// <summary>
    /// checks the given gameobject and child objects for correct IFC links
    /// </summary>
    /// <param name="rootObject"></param>
    public void CheckIfcLinking(GameObject rootObject)
    {
        IfcFileAssociation ifcFileAssoc = rootObject.GetComponent<IfcFileAssociation>();
        if (ifcFileAssoc == null)
        {
            Debug.LogWarning("Could not find a file Association in " + rootObject.name);
        }
        else
        {
            if (ifcFileAssoc.IfcModel == null)
                Debug.LogWarning("No IFC Model attached to " + ifcFileAssoc.name + " in " + rootObject.name);

            IfcProjectData ifcProject = rootObject.GetComponent<IfcProjectData>();
            if (ifcProject == null)
            {
                Debug.LogWarning("No Project attached to " + rootObject.name);
            }

            foreach (Transform childTransform in rootObject.transform)
            {
                this.CheckProductLinks(childTransform.gameObject);
            }
        }
    }

    /// <summary>
    /// Checks the materials for all game objects in the project
    /// </summary>
    public void CheckMaterial()
    {
        HelperFunctions.DoForAllGameObjectsInProject(CheckMaterial);
    }

    /// <summary>
    /// Checks if all materials are applied correctly
    /// </summary>
    /// <param name="gameObject"></param>
    public void CheckMaterial(GameObject gameObject)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null
            && renderer.sharedMaterial == null)
        {
            Debug.LogWarning("Renderer " + renderer.name + " in " + gameObject.name + " has no assigned material.");
        }

        foreach (Transform child in gameObject.transform)
        {
            this.CheckMaterial(child.gameObject);
        }
    }

    /// <summary>
    /// recursive checking of attached product links
    /// </summary>
    /// <param name="gameObject"></param>
    private void CheckProductLinks(GameObject gameObject)
    {
        IfcProductData productData = gameObject.GetComponent<IfcProductData>();
        if (productData != null
            && productData.Product == null)
        {
            Debug.LogWarning("Could not find an IFC entity in " + productData.Id + " in " + gameObject.name);
        }

        foreach (Transform child in gameObject.transform)
        {
            this.CheckProductLinks(child.gameObject);
        }
    }

    /// <summary>
    /// checks the currently selected product if the product association and properties are correct
    /// </summary>
    public void CheckSelectedProduct()
    {
        GameObject selectedGameObject = UnityEditor.Selection.activeGameObject;
        Debug.Log("Selected object: " + selectedGameObject.name);
        IfcProductData productData = selectedGameObject.GetComponent<IfcProductData>();
        if (productData == null)
        {
            Debug.Log("No product data found.");
            return;
        }

        Debug.Log("Product ID: " + productData.Id);
        Debug.Log("Product label: " + productData.Label);
        Debug.Log("IFC product attached: " + (productData.Product != null));

        IfcPropertySetData[] propertySets = selectedGameObject.GetComponents<IfcPropertySetData>();
        if (propertySets == null)
        {
            Debug.Log("No property sets attached.");
            return;
        }

        Debug.Log("Number of property sets attached: " + propertySets.Length);
        foreach(IfcPropertySetData propertySet in propertySets)
        {
            Debug.Log("PropertySet: " + propertySet.PropertySetName);
            Debug.Log("IfcPropertySet Attached: " + (propertySet.PropertySet != null));
            Debug.Log("Number of included properties: " + propertySet.Properties.Count);
            foreach (IfcPropertyData propertyData in propertySet.Properties)
            {
                Debug.Log("Included property: " + propertyData.Name);
                Debug.Log("Property attached: " + (propertyData.IfcProperty != null));
            }
        }
    }
}

#endif