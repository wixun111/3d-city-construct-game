using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Manager;
using Loader;
using Entity;

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Transform messageContainer;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private ScrollRect scrollRect;

    [Header("API Settings")]
    private static readonly string ZhipuApiKey = "e31ab49773ef4ebf8ef92809b06dacd6.KFDPfxjTRVQsJq7x";
    private static readonly string ZhipuUrl = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

    private HttpClient httpClient;
    private List<(string role, string content)> chatHistory = new List<(string, string)>();
    private bool isProcessing = false;
    private BuildingCommandParser commandParser;

    private void Start()
    {
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ZhipuApiKey}");
        
        sendButton.onClick.AddListener(SendMessage);
        messageInput.onSubmit.AddListener(_ => SendMessage());
        
        // 初始化时隐藏聊天面板
        chatPanel.SetActive(false);

        // 初始化命令解析器
        commandParser = BuildingCommandParser.Instance;
        
        // 添加系统提示，告诉AI如何构建城市
        AddSystemPrompt();
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
    }

    public void ToggleChatPanel()
    {
        chatPanel.SetActive(!chatPanel.activeSelf);
        if (chatPanel.activeSelf)
        {
            messageInput.ActivateInputField();
        }
    }

    private void SendMessage()
    {
        if (string.IsNullOrEmpty(messageInput.text) || isProcessing) return;

        string userMessage = messageInput.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        // 添加用户消息到UI
        AddMessageToUI("User", userMessage);
        messageInput.text = "";

        // 检查是否是建筑命令
        if (commandParser.IsBuildingCommand(userMessage))
        {
            ProcessBuildingCommand(userMessage);
        }
        else
        {
            ProcessChatMessage(userMessage);
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
            AddMessageToUI("System", result.errorMessage);
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
            AddMessageToUI("System", $"Building type '{buildingName}' not found.");
            return;
        }

        // 获取建筑数据
        var buildingData = BuildingLoader.Instance.GetBuildingData(buildingId);
        if (buildingData == null)
        {
            AddMessageToUI("System", "Failed to get building data.");
            return;
        }

        // 检查是否可以建造
        if (!CityManager.Instance.CurrentCity.CanBuild(buildingData, position))
        {
            AddMessageToUI("System", "Cannot build at the specified location.");
            return;
        }

        // 建造建筑
        var buildingObject = BuildManager.Instance.SetBuilding(position, Quaternion.identity, buildingId);
        if (buildingObject != null)
        {
            AddMessageToUI("System", $"Successfully built {buildingName} at {position}.");
        }
        else
        {
            AddMessageToUI("System", "Failed to build the structure.");
        }
    }

    private void DemolishBuilding(Vector3Int position)
    {
        var building = CityManager.Instance.GetBuilding(position);
        if (building != null)
        {
            CityManager.Instance.CurrentCity.Dismantle(position);
            AddMessageToUI("System", $"Successfully demolished building at {position}.");
        }
        else
        {
            AddMessageToUI("System", "No building found at the specified location.");
        }
    }

    private void RepairBuilding(Vector3Int position)
    {
        var building = CityManager.Instance.GetBuilding(position);
        if (building != null)
        {
            CityManager.Instance.CurrentCity.RepairBuilding(position);
            AddMessageToUI("System", $"Successfully repaired building at {position}.");
        }
        else
        {
            AddMessageToUI("System", "No building found at the specified location.");
        }
    }

    private void ProcessChatMessage(string message)
    {
        isProcessing = true;
        AddMessageToUI("AI", "Thinking...");

        chatHistory.Add(("user", message));
        StartCoroutine(SendMessageToZhipu(message));
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

                // 检查响应是否包含建筑命令
                if (commandParser.ParseAndExecuteCommand(reply))
                {
                    AddMessageToUI("System", "命令已执行");
                }
                
                AddMessageToUI("AI", reply);
            }
            else
            {
                Debug.LogError("Zhipu API 调用失败: " + response.Result.StatusCode);
                AddMessageToUI("System", "Sorry, there was an error processing your message.");
            }
        }

        isProcessing = false;
    }

    private void AddMessageToUI(string sender, string message)
    {
        GameObject messageObj = Instantiate(messagePrefab, messageContainer);
        TMP_Text messageText = messageObj.GetComponentInChildren<TMP_Text>();
        messageText.text = $"{sender}: {message}";
        Canvas.ForceUpdateCanvases();
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }

    private void OnDestroy()
    {
        httpClient?.Dispose();
    }
}

[System.Serializable]
public class ZhipuResponse
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