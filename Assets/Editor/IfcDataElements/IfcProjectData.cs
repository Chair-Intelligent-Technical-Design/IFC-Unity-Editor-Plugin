using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xbim.Ifc4.Interfaces;

public class IfcProjectData : MonoBehaviour
{
    private IIfcProject internalProject;

    /// <summary>
    /// related project
    /// </summary>
    public IIfcProject IfcProject
    {
        get { return internalProject; }
        set 
        { 
            internalProject = value;
            this.Id = internalProject.GlobalId;
            this.Name = internalProject.Name;
        }
    }

    /// <summary>
    /// Id of the Project
    /// </summary>
    [ReadOnly]
    public string Id;

    /// <summary>
    /// Name of the Project
    /// </summary>
    [ReadOnly]
    public string Name;
}
