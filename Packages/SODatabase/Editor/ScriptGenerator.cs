using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

[InitializeOnLoad]
public class ScriptGenerator : MonoBehaviour
{
    private const string TemplateBasePath = "./Packages/SODatabase/Template";

    [MenuItem("Assets/Create/Scripting/Database/Database")]
    public static void CreateDatabaseCode()
    {
        CreateFile<EndDBScriptNameEditAction>("NewDatabaseScript.cs", "DB.template");
    }

    [MenuItem("Assets/Create/Scripting/Database/Info")]
    public static void CreateInfoCode()
    {
        CreateFile<EndInfoNameEditAction>("NewInfo.cs", "Info.template");
    }

    [MenuItem("Assets/Create/Scripting/Database/Table")]
    public static void CreateTableCode()
    {
        CreateFile<EndTableNameEditAction>("NewTable.cs", "Table.template");
    }

    private static void CreateFile<T>(string fileName, string templateName) where T : EndNameEditAction
    {
        var directoryPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!string.IsNullOrEmpty(Path.GetExtension(directoryPath)))
            directoryPath = Path.GetDirectoryName(directoryPath);

        var newFilePath = Path.Combine(directoryPath, fileName);
        var templateFilePath = Path.Join(TemplateBasePath, templateName);

        var fileText = File.ReadAllText(templateFilePath);

        File.WriteAllText(newFilePath, fileText);
        AssetDatabase.ImportAsset(newFilePath);
        AssetDatabase.Refresh();
        var asset = AssetDatabase.LoadAssetAtPath(newFilePath, typeof(TextAsset));
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
            asset.GetInstanceID(),
            ScriptableObject.CreateInstance<T>(),
            newFilePath,
            AssetPreview.GetMiniThumbnail(asset),
            newFilePath);
        Selection.activeObject = asset;
    }

    private abstract class EndScriptNameEditActionBase : EndNameEditAction
    {
        protected abstract string TemplateFileName { get; }
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var templateFilePath = Path.Join(TemplateBasePath, TemplateFileName);
            var fileText = File.ReadAllText(templateFilePath);
            Debug.Log(templateFilePath);
            File.Move(resourceFile, pathName);
            File.Move(resourceFile + ".meta", pathName + ".meta");
            File.WriteAllText(pathName,
                fileText.Replace("#CLASSNAME", Path.GetFileNameWithoutExtension(pathName).Replace(" ", ""))
                );
            AssetDatabase.Refresh();
        }
    }
    private class EndDBScriptNameEditAction : EndScriptNameEditActionBase
    {
        protected override string TemplateFileName => "DB.template";
    }

    private class EndTableNameEditAction : EndScriptNameEditActionBase
    {
        protected override string TemplateFileName => "Table.template";
    }

    private class EndInfoNameEditAction : EndScriptNameEditActionBase
    {
        protected override string TemplateFileName => "Info.template";
    }
}
