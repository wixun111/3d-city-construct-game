using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class StatePoller : MonoBehaviour
{
    public float pollInterval = 60f;

    void Start()
    {
        StartCoroutine(PollStateLoop());
    }

    IEnumerator PollStateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(pollInterval);
            yield return StartCoroutine(GetStatesFromServer());
        }
    }

    // 这个是实际的请求协程
    IEnumerator GetStatesFromServer()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://localhost:8080/state");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;
            var parsed = JSON.Parse(json);
            var states = parsed["states"];
            bool running = parsed["running"].AsBool;

            Debug.Log($"当前状态: {states} | 是否仍在模拟中: {running}");
        }
        else
        {
            Debug.LogError("❌ 拉取状态失败: " + www.error);
        }
    }

    // 这个函数加了ContextMenu，可以通过Inspector右键菜单触发
    [ContextMenu("手动拉取状态")]
    public void ManualFetch()
    {
        StartCoroutine(GetStatesFromServer());
    }
}
