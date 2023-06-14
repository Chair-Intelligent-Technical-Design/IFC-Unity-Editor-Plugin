#if (UNITY_EDITOR)
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using UnityEditor;
using System;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// class to load IFC files. Mainly based on http://cad-3d.blogspot.com/2018/09/getting-bim-data-into-unity-part-9.html and adjusted with the inclusion of xBIM (https://docs.xbim.net/)
/// </summary>
public class IfcFileLoader : IDisposable
{
    /// <summary>
    /// Configuration object to store the configuration for the project
    /// </summary>
    private readonly Config configuration;

    /// <summary>
    /// path for the config file
    /// </summary>
    private readonly string configFilePath = @".\Assets\Editor\config.xml";

    /// <summary>
    /// default constructor
    /// </summary>
    public IfcFileLoader()
    {
        if (!File.Exists(configFilePath))
        {
            this.configuration = new Config(true);
            configuration.Serialize(configFilePath);
        }
        else
        {
            configuration = Config.Deserialize(configFilePath);
        }

        Debug.Log("Starting IFC Unity Editor Plugin");
        Debug.Log("Current working directory: " + Directory.GetCurrentDirectory());
        Debug.Log("IfcConvert Path: " + configuration.IfcConvertPath);
    }



    /// <summary>
    /// sets the used path for IfcConvert
    /// </summary>
    /// <param name="ifcConvertPath"></param>
    public void SetIfcConvertPath(string ifcConvertPath)
    {
        this.configuration.IfcConvertPath = ifcConvertPath;
    }

    /// <summary>
    /// sets the output path of IfcConvert
    /// </summary>
    /// <param name="outputPath"></param>
    public void SetOutputPath(string outputPath)
    {
        this.configuration.OutputPath = outputPath;
    }

    /// <summary>
    /// Loads the IFC file from the given path and returns the generated game object
    /// </summary>
    /// <param name="ifcFilePath"></param>
    /// <returns></returns>
    public GameObject LoadIfcFile(string ifcFilePath)
    {
        if (!File.Exists(this.configuration.IfcConvertPath))
        {
            Debug.LogError("Could not find IfcConvert. Please set the path via IFC Tools/Set IfcConvert Path.");
            return null;
        }

        if (!Directory.Exists(this.configuration.OutputPath))
        {
            Debug.Log("Output directory " + this.configuration.OutputPath + " does not exist. Create...");
            Directory.CreateDirectory(this.configuration.OutputPath);
        }

        Debug.Log("IFC file Path: " + ifcFilePath);
        string ifcFileName = Path.GetFileName(ifcFilePath);
        string objOutputFileName = ifcFileName.Replace("ifc", "obj");
        string mtlFileName = ifcFileName.Replace("ifc", "mtl");
        string objOutputPath = Path.Combine(configuration.OutputPath, objOutputFileName).Replace('\\', '/');
        string mtlOutputPath = Path.Combine(configuration.OutputPath, mtlFileName).Replace('\\', '/');
        Debug.Log("obj output path: " + objOutputPath);

        //copy ifc file if necessary
        //string assetPath = Directory.GetCurrentDirectory() + "\\Assets";
        string fullStreamingAssetPath = Application.streamingAssetsPath;
        if (!ifcFilePath.StartsWith(fullStreamingAssetPath))
        {
            string cwd = Directory.GetCurrentDirectory();
            string relativeStreamingAssetPath = Path.GetRelativePath(cwd, fullStreamingAssetPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(Path.GetRelativePath(cwd, Application.streamingAssetsPath)))
            {
                AssetDatabase.CreateFolder("Assets", relativeStreamingAssetPath.Replace("Assets/", ""));
            }
            if (!AssetDatabase.IsValidFolder(Path.Combine(relativeStreamingAssetPath, "ifc").Replace('\\','/')))
            {
                AssetDatabase.CreateFolder(relativeStreamingAssetPath, "ifc");
            }
            string targetPath = relativeStreamingAssetPath + "/ifc/" + ifcFileName;

            if (File.Exists(targetPath))
            {
                Debug.LogWarning("File " + targetPath + " exists already. Overwriting...");
                File.Delete(targetPath);
            }
            File.Copy(ifcFilePath, targetPath);
            ifcFilePath = targetPath;
        }

        // delete file if exists
        DeleteFile(objOutputPath);
        AssetDatabase.Refresh();

        //define process for ifcconvert with arguments and obj output
        System.Diagnostics.ProcessStartInfo ifcProcessInfo = GenerateProcessInformation(ifcFilePath, objOutputPath);

        //start obj generation
        StartConversion(ifcProcessInfo);

        //load model into scene
        if (!File.Exists(objOutputPath))
        {
            Debug.LogError("Could not find obj file: " + objOutputPath);
            return null;
        }
        AssetDatabase.Refresh();
        string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), objOutputPath);
        GameObject parsedIfc = MonoBehaviour.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(relativePath));
        parsedIfc.name = Path.GetFileNameWithoutExtension(ifcFileName);
        parsedIfc.transform.localScale = new Vector3(5, 5, 5);

        parsedIfc.transform.Rotate(-90, 0, 0);

        //Load and parse IFC
        IfcEntityLinker entityLinker = new IfcEntityLinker();
        IfcStore store = IfcStore.Open(ifcFilePath);
        Debug.Log("opened IFC file");

        entityLinker.LinkIfcEntitiesByName(parsedIfc, store);
        entityLinker.GenerateIfcElementHierarchy(parsedIfc);
        UvMapGenerator mapGenerator = new UvMapGenerator();
        mapGenerator.CalculateUvMaps(parsedIfc);

        //parse mtl if exists
        List<IfcUnityMaterialLink> unityMaterials = null;
        if (File.Exists(mtlOutputPath))
        {
            MaterialParser mtl = new MaterialParser();
            unityMaterials = mtl.MaterialsFromStlFile(mtlOutputPath);
            string folderMaterials = "Assets/Materials";
            if (!AssetDatabase.IsValidFolder(folderMaterials))
            {
                string[] folderStructure = folderMaterials.Split(new char[] { '/' }, 2);
                AssetDatabase.CreateFolder(folderStructure[0], folderStructure[1]);
            }

            foreach (IfcUnityMaterialLink mtlMaterial in unityMaterials)
            {
                mtlMaterial.UnityMaterial.name = mtlMaterial.UnityMaterial.name.Replace('/', '-').Replace("<","").Replace(">","");
                AssetDatabase.CreateAsset(mtlMaterial.UnityMaterial, folderMaterials + "/" + mtlMaterial.UnityMaterial.name + ".mat");
            }
            AssetDatabase.Refresh();
        }

        entityLinker.LinkMaterials(parsedIfc, unityMaterials);

        // add colliders
        ColliderBuilder colliderBuilder = new ColliderBuilder();
        colliderBuilder.BuildColliders(parsedIfc);

        //Mark scene as unsafed
        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);

        return parsedIfc;
    }

    /// <summary>
    /// helper function to start process and log results
    /// </summary>
    /// <param name="processInfo"></param>
    private void StartConversion(System.Diagnostics.ProcessStartInfo processInfo)
    {
        using (System.Diagnostics.Process ifcProcess = System.Diagnostics.Process.Start(processInfo))
        {
            ifcProcess.WaitForExit();
        }
    }

    /// <summary>
    /// Helper function to generate process information
    /// </summary>
    /// <param name="ifcFilePath">Path to the source ifc file</param>
    /// <param name="outputPath">Path for the output file</param>
    /// <returns></returns>
    private System.Diagnostics.ProcessStartInfo GenerateProcessInformation(string ifcFilePath, string outputPath)
    {
        System.Diagnostics.ProcessStartInfo ifcProcessInfo =
            new System.Diagnostics.ProcessStartInfo(configuration.IfcConvertPath)
            {
                CreateNoWindow = false,
                UseShellExecute = true
            };
        string arguments = "--use-element-guids \"" + ifcFilePath + "\" \"" + outputPath + "\"";
        Debug.Log("ifcconvert arguments: " + arguments);
        ifcProcessInfo.Arguments = arguments;
        return ifcProcessInfo;
    }

    /// <summary>
    /// delete file if exists
    /// </summary>
    /// <param name="filePath"></param>
    private void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            Debug.Log("File " + filePath + " already exists. deleting");
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// unregister event handler
    /// </summary>
    public void Dispose()
    {

    }
}

#endif