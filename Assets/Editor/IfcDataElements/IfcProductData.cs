using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Xbim.Ifc4.Interfaces;

/// <summary>
/// Data elements to represent IFC products
/// </summary>
public class IfcProductData : MonoBehaviour
{
    /// <summary>
    /// Ifc class
    /// </summary>
    [ReadOnly]
    public string IfcClass;

    /// <summary>
    /// Index of the element
    /// </summary>
    [ReadOnly]
    public int Label;

    /// <summary>
    /// Id of the element
    /// </summary>
    [ReadOnly]
    public string Id;

    /// <summary>
    /// all related Materials
    /// </summary>
    private List<IIfcMaterialSelect> internalRelatedMaterials = new List<IIfcMaterialSelect>();

    public IReadOnlyList<IIfcMaterialSelect> RelatedMaterials;

    /// <summary>
    /// Product of the object
    /// </summary>
    public IIfcProduct Product 
    { 
        get
        {
            return this.internalProduct;
        }

        set
        {
            this.internalProduct = value;
            this.IfcClass = this.internalProduct.ExpressType.ExpressName;
            this.Label = this.internalProduct.EntityLabel;
            this.Id = this.internalProduct.GlobalId;
            this.SetMaterials();
        }
    }

    private IIfcProduct internalProduct;

    /// <summary>
    /// create an empty IFC data entity
    /// </summary>
    public IfcProductData()
    {

    }

    /// <summary>
    /// Initialize the data entity with a product
    /// </summary>
    /// <param name="product"></param>
    public IfcProductData(IIfcProduct product)
    {
        this.Product = product;
    }

    private IIfcMaterial ExtractMaterialFromLayerSet(IIfcMaterialLayerSet materialLayerSet)
    {
        if (materialLayerSet.MaterialLayers.Any())
        {
            return materialLayerSet.MaterialLayers[0].Material;
        }

        return null;
    }

    private void SetMaterials()
    {
        this.internalRelatedMaterials.Clear();
        foreach (IIfcRelAssociatesMaterial assocMaterial in this.internalProduct.HasAssociations.Where(x => x is IIfcRelAssociatesMaterial))
        {
            this.internalRelatedMaterials.Add(assocMaterial.RelatingMaterial);
            if (assocMaterial.RelatingMaterial is IIfcMaterialLayerSetUsage layerUsage)
            {
                this.AddLayerSetData(layerUsage.ForLayerSet);
            }
            else if (assocMaterial.RelatingMaterial is IIfcMaterialLayerSet layerSet)
            {
                this.AddLayerSetData(layerSet);
            }
            else if (assocMaterial.RelatingMaterial is IIfcMaterialLayer layer)
            {
                this.AddLayerData(layer);
            }
            else if (assocMaterial.RelatingMaterial is IIfcMaterialConstituentSet constitutentSet)
            {
                this.internalRelatedMaterials.Add(constitutentSet);
                foreach (IIfcMaterialConstituent constiutent in constitutentSet.MaterialConstituents)
                {
                    this.AddConstitutentData(constiutent);
                }
            }
            else if (assocMaterial.RelatingMaterial is IIfcMaterialConstituent constitutent)
            {
                this.AddConstitutentData(constitutent);
            }
            else if (assocMaterial.RelatingMaterial is IIfcMaterial material)
            {
                this.internalRelatedMaterials.Add(material);
            }
        }
        this.RelatedMaterials = this.internalRelatedMaterials.AsReadOnly();
    }

    private void AddConstitutentData(IIfcMaterialConstituent constiutent)
    {
        this.internalRelatedMaterials.Add(constiutent);
        this.internalRelatedMaterials.Add(constiutent.Material);
    }

    private void AddLayerSetData(IIfcMaterialLayerSet forLayerSet)
    {
        this.internalRelatedMaterials.Add(forLayerSet);
        foreach (IIfcMaterialLayer layer in forLayerSet.MaterialLayers)
        {
            this.AddLayerData(layer);
        }
    }

    private void AddLayerData(IIfcMaterialLayer layer)
    {
        this.internalRelatedMaterials.Add(layer);
        this.internalRelatedMaterials.Add(layer.Material);
    }
}
