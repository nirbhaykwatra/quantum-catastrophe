using System;
using System.Collections.Generic;
using System.IO;

namespace SCP.Systems.SaveLoad
{
    public class FileDataService : IDataService
    {
        private BinarySerializer m_serializer;
        private string m_dataPath;
        private string m_fileExtension;
        
        public FileDataService(BinarySerializer serializer)
        {
            this.m_serializer = serializer;
            this.m_dataPath = "Assets\\[GAME NAME HERE]\\Data";
            this.m_fileExtension = ".scp";
        }

        private string GetPathToFile(string name)
        {
            return Path.Combine(m_dataPath, string.Concat(name, m_fileExtension));
        }
        
        public void Save(GameData data, bool overwrite = true)
        {
            // TODO: Add encryption and decryption to binary save file
            string fileLocation = GetPathToFile(data.Name);

            if (!overwrite && File.Exists(fileLocation))
            {
                throw new IOException("File already exists");
            }
            
            File.WriteAllBytes(fileLocation, m_serializer.Serialize(data));
        }
        public GameData Load(string name)
        {
            string fileLocation = GetPathToFile(name);
            
            if (!File.Exists(fileLocation))
            {
                throw new FileNotFoundException("File not found");
            }
            
            return m_serializer.Deserialize<GameData>(File.ReadAllBytes(fileLocation));
        }
        public void Delete(string name)
        {
            string fileLocation = GetPathToFile(name);
            if (!File.Exists(fileLocation))
            {
                throw new FileNotFoundException("File not found");
            }
            
            File.Delete(fileLocation);
        }
        public void DeleteAll()
        {
            foreach (string filePath in Directory.GetFiles(m_dataPath, string.Concat("*", m_fileExtension)))
            {
                File.Delete(filePath);
            }
        }
        public IEnumerable<string> ListSaves()
        {
            foreach (string path in Directory.EnumerateFiles(m_dataPath, string.Concat("*", m_fileExtension)))
            {
                yield return Path.GetFileNameWithoutExtension(path);
            }
        }

        public string GetFileLocation(string name)
        {
            return GetPathToFile(name);
        }
    }
}