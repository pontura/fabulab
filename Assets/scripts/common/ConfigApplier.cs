using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class FieldEntry
{
    public string field;
    public string value;
}

[System.Serializable]
public class ComponentEntry
{
    public string name;
    public string platform;
    public FieldEntry[] fields;
}

[System.Serializable]
public class PlatformConfig
{
    public ComponentEntry[] components;
}

public class ConfigApplier : MonoBehaviour
{
    [SerializeField] private TextAsset configJson;

    void Awake() {
        if (configJson == null) return;

        PlatformConfig config = JsonUtility.FromJson<PlatformConfig>(configJson.text);

        foreach (var compEntry in config.components) {

#if UNITY_EDITOR
            if (compEntry.platform != "editor")
                continue;
#elif UNITY_ANDROID
            if (compEntry.platform != "android")
                continue;
#elif UNITY_IOS
            if (compEntry.platform != "ios")
                continue;
#elif UNITY_WEBGL
            if (compEntry.platform != "webgl")
                continue;
#elif UNITY_STANDALONE_WIN
            if (compEntry.platform != "win")
                continue;
#elif UNITY_STANDALONE_OSX
            if (compEntry.platform != "osx")
                continue;
#elif UNITY_STANDALONE_LINUX
            if (compEntry.platform != "linux")
                continue;
#else
            continue;
#endif

            Component comp = GetComponent(compEntry.name);
            if (comp == null) {
                Debug.LogWarning($"No se encontró el componente {compEntry.name} en {gameObject.name}");
                continue;
            }

            foreach (var fieldEntry in compEntry.fields) {
                FieldInfo field = comp.GetType().GetField(fieldEntry.field,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null) {
                    object converted = Convert.ChangeType(fieldEntry.value, field.FieldType);
                    field.SetValue(comp, converted);
                    Debug.Log($"Campo {fieldEntry.field} de {compEntry.name} sobreescrito con {fieldEntry.value}");
                } else {
                    Debug.LogWarning($"No se encontró el campo {fieldEntry.field} en {compEntry.name}");
                }
            }
        }
    }    
}
