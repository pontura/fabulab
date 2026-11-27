using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoardItems.BoardData
{
    public enum MetadataTypes {
        characters,
        presets,
        so,
        stories
    }

    // The users that made a work (metadata "creators"), in the order they last modified it: the last one is the one
    // that modified it last (the face shown for the work). A work made from scratch has only its author; a work made
    // from another one (a copy of a work of another user, a duplicate) has the creators of that one plus its author
    public static class WorkCreators
    {
        // The creators once uid saved it: the ones it had (or the author, when it has none) with uid at the end
        public static List<string> ModifiedBy(List<string> creators, string uid, string author = null) {
            List<string> result = creators != null && creators.Count > 0
                ? new List<string>(creators)
                : new List<string>();
            if (result.Count == 0 && !string.IsNullOrEmpty(author))
                result.Add(author);
            result.RemoveAll(c => string.IsNullOrEmpty(c) || c == uid);
            if (!string.IsNullOrEmpty(uid))
                result.Add(uid);
            return result;
        }

        // The one that modified it last (fallback: its author, for works without creators)
        public static string Last(List<string> creators, string fallback = null) {
            return creators != null && creators.Count > 0 && !string.IsNullOrEmpty(creators[creators.Count - 1])
                ? creators[creators.Count - 1]
                : fallback;
        }
    }

    [Serializable]
    public class PropMetaData : CharacterMetaData
    {
        public SObjectData.types type;

        public PropMetaData() : base() { }

        public PropMetaData(CharacterMetaData other, string newUserID) : base(other, newUserID) { }
    }

    [Serializable]
    public class ServerPropMetaData : ServerCharacterMetaData
    {
        public SObjectData.types type;   
    }

    [Serializable]
    public class CharacterMetaData
    {
        public string id;
        public Texture2D thumb;
        public string userID;
        public List<string> creators;
        public List<string> tags;
        public string timestamp;
        public bool isPublic;
        public string name;
        public int likes;

        public void SetName(string name)
        {
           this.name = name;
        }
        public void AddCreator(string uid)
        {
            if (creators == null)
            {
                creators = new List<string>();
            }
            if (!creators.Contains(uid))
            {
                creators.Add(uid);
            }
        }
        public void AddTag(string tagID)
        {
            if (tags == null)
            {
                tags = new List<string>();
            }
            if (!tags.Contains(tagID))
            {
                tags.Add(tagID);
            }
        }

        public Sprite GetSprite()
        {
            if (thumb == null)
            {
                Debug.LogError("No hay thumb para character id: " + id);
                return null;
            }
            return Sprite.Create(thumb, new Rect(0, 0, thumb.width, thumb.height), Vector2.zero);
        }

        public CharacterMetaData() { }

        public CharacterMetaData(CharacterMetaData other, string newUserID)
        {
            id = other.id;
            thumb = other.thumb;
            userID = newUserID;
            creators = other.creators;
            tags = other.tags;
            isPublic = other.isPublic;
            name = other.name;
        }
    }

    [Serializable]
    public class ServerCharacterMetaData
    {
        public string userID;
        public List<string> creators;
        public List<string> tags;
        public string timestamp;
        public bool isPublic;
        public string name;
        public void AddCreator(string uid)
        {
            if (creators == null)
            {
                creators = new List<string>();
            }
            if (!creators.Contains(uid))
            {
                if(userID != uid)
                    creators.Add(uid);
            }
        }
        public void AddTag(string tagID)
        {
            if (tags == null)
            {
                tags = new List<string>();
            }
            if (!tags.Contains(tagID))
            {
                tags.Add(tagID);
            }
        }
        public void SetName(string name)
        {
           this.name = name;
        }

    }

    [Serializable]
    public class ServerPartMetaData
    {
        public string partID;
        public List<string> creators;
        public List<string> tags;
        public string timestamp;
        public string name;
    }

}