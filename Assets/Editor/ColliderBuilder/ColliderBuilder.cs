using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Generate the colliders for the IFC
/// </summary>
public class ColliderBuilder
{
    private static readonly string[] ifcClassesCollider = { "IfcWall" , "IfcRoof", "IfcBeam", "IfcColumn", "IfcSlab", "IfcCurtainWall", "IfcPlate", "IfcStair", "IfcStairFlight"};

    /// <summary>
    /// Adds the colliders to the building depending on the ifc type
    /// </summary>
    /// <param name="building"></param>
    public void BuildColliders(GameObject building)
    {
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
            else if (productData != null
                && ifcClassesCollider.Contains(productData.IfcClass))
            {
                MeshCollider collider = child.AddComponent<MeshCollider>();
            }
        }
    }
}
