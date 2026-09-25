using System.Collections.Generic;
using UnityEngine;
using Yaguar.Auth;

public static class FabulabAnalytics
{
    public const string WebGLFirebaseId = "15845567895";
    public const string WebGLMeasurementId = "G-63XP5467LY";    
    public const string WebGLApiSecret = "__m_LIE7TMSd21ychzFQpw";

    /// <summary>
    /// Firma igual a FabulabAnalytics.LogEvent
    /// </summary>
    public static void LogEvent(string eventName, Dictionary<string, object> parameters = null) {

        // Asegurar que siempre haya un diccionario
        if (parameters == null)
            parameters = new Dictionary<string, object>();

        // Agregar parámetro platform según plataforma
        string platformValue;
        switch (Application.platform) {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                platformValue = "mobile";
                break;

            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.LinuxPlayer:
                platformValue = "desktop";
                break;


            //case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WebGLPlayer:
                platformValue = "webgl";
                break;

            default:
                platformValue = Application.platform.ToString().ToLower();
                break;
        }

        parameters["platform"] = platformValue;
        //parameters["debug_mode"] = 1;

        // Despachar según plataforma
        switch (Application.platform) {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.LinuxPlayer:
                Firebase.Analytics.FirebaseAnalytics.LogEvent(
                    eventName,
                    ConvertParameters(parameters)
                );
                break;

            //case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WebGLPlayer:
                string userId = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? null;
                //Debug.Log($"## UserId: {userId}");
                GA4Analytics.LogEvent(WebGLFirebaseId, WebGLMeasurementId, WebGLApiSecret, eventName, userId, parameters);
                break;

            default:
                Debug.LogWarning($"Plataforma {Application.platform} no soportada para Analytics");
                break;
        }

        Debug.Log($"## Fabulab Analytics event: {eventName} parameters: {parameters}");
    }

    /// <summary>
    /// Helper para convertir parámetros Dictionary a Firebase.Parameter[]
    /// </summary>
    private static Firebase.Analytics.Parameter[] ConvertParameters(Dictionary<string, object> parameters) {
        if (parameters == null) return new Firebase.Analytics.Parameter[0];

        var list = new List<Firebase.Analytics.Parameter>();
        foreach (var kv in parameters) {
            if (kv.Value is string s)
                list.Add(new Firebase.Analytics.Parameter(kv.Key, s));
            else if (kv.Value is int i)
                list.Add(new Firebase.Analytics.Parameter(kv.Key, i));
            else if (kv.Value is long l)
                list.Add(new Firebase.Analytics.Parameter(kv.Key, l));
            else if (kv.Value is double d)
                list.Add(new Firebase.Analytics.Parameter(kv.Key, d));
            else
                list.Add(new Firebase.Analytics.Parameter(kv.Key, kv.Value.ToString()));
        }
        return list.ToArray();
    }
}
