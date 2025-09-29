using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using FilePathAttribute = Sirenix.OdinInspector.FilePathAttribute;

namespace SCP.Systems.SaveLoad
{
    // TODO: Programmatically add an instance of the data class to GameData in SaveLoadManager
    // Add a Bind<T, TData> call, using some event or other, using the GameData from SaveLoadManager.
    public class SaveDataManagerWindow : OdinEditorWindow
    {
        [ShowInInspector]
        [PropertyOrder(1)]
        [FilePath(ParentFolder = "Assets/[GAME NAME HERE]", RequireExistingPath = true)]
        private string m_saveFilePath;
        
        [ShowInInspector]
        [SerializeField]
        [PropertyOrder(2)]
        public GameData GameData;
        
        private IDataService m_dataService;
        
        [MenuItem("Tools/Save System/Save Data Manager")]
        private static void OpenWindow() => GetWindow<SaveDataManagerWindow>();

        protected override void OnEnable()
        {
            m_dataService = new FileDataService(new BinarySerializer());
        }
        
        protected override void OnDisable()
        {
            m_dataService = null;
        }
        
        [PropertyOrder(3)]
        [Button("New Game",ButtonSizes.Large)]
        public void NewGame()
        {
            GameData = new GameData
            {
                Name = "New Game",
                CurrentLevelName = "SCP_Demo"
            };
        }
        
        [PropertyOrder(4)]
        [Button("Save Game",ButtonSizes.Large)]
        public void SaveGame()
        {
            m_dataService.Save(GameData);
        }
        
        [PropertyOrder(5)]
        [Button("Load Game",ButtonSizes.Large)]
        public void LoadGame()
        {
            GameData = m_dataService.Load((Path.GetFileNameWithoutExtension(m_saveFilePath)));
        }
        
        // Deserialize to a GameData object from the save file and show it in the editor. Use a FileDataService object to
        // enable saving data edited in the window.
        
    }
}
