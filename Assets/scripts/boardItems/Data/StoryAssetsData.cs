using System;
using System.Collections.Generic;

namespace BoardItems.BoardData
{
    // Copies of every character and object (backgrounds and the objects inside other works included) that a
    // story shows, saved with the story in storyAssets/{uid}/{storyId}. The key is the id of the original work:
    // a story keeps showing its copy even if the original changes or is deleted.
    //   characters/{id} = { userID, version, data: CharacterServerData }
    //   so/{id}         = { userID, version, data: SObjectServerData }
    // It is a separate node from stories/{uid}/{storyId} because the app reads every child of that one as a
    // keyframe (older versions of the app would break with anything else there)
    [Serializable]
    public class StoryAssetsData
    {
        public Dictionary<string, StoryCharacterAsset> characters = new Dictionary<string, StoryCharacterAsset>();
        public Dictionary<string, StoryObjectAsset> so = new Dictionary<string, StoryObjectAsset>();
    }

    [Serializable]
    public class StoryCharacterAsset
    {
        // Author of the original (characters/{userID}/{id})
        public string userID;
        // Timestamp of the metadata of the original when it was copied: it only changes when its content is
        // saved. A newer one in the metadata means that the original changed since the copy
        public string version;
        public CharacterServerData data;
    }

    [Serializable]
    public class StoryObjectAsset
    {
        public string userID;
        public string version;
        public SObjectServerData data;
    }
}
