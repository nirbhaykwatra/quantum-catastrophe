using System;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using SCP.Utilities;

namespace SCP.Systems.SaveLoad
{
    public class SaveLoadManager : PersistentSingleton<SaveLoadManager>
    {
        // TODO: Create/subscribe to events for binding
        
        [SerializeField] public GameData GameData;
        private IDataService m_dataService;

        protected override void Awake()
        {
            base.Awake();
            m_dataService = new FileDataService(new BinarySerializer());
        }

        [Button("Bind Data", ButtonSizes.Gigantic)]
        private void BindData()
        {
            
        }

        private void Bind<T, TData>(TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
            var entity = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if (entity != null)
            {
                if (data == null)
                {
                    entity.Bind(new TData());
                }
                entity.Bind(data);
            }
        }

        public void NewGame()
        {
            GameData = new GameData
            {
                Name = "New Game",
                CurrentLevelName = "Demo"
            };
            //UnityEngine.SceneManagement.SceneManager.LoadScene(GameData.CurrentLevelName);
        }
        [Button("Save Game", ButtonSizes.Large)]
        public void SaveGame()
        {
            m_dataService.Save(GameData);
        }

        public void LoadGame(string gameName)
        {
            GameData = m_dataService.Load(gameName);
            BindData();

            if (String.IsNullOrWhiteSpace(GameData.CurrentLevelName))
            {
                GameData.CurrentLevelName = "Demo";
            }
            
            //UnityEngine.SceneManagement.SceneManager.LoadScene(GameData.CurrentLevelName);
        }
        
        [Button("Load Game", ButtonSizes.Large)]
        private void LoadGame() => LoadGame(GameData.Name);
        
        public void ReloadGame() => LoadGame(GameData.Name);
        public void DeleteGame(string gameName) => m_dataService.Delete(gameName);
        public void DeleteAllGames() => m_dataService.DeleteAll();
    }
}