using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;

/// <summary>
/// Parses an mtl file to unity materials
/// </summary>
public class MaterialParser
{
    /// <summary>
    /// Enum to set the shader mode
    /// </summary>
    private enum BlendMode
    {
        Opaque,
        Cutout,
        Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
        Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
    }

    /// <summary>
    /// generates all materials defined in a mtl file
    /// </summary>
    /// <param name="mtlFilePath"></param>
    /// <returns></returns>
    public List<IfcUnityMaterialLink> MaterialsFromStlFile(string mtlFilePath)
    {

        List<IfcUnityMaterialLink> result = new List<IfcUnityMaterialLink>();
        string[] linesOfMtlFile = File.ReadAllLines(mtlFilePath);

        List<string> materialLines = new List<string>();
        //iterate through the lines in the file
        foreach (string line in linesOfMtlFile)
        {
            //skip empty/comment lines
            if (string.IsNullOrWhiteSpace(line)
                || line.StartsWith("#"))
                continue;
            
            if (line.StartsWith("newmtl")
                && materialLines.Count > 0)
            {
                //generate material
                result.Add(MaterialFromMtlString(materialLines));
                //reset string lines
                materialLines.Clear();
            }

            //add new line
            materialLines.Add(line);
        }
        return result;
    }

    /// <summary>
    /// Generates an individual material from an entry in a mtl file
    /// </summary>
    /// <param name="mtlLines"></param>
    /// <returns></returns>
    private IfcUnityMaterialLink MaterialFromMtlString(IEnumerable<string> mtlLines)
    {
        IfcUnityMaterialLink result = new IfcUnityMaterialLink();
        result.UnityMaterial = new Material(Shader.Find("Standard"));

        //regex that finds the material name and stores it in the gourp materialName
        string nameGroupMaterialName = "materialName";
        string nameGroupIfcLabel = "ifcLabel";
        Regex regexNewMaterial = new Regex(@"(?'prefix'^newmtl surface-style-)(?'" + nameGroupIfcLabel + @"'\d{1,})-(?'" + nameGroupMaterialName + "'.*)");

        //regex to get the material color
        string nameGroupRed = "red";
        string nameGroupGreen = "green";
        string nameGroupBlue = "blue";
        Regex regexDiffuseReflection = new Regex(@"(?'diffuse'^Kd) (?'" + nameGroupRed + @"'\d+.\d+) (?'" + nameGroupGreen + @"'\d+.\d+) (?'" + nameGroupBlue + @"'\d+.\d+)");

        //regex for the specular reflectivity
        Regex regexSpecularReflection = new Regex(@"(?'specular'^Ks) (?'" + nameGroupRed + @"'\d+.\d+) (?'" + nameGroupGreen + @"'\d+.\d+) (?'" + nameGroupBlue + @"'\d+.\d+)");

        //regex for the specular exponent
        string valueName = "value";
        Regex regexSpecularExp = new Regex(@"(?'specularExponent'^Ns) (?'" + valueName + @"'\d+)");

        //regex for transparency factor
        Regex regexTransparency = new Regex(@"(?'transparency'^d) (?'" + valueName + @"'\d*(\.?(\d*))?)");

        //culture to correctly parse floats
        CultureInfo cultureEng = CultureInfo.GetCultureInfo("en-US");

        foreach (string line in mtlLines)
        {
            //store get the name of the new material
            Match newMtlMatch = regexNewMaterial.Match(line);
            if (newMtlMatch.Success)
            {
                Group groupMaterialName = newMtlMatch.Groups[nameGroupMaterialName];
                result.UnityMaterial.name = newMtlMatch.Groups[nameGroupIfcLabel].Value + "-" + groupMaterialName.Value;
                result.IfcLabel = int.Parse(newMtlMatch.Groups[nameGroupIfcLabel].Value);
                continue;
            }

            //get the diffuse reflection (color) of the new material
            newMtlMatch = regexDiffuseReflection.Match(line);
            if (newMtlMatch.Success)
            {
                float red = float.Parse(newMtlMatch.Groups[nameGroupRed].Value, cultureEng);
                float green = float.Parse(newMtlMatch.Groups[nameGroupGreen].Value, cultureEng);
                float blue = float.Parse(newMtlMatch.Groups[nameGroupBlue].Value, cultureEng);
                result.UnityMaterial.color = new Color(red, green, blue, result.UnityMaterial.color.a);
                continue;
            }

            //get the specular reflectivity of a material
            newMtlMatch = regexSpecularReflection.Match(line);
            if (newMtlMatch.Success)
            {
                float red = float.Parse(newMtlMatch.Groups[nameGroupRed].Value, cultureEng);
                float green = float.Parse(newMtlMatch.Groups[nameGroupGreen].Value, cultureEng);
                float blue = float.Parse(newMtlMatch.Groups[nameGroupBlue].Value, cultureEng);
                result.UnityMaterial.SetColor("_Specular", new Color(red, green, blue));
                continue;
            }

            newMtlMatch = regexSpecularExp.Match(line);
            if (newMtlMatch.Success)
            {
                //divided by 1000 because that is the normal range
                float value = float.Parse(newMtlMatch.Groups[valueName].Value, cultureEng) / 1000f;
                value = Math.Min(value, 1);
                result.UnityMaterial.SetFloat("_Smoothness", value);
                continue;
            }

            newMtlMatch = regexTransparency.Match(line);
            if (newMtlMatch.Success)
            {
                BlendMode mode = (BlendMode)result.UnityMaterial.GetFloat("_Mode");
                result.UnityMaterial.SetFloat("_Mode", (float)BlendMode.Transparent);
                mode = (BlendMode)result.UnityMaterial.GetFloat("_Mode");
                float value = float.Parse(newMtlMatch.Groups[valueName].Value, cultureEng);
                result.UnityMaterial.color = new Color(result.UnityMaterial.color.r, 
                    result.UnityMaterial.color.g, result.UnityMaterial.color.b, value);
                continue;
            }
        }

        return result;
    }
}
