using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Xbim.Ifc4.Interfaces;

[Serializable]
public class IfcPropertySetData : MonoBehaviour
{
    /// <summary>
    /// included Properties
    /// </summary>
    public List<IfcPropertyData> Properties = new List<IfcPropertyData>();

    /// <summary>
    /// Name of the property set
    /// </summary>
    [ReadOnly]
    public string PropertySetName;

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

    private IIfcPropertySet internalPropertySet;        

    /// <summary>
    /// Property from the IFC model
    /// </summary>
    public IIfcPropertySet PropertySet
    {
        get { return internalPropertySet; }
        set 
        { 
            internalPropertySet = value;
            this.Properties.Clear();

            if (this.internalPropertySet != null)
            {
                this.PropertySetName = this.internalPropertySet.Name;
                this.IfcClass = this.internalPropertySet.ExpressType.ExpressName;
                this.Label = this.internalPropertySet.EntityLabel;
                this.Id = this.internalPropertySet.GlobalId;
                if (this.internalPropertySet.HasProperties.Any())
                {
                    this.Properties.AddRange(this.internalPropertySet.HasProperties.Select(x => new IfcPropertyData(x))); 
                }
            }
            else
            {
                this.PropertySetName = string.Empty;
                this.IfcClass = string.Empty;
                this.Label = -1;
                this.Id = string.Empty;
            }
        }
    }

}
