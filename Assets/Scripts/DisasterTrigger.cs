using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DisasterTrigger : MonoBehaviour
{
    [ContextMenu("Trigger Disaster")]
    public void TriggerDisaster()
    {
        StartCoroutine(PostDisaster());
    }

    IEnumerator PostDisaster()
    {
        string url = "http://localhost:8080/start_disaster";
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("managed to start disaster");
        else
            Debug.LogError("Failed to start disaster: " + request.error);
    }
}
