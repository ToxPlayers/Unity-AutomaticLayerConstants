#if UNITY_EDITOR 
using System.Collections.Generic; 
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
static public class LayersCodeGenerator  
{ 
    static private string LayersFileName = @"Layers.cs";

    [MenuItem("Tools/Generate Layers Constants")]
    private static void Generate()
    {
        var filePath = GetFilePath();
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = Application.dataPath + $"/{LayersFileName}.cs";
            File.WriteAllText(filePath, "");
        }

        var content = File.ReadAllText(filePath);
        var genContent = GenerateCode();
        if (content != genContent)
        {
            File.WriteAllText(filePath, genContent);
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
        }
    }
     
    static string GetFilePath()
    {
        var assets = AssetDatabase.FindAssets("t:script Layers");
        foreach(var asset in assets)
        {
            var path = AssetDatabase.GUIDToAssetPath(asset); 
            if ( Path.GetFileName(path) == "Layers.cs")
                return path;
        }
        return null;
    }


    static string GenerateCode()
    { 
        var fieldsCode = "";
        List<string> layersAdded = new List<string>();
        for (int i = 0; i < 32; i++)
        {
           var layerName = InternalEditorUtility.GetLayerName(i); 
           if( ! string.IsNullOrEmpty(layerName) )
           {
                layerName = layerName.Replace(" ", "_");
                if(layersAdded.Contains(layerName) )
                {
                    Debug.LogError($"Multiple layers with the same name. ({layerName})");
                    continue;
                }

                layersAdded.Add(layerName);
                var layerCode = LayerIndexTemplate
                 .Replace(NameReplacer, layerName).Replace(ValueReplacer, i.ToString());
                fieldsCode += layerCode;
           }
        }

        var index = ClassTemplate.IndexOf("}");
        return ClassTemplate.Insert(index, fieldsCode);
    }

    static string NameReplacer = "<name>";
    static string ValueReplacer = "<value>";
    static string ClassTemplate =
@"static public class Layers
{}
";
    static string LayerIndexTemplate =
$@"    
    public const int {NameReplacer} = {ValueReplacer};
    public const int {NameReplacer}Mask = 1 << {ValueReplacer};
";

     
}
#endif
