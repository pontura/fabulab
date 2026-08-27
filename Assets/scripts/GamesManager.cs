using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class GameData
{
    public string id;
    public string section;
    public string title;
    public string description;
    [JsonIgnore] public Sprite thumbnail;
    public string[] ids;
}

public static class GameSection
{
    public const string Stories = "stories";
    public const string Object = "object";
    public const string Character = "character";
    public const string Bg = "bg";
}

[Serializable]
public class GameThumbnail
{
    public string id;
    public Sprite thumbnail;
}

public class GamesManager : MonoBehaviour
{
    public bool done {  get; private set; }
    [field:SerializeField] public List<GameData> Games { get; private set; }
    [SerializeField] private List<GameThumbnail> thumbnails;

    public event Action OnGamesLoaded;


    void Start()
    {
        LoadGames();
    }

    public void AddGame(string newGameTitle, string newGameSection, string newGameDescription, Action<bool, GameData> callback = null)
    {
        Debug.Log($"#GamesManager Adding Game '{newGameTitle}'");
        string normalized = newGameTitle.Trim().ToLower();
        bool exists = Games.Exists(g => g.title.Trim().ToLower() == normalized);
        if (exists)
        {
            Debug.LogWarning($"#GamesManager AddGame: Game '{newGameTitle}' ya existe");
            callback?.Invoke(false, null);
            return;
        }

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("games");
        string key = reference.Push().Key;
        GameData newGame = new() { id = key, section = newGameSection, title = newGameTitle.Trim(), description = newGameDescription?.Trim() };
        string json = JsonConvert.SerializeObject(newGame);

        reference.Child(key).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("#GamesManager AddGame FAIL: " + task.Exception);
                callback?.Invoke(false, null);
                return;
            }

            Games.Add(newGame);
            Debug.Log($"#GamesManager Game '{newGameTitle}' creado con id {key}");
            callback?.Invoke(true, newGame);
        });
    }

    public void LoadGames()
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("games");
        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("#GamesManager LoadGames FAIL: " + task.Exception);
                return;
            }

            Games.Clear();
            foreach (var child in task.Result.Children)
            {
                string json = child.GetRawJsonValue();
                GameData game = JsonConvert.DeserializeObject<GameData>(json);
                if (game != null)
                {
                    game.thumbnail = thumbnails.Find(t => t.id == game.id)?.thumbnail;
                    Games.Add(game);
                }
            }

            Debug.Log($"#GamesManager loaded {Games.Count} Games");
            OnGamesLoaded?.Invoke();
            done = true;
        });
    }

    public string GetGameID(string gameTitle)
    {
        GameData game = Games.Find(g => g.title.Trim().ToLower() == gameTitle.Trim().ToLower());
        return game != null ? game.id : null;
    }

    public GameData GetGame(string gameId)
    {
        return Games.Find(g => g.id == gameId);
    }

    public List<GameData> GetGamesBySection(string section)
    {
        return Games.FindAll(g => g.section == section);
    }
}
