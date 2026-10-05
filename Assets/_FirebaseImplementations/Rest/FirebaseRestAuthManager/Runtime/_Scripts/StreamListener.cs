using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Yaguar.Auth { 
public class StreamListener : MonoBehaviour
{
    private string url;

    public Action<string, string, bool> OnDataReceived;
    public Action<string> OnRemoved;

    public void Init(string firebaseUrlWithAuth) {
        url = firebaseUrlWithAuth;
        StartCoroutine(OpenStream());
    }

    private IEnumerator OpenStream() {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Accept", "text/event-stream");

        var asyncOp = www.SendWebRequest();

        while (!asyncOp.isDone) {
            string chunk = www.downloadHandler.text;
            if (!string.IsNullOrEmpty(chunk)) {
                ProcessChunk(chunk);
            }
            yield return null;
        }
    }

        private void ProcessChunk(string chunk) {
            var lines = chunk.Split('\n');
            string eventType = null;
            string json = null;

            foreach (var line in lines) {
                if (line.StartsWith("event:"))
                    eventType = line.Substring(6).Trim();
                else if (line.StartsWith("data:"))
                    json = line.Substring(5).Trim();
            }

            if (string.IsNullOrEmpty(json) || json == "{}") return;

            FirebaseEvent evt = JsonUtility.FromJson<FirebaseEvent>(json);
            string childId = evt.path.TrimStart('/');

            if (evt.data == null) {
                OnRemoved?.Invoke(childId);
            } else {
                OnDataReceived?.Invoke(childId, evt.data, eventType == "patch");                
            }
        }
    }

[Serializable]
public class FirebaseEvent
{
    public string path;
    public string data; // raw JSON string
    }
}