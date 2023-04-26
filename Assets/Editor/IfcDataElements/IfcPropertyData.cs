using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xbim.Ifc4.Interfaces;

[Serializable]
/// <summary>
/// Class to represent the property data
/// </summary>
public class IfcPropertyData
{
    private IIfcProperty internalProperty;

    /// <summary>
    /// related Property
    /// </summary>
    public IIfcProperty IfcProperty
    {
        get { return internalProperty; }
        set 
        { 
            internalProperty = value; 
            if (this.internalProperty != null)
            {
                this.Name = this.internalProperty.Name;
                this.Label = this.internalProperty.EntityLabel;
                this.IfcType = this.internalProperty.ExpressType.ExpressName;
                if (this.internalProperty is IIfcPropertySingleValue singleValue)
                {
                    this.PropertyValue = singleValue.NominalValue.ToString();
                }
            }
            else
            {
                this.Name = string.Empty;
                this.Label = -1;
                this.IfcType = string.Empty;
            }
        }
    }

    /// <summary>
    /// Name of the property
    /// </summary>
    [ReadOnly]
    public string Name;

    /// <summary>
    /// label of the property
    /// </summary>
    [ReadOnly]
    public int Label;

    /// <summary>
    /// IFC type of the property
    /// </summary>
    [ReadOnly]
    public string IfcType;

    /// <summary>
    /// Value of the related property
    /// </summary>
    [ReadOnly]
    public string PropertyValue;

    /// <summary>
    /// default constructor
    /// </summary>
    public IfcPropertyData()
    {

    }

    /// <summary>
    /// initializes the property data element with the related IFC property
    /// </summary>
    /// <param name="property"></param>
    public IfcPropertyData(IIfcProperty property)
    {
        this.IfcProperty = property;
    }

}
