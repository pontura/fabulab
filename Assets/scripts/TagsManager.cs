using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TagData
{
    public string id;
    public string name;
}

public class TagsManager : MonoBehaviour
{

    [field:SerializeField] public List<TagData> Tags { get; private set; }

    public event Action OnTagsLoaded;


    void Start()
    {
        LoadTags();
    }

    public void AddTag(string newTagName, Action<bool, TagData> callback = null)
    {
        Debug.Log($"#TagsManager Adding Tag '{newTagName}'");
        string normalized = newTagName.Trim().ToLower();
        bool exists = Tags.Exists(t => t.name.Trim().ToLower() == normalized);
        if (exists)
        {
            Debug.LogWarning($"#TagsManager AddTag: tag '{newTagName}' ya existe");
            callback?.Invoke(false, null);
            return;
        }

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("tags");
        string key = reference.Push().Key;
        TagData newTag = new() { id = key, name = newTagName.Trim() };
        string json = JsonConvert.SerializeObject(newTag);

        reference.Child(key).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("#TagsManager AddTag FAIL: " + task.Exception);
                callback?.Invoke(false, null);
                return;
            }

            Tags.Add(newTag);
            Debug.Log($"#TagsManager tag '{newTagName}' creado con id {key}");
            callback?.Invoke(true, newTag);
        });
    }

    public void LoadTags()
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("tags");
        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("#TagsManager LoadTags FAIL: " + task.Exception);
                return;
            }

            Tags.Clear();
            foreach (var child in task.Result.Children)
            {
                string json = child.GetRawJsonValue();
                TagData tag = JsonConvert.DeserializeObject<TagData>(json);
                if (tag != null)
                    Tags.Add(tag);
            }

            Debug.Log($"#TagsManager loaded {Tags.Count} tags");
            OnTagsLoaded?.Invoke();
        });
    }
    public string GetTagID(string tagName)
    {
        TagData tag = Tags.Find(t => t.name.Trim().ToLower() == tagName.Trim().ToLower());
        return tag != null ? tag.id : null;
    }

    public string GetNoTagName() {
        return "Todos";
    }
}
