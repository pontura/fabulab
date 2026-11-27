using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGames
{
    // An obstacle of the lanes: a character doing an action, or an object (objectId set)
    [Serializable]
    public class MiniGameObstacle
    {
        public string characterId;
        public CharacterAction action = CharacterAction.Stand;
        public string objectId;

        public bool IsObject => !string.IsNullOrEmpty(objectId);
    }

    // What a minigame shows, saved in minigames/{uid}/{id}. The mechanic (ex: runner) is the game that plays it
    // (MiniGame.Id): every minigame is a copy, made by a user, of one of the minigames Fabulab makes of each mechanic
    [Serializable]
    public class MiniGameConfig
    {
        public string mechanic;
        public string backgroundId;
        public string playerCharacterId;
        public List<MiniGameObstacle> obstacles = new List<MiniGameObstacle>();
        public List<string> parallaxObjectIds = new List<string>();
        public string introStoryId;
        public string outroStoryId;
        // Copies of every character and object it shows (and of the objects inside them), by the id of the original:
        // the game shows these, never the originals, so it doesn't change if they are edited or deleted.
        // Made when it is saved (MiniGamesData.Save); a copy, once made, is kept
        public BoardItems.BoardData.StoryAssetsData works;

        // The ids of the characters and objects it shows (the ones with a copy in works)
        public IEnumerable<string> CharacterIds() {
            if (!string.IsNullOrEmpty(playerCharacterId)) yield return playerCharacterId;
            foreach (MiniGameObstacle o in obstacles)
                if (!o.IsObject && !string.IsNullOrEmpty(o.characterId)) yield return o.characterId;
        }

        public IEnumerable<string> ObjectIds() {
            if (!string.IsNullOrEmpty(backgroundId)) yield return backgroundId;
            foreach (string id in parallaxObjectIds)
                if (!string.IsNullOrEmpty(id)) yield return id;
            foreach (MiniGameObstacle o in obstacles)
                if (o.IsObject) yield return o.objectId;
        }

        public MiniGameConfig Clone() {
            MiniGameConfig copy = (MiniGameConfig)MemberwiseClone();
            copy.obstacles = new List<MiniGameObstacle>();
            foreach (MiniGameObstacle o in obstacles ?? new List<MiniGameObstacle>())
                copy.obstacles.Add(new MiniGameObstacle { characterId = o.characterId, action = o.action, objectId = o.objectId });
            copy.parallaxObjectIds = new List<string>(parallaxObjectIds ?? new List<string>());
            return copy;
        }

        // Lists the server leaves out when they are empty
        public void Normalize() {
            obstacles ??= new List<MiniGameObstacle>();
            obstacles.RemoveAll(o => o == null);
            parallaxObjectIds ??= new List<string>();
            parallaxObjectIds.RemoveAll(string.IsNullOrEmpty);
        }
    }

    // metadata/minigames/{id}. The thumbnail is in Storage: images/{userID}/minigames/{id}.jpg
    [Serializable]
    public class MiniGameMetaData
    {
        public string id;
        public string userID;
        public string name;
        public string mechanic;
        public bool isPublic;
        // Changes every time the minigame is saved (the cache of the thumbnail uses it)
        public string timestamp;
        // The minigame this one was copied from (empty: made from scratch by Fabulab)
        public string sourceId;
        // An original of Fabulab, uploaded from the Unity editor (MiniGame: "Subir como minijuego oficial"). The
        // users make their versions of it. Only an admin can set it (database.rules.json)
        public bool isOfficial;
        // The users that made it, the one that modified it last at the end (BoardItems.BoardData.WorkCreators)
        public List<string> creators;
        [NonSerialized] public Texture2D thumb;
    }
}
