using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Xbim.Ifc4.Interfaces;

/// <summary>
/// Mapping material names of the IFC and materials
/// </summary>
[Serializable]
public class MaterialMap : MonoBehaviour, IDictionary<int,MaterialMapKeyValue>
{
    /// <summary>
    /// Mappings of the materials
    /// </summary>
    private Dictionary<int, MaterialMapKeyValue> internalMapping = new Dictionary<int, MaterialMapKeyValue>();

    public List<MaterialMapKeyValue> Mapping = new List<MaterialMapKeyValue>();

    public MaterialMapKeyValue this[int key]
    {
        get => ((IDictionary<int, MaterialMapKeyValue>)this.internalMapping)[key];
        set
        {
            this.Mapping.Remove(this[key]);
            this.internalMapping[key] = value;
            this.Mapping.Add(value);
        }
    }


    /// <summary>
    /// Related building model
    /// </summary>
    public GameObject BuildingModel { get; set; }

    public ICollection<int> Keys => ((IDictionary<int, MaterialMapKeyValue>)this.internalMapping).Keys;

    public ICollection<MaterialMapKeyValue> Values => ((IDictionary<int, MaterialMapKeyValue>)this.internalMapping).Values;

    public int Count => ((ICollection<KeyValuePair<int, MaterialMapKeyValue>>)this.internalMapping).Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<int, MaterialMapKeyValue>>)this.internalMapping).IsReadOnly;

    public void Add(int key, MaterialMapKeyValue value)
    {
        ((IDictionary<int, MaterialMapKeyValue>)this.internalMapping).Add(key, value);
        this.Mapping.Add(value);
    }

    public void Add(KeyValuePair<int, MaterialMapKeyValue> item)
    {
        ((ICollection<KeyValuePair<int, MaterialMapKeyValue>>)this.internalMapping).Add(item);
        this.Mapping.Add(item.Value);
    }

    public void Clear()
    {
        ((ICollection<KeyValuePair<int, MaterialMapKeyValue>>)this.internalMapping).Clear();
        this.Mapping.Clear();
    }

    public bool Contains(KeyValuePair<int, MaterialMapKeyValue> item)
    {
        return ((ICollection<KeyValuePair<int, MaterialMapKeyValue>>)this.internalMapping).Contains(item);
    }

    public bool ContainsKey(int key)
    {
        return ((IDictionary<int, MaterialMapKeyValue>)this.internalMapping).ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<int, MaterialMapKeyValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<int, MaterialMapKeyValue>>)this.internalMapping).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<int, MaterialMapKeyValue>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<int, MaterialMapKeyValue>>)this.internalMapping).GetEnumerator();
    }

    public bool Remove(int key)
    {
        this.Mapping.Remove(this[key]);
        return ((IDictionary<int, MaterialMapKeyValue>)this.internalMapping).Remove(key);
    }

    public bool Remove(KeyValuePair<int, MaterialMapKeyValue> item)
    {
        this.Mapping.Remove(item.Value);
        return ((ICollection<KeyValuePair<int, MaterialMapKeyValue>>)this.internalMapping).Remove(item);
    }

    public bool TryGetValue(int key, out MaterialMapKeyValue value)
    {
        return ((IDictionary<int, MaterialMapKeyValue>)this.internalMapping).TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)this.internalMapping).GetEnumerator();
    }

    /// <summary>
    /// applies all corresponding materials
    /// </summary>
    public void ApplyMaterials()
    {
        this.ApplyMaterialsRecursive(BuildingModel);
    }

    private void ApplyMaterialsRecursive(GameObject gameObject)
    {
        foreach (Transform childElement in gameObject.transform)
        {
            GameObject modelElement = childElement.gameObject;

            //apply materials to child elements
            if (modelElement.transform.childCount > 0)
            {
                this.ApplyMaterialsRecursive(modelElement);
            }

            // get the IFC Product
            IfcProductData ifcProduct = modelElement.GetComponent<IfcProductData>();

            //check product data
            if (ifcProduct == null)
            {
                continue;
            }

            //check renderer
            Renderer renderer = modelElement.GetComponent<Renderer>();
            if (renderer == null)
            {
                continue;
            }

            //check if a material is assigned
            if (ifcProduct.RelatedMaterials.Count == 0)
            {
                Debug.Log("No IfcMaterial provided for " + ifcProduct.name + " with id " + ifcProduct.Id + ".");
                continue;
            }

            MaterialMapKeyValue kvPair = this.SearchForLabel(ifcProduct);
            if (kvPair == null)
            {
                Debug.Log("No mapping entry for any material of " + ifcProduct.Label + ": " + ifcProduct.gameObject.name + ".");
                continue;
            }

            //apply material
            if (kvPair.CorrespondingMaterial == null)
            {
                Debug.Log("No Unity material set for " + ifcProduct.Label + ": " + ifcProduct.gameObject.name + ".");
            }
            else
            {
                renderer.sharedMaterial = kvPair.CorrespondingMaterial;
            }
        }
    }

    private MaterialMapKeyValue SearchForLabel(IfcProductData ifcProduct)
    {
        foreach (IIfcMaterialSelect material in ifcProduct.RelatedMaterials)
        {
            if (this.ContainsKey(material.EntityLabel))
            {
                return this[material.EntityLabel];
            }
        }

        return null;
    }
}
