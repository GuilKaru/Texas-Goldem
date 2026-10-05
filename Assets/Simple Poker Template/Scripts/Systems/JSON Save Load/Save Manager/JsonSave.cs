using UnityEngine;
using System.IO;

namespace SimplePoker.SaveLoad
{
    public class JsonSave
    {
        public static readonly string DEFAULT_GAME_SAVE = "gamesave";
        public static readonly string DEFAULT_MENU_SAVE = "menusave";
        public static readonly string SAVE_FOLDER = Application.persistentDataPath;

        public static void Save<T>(string dataName, T data, string saveDebug = "Saved")
        {
            string jsonData = JsonUtility.ToJson(data);
            string path = Path.Combine(SAVE_FOLDER, $"{dataName}.json");
            File.WriteAllText(path, jsonData);
            Debug.Log(saveDebug);
        }

        public static T Load<T>(string dataName, string loadDebug = "Loaded") where T : new()
        {
            string path = Path.Combine(SAVE_FOLDER, $"{dataName}.json");
            if (File.Exists(path))
            {
                string jsonData = File.ReadAllText(path);
                T loadedData = JsonUtility.FromJson<T>(jsonData);
                Debug.Log(loadDebug);
                return loadedData;
            }
            else
            {
                Debug.LogWarning($"There is no save with name: {dataName}, an empty save was created");
                T instance = new T();
                Save(dataName, instance);
                return instance;
            }
        }

        public static bool FileExist(string dataName)
        {
            string path = Path.Combine(SAVE_FOLDER, $"{dataName}.json");
            return File.Exists(path);
        }

        public static void DeleteSave(string dataName)
        {
            string path = Path.Combine(SAVE_FOLDER, $"{dataName}.json");
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("Save deleted");
            }
        }
    }
}
