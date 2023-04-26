using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Mapping material names of the IFC and materials
/// </summary>
[Serializable]
public class MaterialMap : MonoBehaviour, IDictionary<string,MaterialMapKeyValue>
{
    /// <summary>
    /// Mappings of the materials
    /// </summary>
    private Dictionary<string, MaterialMapKeyValue> internalMapping = new Dictionary<string, MaterialMapKeyValue>();

    public List<MaterialMapKeyValue> Mapping = new List<MaterialMapKeyValue>();

    public MaterialMapKeyValue this[string key]
    {
        get => ((IDictionary<string, MaterialMapKeyValue>)this.internalMapping)[key];
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

    public ICollection<string> Keys => ((IDictionary<string, MaterialMapKeyValue>)this.internalMapping).Keys;

    public ICollection<MaterialMapKeyValue> Values => ((IDictionary<string, MaterialMapKeyValue>)this.internalMapping).Values;

    public int Count => ((ICollection<KeyValuePair<string, MaterialMapKeyValue>>)this.internalMapping).Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<string, MaterialMapKeyValue>>)this.internalMapping).IsReadOnly;

    public void Add(string key, MaterialMapKeyValue value)
    {
        ((IDictionary<string, MaterialMapKeyValue>)this.internalMapping).Add(key, value);
        this.Mapping.Add(value);
    }

    public void Add(KeyValuePair<string, MaterialMapKeyValue> item)
    {
        ((ICollection<KeyValuePair<string, MaterialMapKeyValue>>)this.internalMapping).Add(item);
        this.Mapping.Add(item.Value);
    }

    public void Clear()
    {
        ((ICollection<KeyValuePair<string, MaterialMapKeyValue>>)this.internalMapping).Clear();
        this.Mapping.Clear();
    }

    public bool Contains(KeyValuePair<string, MaterialMapKeyValue> item)
    {
        return ((ICollection<KeyValuePair<string, MaterialMapKeyValue>>)this.internalMapping).Contains(item);
    }

    public bool ContainsKey(string key)
    {
        return ((IDictionary<string, MaterialMapKeyValue>)this.internalMapping).ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<string, MaterialMapKeyValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, MaterialMapKeyValue>>)this.internalMapping).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<string, MaterialMapKeyValue>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, MaterialMapKeyValue>>)this.internalMapping).GetEnumerator();
    }

    public bool Remove(string key)
    {
        this.Mapping.Remove(this[key]);
        return ((IDictionary<string, MaterialMapKeyValue>)this.internalMapping).Remove(key);
    }

    public bool Remove(KeyValuePair<string, MaterialMapKeyValue> item)
    {
        this.Mapping.Remove(item.Value);
        return ((ICollection<KeyValuePair<string, MaterialMapKeyValue>>)this.internalMapping).Remove(item);
    }

    public bool TryGetValue(string key, out MaterialMapKeyValue value)
    {
        return ((IDictionary<string, MaterialMapKeyValue>)this.internalMapping).TryGetValue(key, out value);
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
            if (ifcProduct.IfcMaterial == null)
            {
                Debug.Log("No IfcMaterial provided for " + ifcProduct.name + " with id " + ifcProduct.Id + ".");
                continue;
            }

            if (!this.ContainsKey(ifcProduct.MaterialName))
            {
                Debug.Log("No mapping entry for " + ifcProduct.MaterialName + ".");
                continue;
            }

            //apply material
            MaterialMapKeyValue kvPair = this[ifcProduct.MaterialName];
            if (kvPair.CorrespondingMaterial == null)
            {
                Debug.Log("No Unity material set for " + ifcProduct.MaterialName);
            }
            else
            {
                renderer.sharedMaterial = kvPair.CorrespondingMaterial;
            }
        }
    }
}
