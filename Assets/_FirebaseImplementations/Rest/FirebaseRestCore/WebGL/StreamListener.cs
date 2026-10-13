using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Yaguar.FirebaseRest
{
    [Serializable]
    public class FirebaseEvent
    {
        public string path;
        public string data; // raw JSON string
    }

    public class StreamListener : MonoBehaviour
    {
        private Coroutine streamCoroutine;
        private UnityWebRequest www;
        private string url;
        private string buffer = "";
        private int lastPos = 0;

        public Action<string, string, bool> OnDataReceived;
        public Action<string> OnRemoved;

        // Umbral en caracteres (~bytes). Ajustá según tu caso.
        private const int MaxBufferSize = 10 * 1024 * 1024; // 10 MB

        public async void Init(string firebaseUrlWithAuth) {
            //Debug.Log("% Init StreamListener");
            string token = await FirebaseRestSession.GetIdTokenAsync();
            url = FirebaseRestConfig.DatabaseUrl + firebaseUrlWithAuth + "?auth="+token;
            streamCoroutine = StartCoroutine(OpenStream());
        }

        private IEnumerator OpenStream() {
            www = UnityWebRequest.Get(url);
            www.SetRequestHeader("Accept", "text/event-stream");

            var asyncOp = www.SendWebRequest();

            while (!asyncOp.isDone) {
                string text = www.downloadHandler.text;

                // Procesar solo lo nuevo
                if (text.Length > lastPos) {
                    string chunk = text.Substring(lastPos);
                    lastPos = text.Length;
                    ProcessIncoming(chunk);
                }

                // Si el buffer acumulado es demasiado grande, reiniciar
                if (text.Length > MaxBufferSize) {
                    Debug.LogWarning("Stream buffer demasiado grande, reiniciando conexión...");
                    RestartStream();
                    yield break;
                }

                yield return null;
            }
        }

        private void ProcessIncoming(string chunk) {
            buffer += chunk;
            //Debug.Log("# "+chunk);
            int index;
            while ((index = buffer.IndexOf("\n\n")) >= 0) {
                string message = buffer.Substring(0, index).Trim();
                buffer = buffer.Substring(index + 2);

                if (!string.IsNullOrEmpty(message)) {
                    ProcessMessage(message);
                }
            }
        }

        private void ProcessMessage(string chunk) {
            //Debug.Log("% " + chunk);
            var lines = chunk.Split('\n');
            string eventType = null;
            string json = null;

            foreach (var line in lines) {
                if (line.StartsWith("event:"))
                    eventType = line.Substring(6).Trim();
                else if (line.StartsWith("data:"))
                    json = line.Substring(5).Trim();
            }
            //Debug.Log("% eventType: " + eventType);
            //Debug.Log("% json: " + json);
            if (string.IsNullOrEmpty(json) || json == "{}" || json == "null") return;
            
            FirebaseEvent evt = JsonUtility.FromJson<FirebaseEvent>(json);

            evt.data = ExtractDataBlock(json);

            //Debug.Log("% evt.path: " + evt.path);
            //Debug.Log("% evt.data: " + evt.data);

            string childId = evt.path.TrimStart('/');

            //Debug.Log("% childId: " + childId);

            if (string.IsNullOrEmpty(childId))
                return; // descartar eventos sin path

            Debug.Log("% " + chunk);

            if (evt.data == null || evt.data == "null") {
                OnRemoved?.Invoke(childId);
            } else if(!string.IsNullOrEmpty(evt.data)) {
                OnDataReceived?.Invoke(childId, evt.data, eventType == "patch");                
            }
        }
        public void CloseStream() {
            OnDataReceived = null;
            OnRemoved = null;

            if (streamCoroutine != null) {
                StopCoroutine(streamCoroutine);
                streamCoroutine = null;
            }

            if (www != null) {
                www.Dispose();
                www = null;
            }
            buffer = "";
            lastPos = 0;
        }

        public void RestartStream() {
            CloseStream();
            streamCoroutine = StartCoroutine(OpenStream());
        }

        private void OnDestroy() {
            CloseStream();
        }

        private string ExtractDataBlock(string json) {
            // Buscar la posición del bloque "data"
            int dataIndex = json.IndexOf("\"data\":");
            if (dataIndex < 0) return null;

            // Cortar desde ahí hasta el final
            string dataPart = json.Substring(dataIndex + 7).Trim();

            int lastBrace = dataPart.LastIndexOf("}");
            if (lastBrace >= 0)
                return dataPart.Substring(0, lastBrace);

            return dataPart;
        }
    }
    
}