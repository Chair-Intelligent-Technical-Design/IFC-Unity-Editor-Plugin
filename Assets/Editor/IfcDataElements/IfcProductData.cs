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
    /// Name of the related material
    /// </summary>
    [ReadOnly]
    public string MaterialName;

    /// <summary>
    /// Related IFC material
    /// </summary>
    private IIfcMaterial internalIfcMaterial;

    /// <summary>
    /// Related IFC Material
    /// </summary>
    public IIfcMaterial IfcMaterial
    {
        get { return internalIfcMaterial; }
        set 
        { 
            internalIfcMaterial = value;
            if (internalIfcMaterial != null)
            {
                this.MaterialName = internalIfcMaterial.Name;
            }
        }
    }


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
            IIfcRelAssociatesMaterial assocMaterial = (IIfcRelAssociatesMaterial)this.internalProduct.HasAssociations.FirstOrDefault(x => x is IIfcRelAssociatesMaterial);
            if (assocMaterial != null)
            {
                if (assocMaterial.RelatingMaterial is IIfcMaterial material)
                {
                    this.IfcMaterial = material;
                }
                else if (assocMaterial.RelatingMaterial is IIfcMaterialLayerSet materialLayerSet)
                {
                    this.IfcMaterial = this.ExtractMaterialFromLayerSet(materialLayerSet);
                }
                else if (assocMaterial.RelatingMaterial is IIfcMaterialLayerSetUsage layerUsage)
                {
                    this.IfcMaterial = this.ExtractMaterialFromLayerSet(layerUsage.ForLayerSet);
                }
            }
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
}
