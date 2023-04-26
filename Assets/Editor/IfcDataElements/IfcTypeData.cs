using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xbim.Ifc4.Interfaces;

public class IfcTypeData : MonoBehaviour
{
    /// <summary>
    /// Ifc class
    /// </summary>
    [ReadOnly] 
    public string IfcClass;

    /// <summary>
    /// Index of the type
    /// </summary>
    [ReadOnly] 
    public int Label;

    /// <summary>
    /// Id of the type
    /// </summary>
    [ReadOnly]
    public string Id;

    /// <summary>
    /// Name of the type
    /// </summary>
    [ReadOnly]
    public string TypeName;

    /// <summary>
    /// related IFC type
    /// </summary>
    public IIfcTypeObject TypeObject 
    { 
        get
        {
            return this.internalType;
        }
        set
        {
            this.internalType = value;
            if (this.internalType != null)
            {
                this.Label = this.internalType.EntityLabel;
                this.Id = this.internalType.GlobalId;
                this.TypeName = this.internalType.Name;
                this.IfcClass = this.internalType.ExpressType.ExpressName;
            }
            else
            {
                this.Label = -1;
                this.Id = "";
                this.TypeName = "";
                this.IfcClass = "";
            }
        }
    }

    /// <summary>
    /// internal Type
    /// </summary>
    private IIfcTypeObject internalType;
}
