using System;
using System.Collections.Generic;
using System.Linq;
using Loader;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SavePanel : MonoBehaviour
    {
        [SerializeField] private GameObject emptySaveSlotPrefab;
        [SerializeField] private GameObject saveSlotPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform saveSlotContainer;
        [SerializeField] private List<int> saveFiles;
        private const int MaxSaveSlots = 3;
        private void Start()
        {
            closeButton.onClick.AddListener(HidePanel);
            UpdateSlots();
        }
        
        private void LoadSaveSlots()
        {
            foreach (Transform child in saveSlotContainer)
            {
                Destroy(child.gameObject);
            }
            var slotCount = Mathf.Max(saveFiles.Count, MaxSaveSlots); // 确保至少有3个槽
            for (var i = 1; i <= slotCount; i++)
            {
                if (saveFiles.Contains(i))
                {
                    // 存档文件存在
                    var slot = Instantiate(saveSlotPrefab, saveSlotContainer);
                    slot.transform.Find("SaveName").GetComponent<Text>().text = (string) SaveLoader.Instance.SaveDataDict[i]["saveName"];
                    var i1 = i;
                    // 加载存档按钮的点击事件
                    slot.transform.Find("LoadButton").GetComponent<Button>().onClick.AddListener(() => LoadSave(i1));
                    // 保存存档按钮的点击事件
                    slot.transform.Find("SaveButton").GetComponent<Button>().onClick.AddListener(() => Save(i1,"save" + i1));
                    // 删除存档按钮的点击事件
                    slot.transform.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(() => DeleteSave(i1));
                }
                else
                {
                    // 没有存档，显示空存档槽
                    var emptySlot = Instantiate(emptySaveSlotPrefab, saveSlotContainer);
                    var i1 = i;
                    emptySlot.transform.Find("SaveButton").GetComponent<Button>().onClick.AddListener(() => Save(i1,"save" + i1));
                    emptySlot.transform.Find("SaveName").GetComponent<Text>().text = "空存档" + i; // 提示没有存档
                }
            }
        }

        private void UpdateSlots()
        {
            SaveLoader.Instance.LoadSaveData();
            saveFiles = SaveLoader.Instance.SaveDataDict.Keys.ToList();
            LoadSaveSlots(); // 重新刷新 UI
        }

        private void LoadSave(int saveId)
        {
            Debug.Log($"load save: {saveId}");
            SaveManager.Instance.LoadGame(saveId);
            gameObject.SetActive(false);
        }

        private void Save(int saveId, string saveName)
        {
            Debug.Log($"write save: {saveId}");
            SaveManager.Instance.SaveGame(saveId,saveName);
            UpdateSlots(); // 重新刷新 UI
        }

        private void HidePanel()
        {
            UIManager.Instance.ShowGameMenu();
            gameObject.SetActive(false);
        }

        private void DeleteSave(int saveId)
        {
            Debug.Log($"delete save: {saveId}");
            SaveManager.Instance.DeleteSave(saveId);
            UpdateSlots(); // 重新刷新 UI
        }
    }
}