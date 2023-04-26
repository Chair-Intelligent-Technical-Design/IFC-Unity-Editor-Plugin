using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Xbim.Ifc;

public class IfcFileAssociation : MonoBehaviour
{
    /// <summary>
    /// associated file
    /// </summary>
    [ReadOnly]
    public string IfcFile;

    /// <summary>
    /// Model of the IFC file
    /// </summary>
    public IfcStore IfcModel 
    { 
        get
        {
            return this.internalIfcModel;
        }

        set
        {
            this.internalIfcModel = value;
            if (this.internalIfcModel != null)
            {
                this.IfcFile = Path.GetRelativePath(Application.streamingAssetsPath,this.internalIfcModel.FileName).Replace('\\','/');
            }
        }
    }

    private IfcStore internalIfcModel;

    /// <summary>
    /// Initialization during runtime
    /// </summary>
    public void Start()
    {
        if (!string.IsNullOrWhiteSpace(IfcFile))
        {
            if (this.IfcModel != null)
            {
                this.IfcModel.Close();
                this.IfcModel.Dispose();
            }

            this.IfcModel = IfcStore.Open(Application.streamingAssetsPath + "/" + IfcFile);
            IfcEntityLinker linker = new IfcEntityLinker();
            linker.LinkEntitiesByProductData(this.gameObject, this.IfcModel);
        }
    }

    /// <summary>
    /// frees the memory occupied by the IFC model
    /// </summary>
    private void OnDestroy()
    {
        if (this.IfcModel != null)
        {
            this.IfcModel.Close();
            this.IfcModel.Dispose();
        }
    }
}
