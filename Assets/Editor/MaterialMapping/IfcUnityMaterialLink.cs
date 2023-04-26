using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfcUnityMaterialLink
{
    /// <summary>
    /// Label of the related IFC entity
    /// </summary>
    public int IfcLabel { get; set; }

    /// <summary>
    /// Material
    /// </summary>
    public Material UnityMaterial { get; set; }

}
