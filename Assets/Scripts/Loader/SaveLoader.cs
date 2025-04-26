using System.Collections.Generic;
using System.IO;
using Manager;
using Newtonsoft.Json;
using UnityEngine;

namespace Loader
{
    public class SaveLoader : Singleton<SaveLoader>
    {
        private Dictionary<int,Dictionary<string,object>> saveDataDict = new Dictionary<int, Dictionary<string,object>>();
        private string saveDirectory = "Saves";
        private void Awake()
        {
            LoadSaveData();
            // 确保存档目录存在
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
        }
        public void LoadSaveData()
        {
            saveDataDict.Clear();
            var saveFiles = Directory.GetFiles(saveDirectory, "*.json");
            if (saveFiles.Length == 0)
            {
                Debug.LogWarning("No save files found！");
                return;
            }
            foreach (var filePath in saveFiles)
            {
                try
                {
                    var json = File.ReadAllText(filePath); // 读取 JSON 文件
                    var saveData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    saveDataDict.Add(int.Parse(fileName.Replace("save", "")),saveData);
                    Debug.Log($"save {Path.GetFileName(filePath)} load success！");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"load save faild: {filePath}, error message: {e.Message}");
                }
            }
        }

        public Dictionary<int, Dictionary<string, object>> SaveDataDict
        {
            get => saveDataDict;
            set => saveDataDict = value;
        }
    }
}