using System.Collections.Generic;

namespace QC.Systems.SaveLoad
{
    public interface IDataService
    {
        void Save(GameData data, bool overwrite = true);
		GameData Load(string name);
		void Delete(string name);
		void DeleteAll();
		IEnumerable<string> ListSaves();
		string GetFileLocation(string name);
    }
}