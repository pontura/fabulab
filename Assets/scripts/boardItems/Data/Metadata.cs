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