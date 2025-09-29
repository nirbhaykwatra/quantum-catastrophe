using System;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.IO;
using System.Reflection;

namespace SCP.Systems.SaveLoad
{
    public class SaveDataClassCreator : OdinEditorWindow
    {
        [ShowInInspector]
        [LabelText("Data Class Name")]
        private string m_dataClassType;
        
        [ShowInInspector]
        [LabelText("Data Class Path")]
        [FolderPath(RequireExistingPath = true)]
        private string m_dataClassPath;
        
        [ShowInInspector]
        [Tooltip("The namespace which the data class should be contained in. Leave blank for global.")]
        [LabelText("Data Class Namespace")]
        private string m_dataClassNamespace;
        
        [ShowInInspector]
        [Tooltip("The namespace for the ISaveable interface. Leave blank for global.")]
        [LabelText("ISaveable Namespace")]
        private string m_saveableNamespace;
        
        [MenuItem("Tools/Save System/Save Data Class Creator")]
        private static void OpenWindow() => GetWindow<SaveDataClassCreator>();
        
        [Button(ButtonSizes.Large)]
        private void CreateDataClass()
        {
            string namespacePrefix = GetNamespacePrefix();
            if (CheckClass($"{namespacePrefix}{m_dataClassType}")) return;
            
            string path = $"{m_dataClassPath}/{m_dataClassType}.cs";
            string classContent = GenerateClassContent();
            
            File.WriteAllText(path, classContent);
            AssetDatabase.Refresh();
        }
        
        private string GetNamespacePrefix()
        {
            if (string.IsNullOrWhiteSpace(m_dataClassNamespace))
                return "";
            
            return $"{m_dataClassNamespace.Replace(" ", "")}.";
        }
        
        private string GenerateClassContent()
        {
            bool hasNamespace = !string.IsNullOrWhiteSpace(m_dataClassNamespace);
            bool hasSaveableNamespace = !string.IsNullOrWhiteSpace(m_saveableNamespace);
            string indent = hasNamespace ? "    " : "";
            
            var content = new System.Text.StringBuilder();
            
            // Add using statements
            content.AppendLine("using System;");
            content.AppendLine("using UnityEngine;");
            if (hasSaveableNamespace)
            {
                content.AppendLine($"using {m_saveableNamespace};");
            }
            content.AppendLine();
            
            // Add namespace if specified
            if (hasNamespace)
            {
                content.AppendLine($"namespace {m_dataClassNamespace}");
                content.AppendLine("{");
            }
            
            // Add class definition
            content.AppendLine($"{indent}[Serializable]");
            content.AppendLine($"{indent}public class {m_dataClassType} : ISaveable");
            content.AppendLine($"{indent}{{");
            content.AppendLine($"{indent}    [field: SerializeField] public string Name {{ get; set; }}");
            content.AppendLine($"{indent}}}");
            
            // Close namespace if specified
            if (hasNamespace)
            {
                content.AppendLine("}");
            }
            
            return content.ToString();
        }

        
        private bool CheckClass(string classNameToCheck)
        {
            // Try to get the type from the currently loaded assemblies
            Type type = Type.GetType(classNameToCheck);

            // If not found in the current assembly, search all loaded assemblies
            if (type == null)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(classNameToCheck);
                    if (type != null)
                    {
                        break; // Found the type
                    }
                }
            }

            return (type != null);
        }
    }
}