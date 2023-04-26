using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xbim.Ifc4.Interfaces;

[Serializable]
public class MaterialMapKeyValue
{
    /// <summary>
    /// Key of the element
    /// </summary>
    [ReadOnly]
    public string IfcMaterialName = "[IFC Material]";

    /// <summary>
    /// Value of the element
    /// </summary>
    private IIfcMaterial internalIfcMaterial;

    /// <summary>
    /// corresponding label
    /// </summary>
    public int IfcLabel { get; private set; }

    /// <summary>
    /// Label of the related appearance
    /// </summary>
    public int SurfaceStyleLabel { get; private set; }

    public IIfcMaterial IfcMaterial
    {
        get { return internalIfcMaterial; }
        set
        {
            this.internalIfcMaterial = value;
            if (this.internalIfcMaterial != null)
            {
                this.IfcMaterialName = internalIfcMaterial.Name;
                this.IfcLabel = internalIfcMaterial.EntityLabel;
                IIfcMaterialDefinitionRepresentation matDefRep = this.IfcMaterial.HasRepresentation.FirstOrDefault();
                if (matDefRep != null)
                {
                    IIfcStyledRepresentation styledRep = matDefRep.Representations.FirstOrDefault(x => x is IIfcStyledRepresentation) as IIfcStyledRepresentation;
                    if (styledRep != null)
                    {
                        IIfcStyledItem item = styledRep.Items.FirstOrDefault(x => x is IIfcStyledItem) as IIfcStyledItem;
                        if (item != null)
                        {
                            IIfcSurfaceStyle surfaceStyle = item.Styles.FirstOrDefault(x => x is IIfcSurfaceStyle) as IIfcSurfaceStyle;
                            if (surfaceStyle != null)
                            {
                                this.SurfaceStyleLabel = surfaceStyle.EntityLabel;
                            }
                        }
                    }
                }
            }
            else
            {
                this.IfcLabel = -1;
                this.IfcMaterialName = null;
                this.SurfaceStyleLabel = -1;
            }
        }
    }

    /// <summary>
    /// Correspondinc material for visualization
    /// </summary>
    public IfcUnityMaterialLink MaterialLink 
    { 
        get
        {
            return this.internalMaterialLink;
        }

        set
        {
            this.internalMaterialLink = value;
            if (this.internalMaterialLink != null)
            {
                this.CorrespondingMaterial = internalMaterialLink.UnityMaterial;
            }
            else
            {
                this.CorrespondingMaterial = null;
            }
        }
    }

    private IfcUnityMaterialLink internalMaterialLink;

    public Material CorrespondingMaterial;
}
