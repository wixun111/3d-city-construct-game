using System;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Entity.Buildings;
using Manager;
using Loader;

public enum BuildingCommandType
{
    Build,
    Demolish,
    Repair
}

public struct CommandResult
{
    public bool success;
    public BuildingCommandType commandType;
    public string buildingName;
    public Vector3Int position;
    public string errorMessage;
}

public class BuildingCommandParser : MonoBehaviour
{
    private static BuildingCommandParser instance;
    public static BuildingCommandParser Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("BuildingCommandParser");
                instance = go.AddComponent<BuildingCommandParser>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    // 建筑类型映射
    private Dictionary<string, int> buildingTypeMap = new()
    {
        {"住宅", 1},
        {"商业", 2},
        {"工业", 3},
        {"政府", 4},
        {"学校", 5},
        {"医院", 6},
        {"公园", 7},
        {"道路", 8},
        {"桥梁", 9},
        {"地铁", 10},
        {"机场", 11},
        {"港口", 12}
    };

    private readonly Regex buildPattern = new Regex(@"build\s+(?:a|an)?\s+(\w+)(?:\s+at\s+\((\d+),\s*(\d+)\))?", RegexOptions.IgnoreCase);
    private readonly Regex demolishPattern = new Regex(@"demolish\s+(?:the\s+)?(?:building\s+)?at\s+\((\d+),\s*(\d+)\)", RegexOptions.IgnoreCase);
    private readonly Regex repairPattern = new Regex(@"repair\s+(?:the\s+)?(?:building\s+)?at\s+\((\d+),\s*(\d+)\)", RegexOptions.IgnoreCase);

    public bool IsBuildingCommand(string message)
    {
        return buildPattern.IsMatch(message) || 
               demolishPattern.IsMatch(message) || 
               repairPattern.IsMatch(message);
    }

    public CommandResult ParseCommand(string command)
    {
        var result = new CommandResult { success = false };

        // 尝试匹配建造命令
        var buildMatch = buildPattern.Match(command);
        if (buildMatch.Success)
        {
            result.commandType = BuildingCommandType.Build;
            result.buildingName = buildMatch.Groups[1].Value;
            
            // 如果指定了位置
            if (buildMatch.Groups[2].Success && buildMatch.Groups[3].Success)
            {
                int x = int.Parse(buildMatch.Groups[2].Value);
                int z = int.Parse(buildMatch.Groups[3].Value);
                result.position = new Vector3Int(x, 0, z);
            }
            else
            {
                // 默认位置
                result.position = new Vector3Int(0, 0, 0);
            }
            
            result.success = true;
            return result;
        }

        // 尝试匹配拆除命令
        var demolishMatch = demolishPattern.Match(command);
        if (demolishMatch.Success)
        {
            result.commandType = BuildingCommandType.Demolish;
            int x = int.Parse(demolishMatch.Groups[1].Value);
            int z = int.Parse(demolishMatch.Groups[2].Value);
            result.position = new Vector3Int(x, 0, z);
            result.success = true;
            return result;
        }

        // 尝试匹配修复命令
        var repairMatch = repairPattern.Match(command);
        if (repairMatch.Success)
        {
            result.commandType = BuildingCommandType.Repair;
            int x = int.Parse(repairMatch.Groups[1].Value);
            int z = int.Parse(repairMatch.Groups[2].Value);
            result.position = new Vector3Int(x, 0, z);
            result.success = true;
            return result;
        }

        result.errorMessage = "Invalid command format. Try 'build a house', 'demolish at (0,0)', or 'repair at (0,0)'.";
        return result;
    }

    public bool ParseAndExecuteCommand(string command)
    {
        // 移除多余的空格
        command = Regex.Replace(command, @"\s+", " ").Trim();

        // 检查是否是建筑命令
        if (command.StartsWith("建造") || command.StartsWith("建设") || command.StartsWith("修建"))
        {
            return ParseBuildingCommand(command);
        }
        else if (command.StartsWith("拆除") || command.StartsWith("移除"))
        {
            return ParseDemolishCommand(command);
        }
        else if (command.StartsWith("修复") || command.StartsWith("修理"))
        {
            return ParseRepairCommand(command);
        }

        return false;
    }

    private bool ParseBuildingCommand(string command)
    {
        // 示例命令格式：
        // "建造住宅在(10,20)"
        // "在(15,25)修建商业建筑"
        // "建设工业区在(30,40)"

        var positionMatch = Regex.Match(command, @"\((\d+),\s*(\d+)\)");
        if (!positionMatch.Success)
        {
            Debug.LogWarning("无法解析位置信息");
            return false;
        }

        int x = int.Parse(positionMatch.Groups[1].Value);
        int z = int.Parse(positionMatch.Groups[2].Value);
        Vector3Int position = new Vector3Int(x, 0, z);

        // 查找建筑类型
        int buildingId = -1;
        foreach (var buildingType in buildingTypeMap)
        {
            if (command.Contains(buildingType.Key))
            {
                buildingId = buildingType.Value;
                break;
            }
        }

        if (buildingId == -1)
        {
            Debug.LogWarning("无法识别建筑类型");
            return false;
        }

        // 检查位置是否可建造
        if (!CityManager.Instance.CurrentCity.CanBuild(BuildingLoader.Instance.BuildingsData[buildingId], position))
        {
            Debug.LogWarning("该位置无法建造建筑");
            return false;
        }

        // 执行建造
        var buildingObject = BuildManager.Instance.SetBuilding(position, Quaternion.identity, buildingId);
        var building = buildingObject.AddComponent<Building>();
        building.InitData(BuildingLoader.Instance.BuildingsData[buildingId], position);
        CityManager.Instance.CurrentCity.AddBuilding(building, position);

        return true;
    }

    private bool ParseDemolishCommand(string command)
    {
        var positionMatch = Regex.Match(command, @"\((\d+),\s*(\d+)\)");
        if (!positionMatch.Success)
        {
            Debug.LogWarning("无法解析位置信息");
            return false;
        }

        int x = int.Parse(positionMatch.Groups[1].Value);
        int z = int.Parse(positionMatch.Groups[2].Value);
        Vector3Int position = new Vector3Int(x, 0, z);

        CityManager.Instance.CurrentCity.Dismantle(position);
        return true;
    }

    private bool ParseRepairCommand(string command)
    {
        var positionMatch = Regex.Match(command, @"\((\d+),\s*(\d+)\)");
        if (!positionMatch.Success)
        {
            Debug.LogWarning("无法解析位置信息");
            return false;
        }

        int x = int.Parse(positionMatch.Groups[1].Value);
        int z = int.Parse(positionMatch.Groups[2].Value);
        Vector3Int position = new Vector3Int(x, 0, z);

        CityManager.Instance.CurrentCity.RepairBuilding(position);
        return true;
    }
} 