using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Yaguar.Auth
{
    public static class GA4Analytics
    {
        public class ValidationResponse
        {
            public List<ValidationMessage> validationMessages { get; set; }
        }
        public class ValidationMessage
        {
            public string description { get; set; }
            public string fieldPath { get; set; }
        }

        /// <summary>
        /// Imitación de FabulabAnalytics.LogEvent
        /// </summary>
        public static async void LogEvent(string firebaseAppId, string measurementId, string apiSecret, string eventName, string userId = null, Dictionary<string, object> parameters = null, bool debugMode = false) {
            var eventObj = new Dictionary<string, object>
            {
            { "name", eventName },
            { "params", parameters ?? new Dictionary<string, object>() }
        };

            var payload = new Dictionary<string, object>
            {
            { "client_id", System.Guid.NewGuid().ToString() }, // único por sesión
            { "user_properties", new Dictionary<string, object> {
                { "platform", new Dictionary<string, string> { { "value", "webgl" } } }
            }},
            { "events", new List<object> { eventObj } }
        };

            if (!string.IsNullOrEmpty(userId)) {
                payload["user_id"] = userId;
            }

            string json = JsonConvert.SerializeObject(payload);

            string endpoint = debugMode
            ? "https://www.google-analytics.com/debug/mp/collect"
            : "https://www.google-analytics.com/mp/collect";

            string url = $"{endpoint}?firebase_app_id={firebaseAppId}&measurement_id={measurementId}&api_secret={apiSecret}";

            using (UnityWebRequest www = new UnityWebRequest(url, "POST")) {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                var asyncOp = www.SendWebRequest();
                while (!asyncOp.isDone)
                    await Task.Yield();

                if (www.result != UnityWebRequest.Result.Success) {
                    Debug.LogError("GA4 LogEvent error: " + www.error);
                } else {
                    if (debugMode) {
                        string responseText = www.downloadHandler.text;
                        Debug.Log("##Respuesta de validación: " + responseText);

                        try {
                            var validation = JsonConvert.DeserializeObject<ValidationResponse>(responseText);
                            if (validation?.validationMessages != null && validation.validationMessages.Count > 0) {
                                foreach (var msg in validation.validationMessages) {
                                    Debug.LogWarning($"##GA4 Validation: {msg.description} (field: {msg.fieldPath})");
                                }
                            } else {
                                Debug.Log("##GA4 Validation: sin errores, evento válido.");
                            }
                        } catch (System.Exception ex) {
                            Debug.LogError("Error parseando respuesta de validación: " + ex.Message);
                        }
                    } else {
                        Debug.Log($"##GA4 LogEvent enviado: {eventName}");
                    }
                }
            }
        }
    }
}