#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class MenuEntries
{
    /// <summary>
    /// static field for the file loader
    /// </summary>
    private static IfcFileLoader fileLoader;

    static MenuEntries()
    {
        if (fileLoader == null)
        {
            fileLoader = new IfcFileLoader();
        }
    }

    /// <summary>
    /// menu entry to select and import an IFC file
    /// </summary>
    [MenuItem("IFC Tools/Load IFC file")]
    static void LoadIfcFile()
    {
        string ifcFilePath = EditorUtility.OpenFilePanel("Select IFC file", "", "ifc");
        if (!string.IsNullOrEmpty(ifcFilePath))
        {
            fileLoader.LoadIfcFile(ifcFilePath);
        }
    }

    /// <summary>
    /// menu entry to select ifcconvert
    /// </summary>
    [MenuItem("IFC Tools/Set IfcConvert path")]
    static void SetIfcConvertPath()
    {
        string selectedPath = EditorUtility.OpenFilePanel("Select IfcConvert", "", "exe");
        if (!string.IsNullOrEmpty(selectedPath))
        {
            fileLoader.SetIfcConvertPath(selectedPath);
            Debug.Log("IfcConvert Path: " + selectedPath);
        }
    }

    /// <summary>
    /// Menu entry to set output path
    /// </summary>
    [MenuItem("IFC Tools/Set obj output path")]
    static void SetObjOutputPath()
    {
        string selectedPath = EditorUtility.OpenFolderPanel("Select IFC output folder", "", "");
        if (!string.IsNullOrEmpty(selectedPath))
        {
            fileLoader.SetOutputPath(selectedPath);
            Debug.Log("Output path: " + selectedPath);
        }
    }

    /// <summary>
    /// Check the model for errors
    /// </summary>
    [MenuItem("IFC Tools/Check IFC links")]
    public static void CheckIfcLinks()
    {
        Debug.Log("Start IFC link checking...");
        UnityModelChecker checker = new UnityModelChecker();
        checker.CheckIfcLinking();
        Debug.Log("Checking of IFC links finished.");
    }

    [MenuItem("IFC Tools/Check IFC Materials")]
    public static void CheckIfcMaterial()
    {
        Debug.Log("Start material checking...");
        UnityModelChecker checker = new UnityModelChecker();
        checker.CheckMaterial();
        Debug.Log("Material checking finished.");
    }

    [MenuItem("IFC Tools/Check IFC Product")]
    public static void CheckIfcProduct()
    {
        Debug.Log("Start checking Product...");
        UnityModelChecker checker = new UnityModelChecker();
        checker.CheckSelectedProduct();
        Debug.Log("Checking Product finished.");
    }
}

#endif