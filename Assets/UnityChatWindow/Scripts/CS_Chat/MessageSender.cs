using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Manager;
using Loader;
using Entity;
using Entity.Buildings;

public class MessageSender : MonoBehaviour
{
    public string senderName;

    [Header("TextBox Prefabs")]
    public GameObject AuthorTextBox;
    public GameObject PlayerAuthorBox;

    [Header("User Input UI")]
    public GameObject userInputPanel;
    public TMP_InputField userInputField;
    public Button sendButton;

    [Header("Font Settings")]
    public TMP_FontAsset chineseFont;

    public ScrollRect scroll;

    private CustomVerticalLayout layout;
    private string SaveFolder;
    private string SaveFilePath;

    private static readonly string ZhipuApiKey = "e31ab49773ef4ebf8ef92809b06dacd6.KFDPfxjTRVQsJq7x";
    private static readonly string ZhipuUrl = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

    private List<(string role, string content)> chatHistory = new List<(string, string)>();
    private BuildingCommandParser commandParser;

    private void Awake()
    {
        SaveFolder = Path.Combine(Application.persistentDataPath, "data");
        SaveFilePath = Path.Combine(SaveFolder, "zhipu_chat.json");
        commandParser = BuildingCommandParser.Instance;
    }

    private void Start()
    {
        layout = GetComponent<CustomVerticalLayout>();
        LoadChatHistory();
        sendButton.onClick.AddListener(OnSendMessage);
        
        // 添加系统提示
        AddSystemPrompt();

        // 设置输入框字体
        if (chineseFont != null)
        {
            userInputField.fontAsset = chineseFont;
        }
    }

    private void AddSystemPrompt()
    {
        string systemPrompt = @"你是一个城市建造助手。你可以帮助玩家建造和管理城市。
        你可以执行以下命令：
        1. 建造建筑：'建造[建筑类型]在(x,z)'，例如：'建造住宅在(10,20)'
        2. 拆除建筑：'拆除(x,z)'，例如：'拆除(10,20)'
        3. 修复建筑：'修复(x,z)'，例如：'修复(10,20)'

        可用的建筑类型包括：
        - 住宅
        - 商业
        - 工业
        - 政府
        - 学校
        - 医院
        - 公园
        - 道路
        - 桥梁
        - 地铁
        - 机场
        - 港口

        请确保在给出坐标时使用正确的格式，并且坐标在合理范围内。";
        chatHistory.Add(("system", systemPrompt));
        SaveChatHistory();
    }

    public void ShowInputPanel()
    {
        userInputPanel.SetActive(true);
        userInputField.text = "";
        userInputField.ActivateInputField();
    }

    private void OnSendMessage()
    {
        string userText = userInputField.text.Trim();
        if (string.IsNullOrEmpty(userText)) return;

        CreateUserMessage(userText);
        userInputPanel.SetActive(false);

        chatHistory.Add(("user", userText));
        SaveChatHistory();

        // 检查是否是建筑命令
        if (commandParser.IsBuildingCommand(userText))
        {
            ProcessBuildingCommand(userText);
        }
        else
        {
            StartCoroutine(SendMessageToZhipu(userText));
        }
    }

    private void ProcessBuildingCommand(string command)
    {
        var result = commandParser.ParseCommand(command);
        if (result.success)
        {
            switch (result.commandType)
            {
                case BuildingCommandType.Build:
                    BuildBuilding(result.buildingName, result.position);
                    break;
                case BuildingCommandType.Demolish:
                    DemolishBuilding(result.position);
                    break;
                case BuildingCommandType.Repair:
                    RepairBuilding(result.position);
                    break;
            }
        }
        else
        {
            CreateSystemMessage(result.errorMessage);
        }
    }

    private void BuildBuilding(string buildingName, Vector3Int position)
    {
        // 查找建筑ID
        var buildingIds = BuildingLoader.Instance.GetBuildingIds();
        int buildingId = -1;
        
        foreach (var id in buildingIds)
        {
            if (BuildingLoader.Instance.GetBuildingName(id).ToLower() == buildingName.ToLower())
            {
                buildingId = id;
                break;
            }
        }

        if (buildingId == -1)
        {
            CreateSystemMessage($"Building type '{buildingName}' not found.");
            return;
        }

        // 获取建筑数据
        var buildingData = BuildingLoader.Instance.GetBuildingData(buildingId);
        if (buildingData == null)
        {
            CreateSystemMessage("Failed to get building data.");
            return;
        }

        // 检查是否可以建造
        if (!CityManager.Instance.CurrentCity.CanBuild(buildingData, position))
        {
            CreateSystemMessage("Cannot build at the specified location.");
            return;
        }

        // 建造建筑
        var buildingObject = BuildManager.Instance.SetBuilding(position, Quaternion.identity, buildingId);
        if (buildingObject != null)
        {
            // 添加Building组件并初始化数据
            var building = buildingObject.AddComponent<Building>();
            
            // 确保建筑数据包含所有必要字段
            if (!buildingData.ContainsKey("buildingId"))
                buildingData["buildingId"] = buildingId;
            if (!buildingData.ContainsKey("buildingName"))
                buildingData["buildingName"] = buildingName;
            if (!buildingData.ContainsKey("level"))
                buildingData["level"] = 1;
            if (!buildingData.ContainsKey("maxHealth"))
                buildingData["maxHealth"] = 200f;
            if (!buildingData.ContainsKey("currentHealth"))
                buildingData["currentHealth"] = 200f;
            if (!buildingData.ContainsKey("buildable"))
                buildingData["buildable"] = true;
            if (!buildingData.ContainsKey("size"))
                buildingData["size"] = new int[] { 1, 1 };
            
            building.InitData(buildingData, position,0);
            
            // 将建筑添加到城市系统中
            CityManager.Instance.CurrentCity.AddBuilding(building, position);
            
            // 保存游戏状态
            SaveManager.Instance.SaveGame(1, "save1"); // 保存到第一个存档
            
            CreateSystemMessage($"Successfully built {buildingName} at {position}.");
        }
        else
        {
            CreateSystemMessage("Failed to build the structure.");
        }
    }

    private void DemolishBuilding(Vector3Int position)
    {
        var building = CityManager.Instance.GetBuilding(position);
        if (building != null)
        {
            CityManager.Instance.CurrentCity.Dismantle(position);
            CreateSystemMessage($"Successfully demolished building at {position}.");
        }
        else
        {
            CreateSystemMessage("No building found at the specified location.");
        }
    }

    private void RepairBuilding(Vector3Int position)
    {
        var building = CityManager.Instance.GetBuilding(position);
        if (building != null)
        {
            CityManager.Instance.CurrentCity.RepairBuilding(position);
            CreateSystemMessage($"Successfully repaired building at {position}.");
        }
        else
        {
            CreateSystemMessage("No building found at the specified location.");
        }
    }

    private void CreateUserMessage(string content)
    {
        GameObject msg = Instantiate(PlayerAuthorBox, transform);
        msg.transform.localPosition = Vector3.zero;
        var text = msg.GetComponentInChildren<TextMeshProUGUI>();
        text.text = content;
        if (chineseFont != null)
        {
            text.font = chineseFont;
        }

        var box = msg.GetComponent<CustomTextBox>();
        box?.ForceRefreshSize();

        layout.RefreshChildren();
        layout.RefreshAllTextBoxWidths();
        layout.UpdateLayout();
        scroll.verticalNormalizedPosition = 0;
    }

    private void CreateAssistantMessage(string content)
    {
        GameObject msg = Instantiate(AuthorTextBox, transform);
        msg.transform.localPosition = Vector3.zero;
        var text = msg.GetComponentInChildren<TextMeshProUGUI>();
        text.text = content;
        if (chineseFont != null)
        {
            text.font = chineseFont;
        }

        var box = msg.GetComponent<CustomTextBox>();
        box?.ForceRefreshSize();

        layout.RefreshChildren();
        layout.RefreshAllTextBoxWidths();
        layout.UpdateLayout();
        scroll.verticalNormalizedPosition = 0;
    }

    private void CreateSystemMessage(string content)
    {
        GameObject msg = Instantiate(AuthorTextBox, transform);
        msg.transform.localPosition = Vector3.zero;
        var text = msg.GetComponentInChildren<TextMeshProUGUI>();
        text.text = $"System: {content}";
        text.color = Color.yellow;
        if (chineseFont != null)
        {
            text.font = chineseFont;
        }

        var box = msg.GetComponent<CustomTextBox>();
        box?.ForceRefreshSize();

        layout.RefreshChildren();
        layout.RefreshAllTextBoxWidths();
        layout.UpdateLayout();
        scroll.verticalNormalizedPosition = 0;
    }

    private IEnumerator SendMessageToZhipu(string userText)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ZhipuApiKey}");

            var messages = new List<Dictionary<string, string>>();
            foreach (var entry in chatHistory)
            {
                messages.Add(new Dictionary<string, string>
                {
                    { "role", entry.role },
                    { "content", entry.content }
                });
            }

            var requestBody = new
            {
                model = "glm-4",
                messages = messages
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = client.PostAsync(ZhipuUrl, content);
            yield return new WaitUntil(() => response.IsCompleted);

            if (response.Result.IsSuccessStatusCode)
            {
                string jsonResponse = response.Result.Content.ReadAsStringAsync().Result;
                var parsed = JsonConvert.DeserializeObject<ZhipuResponse>(jsonResponse);

                string reply = parsed.choices[0].message.content;
                chatHistory.Add(("assistant", reply));
                SaveChatHistory();

                // 检查响应是否包含建筑命令
                if (commandParser.ParseAndExecuteCommand(reply))
                {
                    CreateSystemMessage("命令已执行");
                }

                CreateAssistantMessage(reply);
            }
            else
            {
                Debug.LogError("Zhipu API 调用失败: " + response.Result.StatusCode);
                CreateSystemMessage("Sorry, there was an error processing your message.");
            }
        }
    }

    private void SaveChatHistory()
    {   
        if (!Directory.Exists(SaveFolder)) Directory.CreateDirectory(SaveFolder);
        string json = JsonConvert.SerializeObject(chatHistory, Formatting.Indented);
        File.WriteAllText(SaveFilePath, json);
    }

    private void LoadChatHistory()
    {
        if (!File.Exists(SaveFilePath)) return;
        string json = File.ReadAllText(SaveFilePath);
        chatHistory = JsonConvert.DeserializeObject<List<(string role, string content)>>(json);
    }

    [System.Serializable]
    private class ZhipuResponse
    {
        public List<Choice> choices;
        [System.Serializable]
        public class Choice
        {
            public Message message;
        }

        [System.Serializable]
        public class Message
        {
            public string role;
            public string content;
        }
    }
}
