using BoardItems.BoardData;
using BoardItems.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UI;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Windows;
using Yaguar.Auth;
using Yaguar.StoryMaker.DB;
using static BoardItems.Characters.CharacterPartsHelper;

namespace BoardItems
{
    public class CharactersData : MonoBehaviour {
        public Vector2Int thumbSize;

        public List<CharacterData> userCharacters;
        public List<CharacterData> othersCharacters;

        public List<SOPartData> heads;
        public List<SOPartData> bellies;
        public List<SOPartData> hands;
        public List<SOPartData> feet;
        public List<SOPartData> hairs;
        public List<SOPartData> faces;

        public List<CharacterMetaData> charactersMetaData;
        public List<CharacterMetaData> userCharactersMetaData;

        string fieldSeparator = ":";
        string itemSeparator = "&";
        string itemFieldSeparator = "#";

        [SerializeField] string currentID;

        CharacterData currentCharacter;
        Dictionary<string, ServerPartMetaData> serverPartsMetaData;

        [SerializeField] string loadedPresetId;
        public string PresetID { get { return loadedPresetId; } }
       
        private void Start() {
            FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
            Events.OnPresetReset += OnPresetReset;
            Events.OnPresetLoaded += OnPresetLoaded;
            Events.OnCharacterReset += OnCharacterReset;            
        }

       

        private void OnDestroy() {
            FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
            Events.OnPresetReset -= OnPresetReset;
            Events.OnPresetLoaded -= OnPresetLoaded;
            Events.OnCharacterReset -= OnCharacterReset;
        }

        void OnTokenUpdated() {
            Debug.Log("#OnTokenUpdated");
            if (Data.Instance.userData.IsLogged()) {
                CancelInvoke();
                LoadCharacterMetadataFromServer();                
            } else
                Invoke("OnTokenUpdated", 1);
        }
        void LoadCharacterMetadataFromServer() {
            Debug.Log("#LoadUserCharacterMetadataFromServer");
            //if (Data.Instance.userData.IsLogged()) {
                Debug.Log("#is Logged");
                charactersMetaData = new List<CharacterMetaData>();
                FirebaseStoryMakerDBManager.Instance.LoadMetadataFromServer(MetadataTypes.characters.ToString(), OnLoadCharacterDataFromServer);
            //}
        }

        void LoadPartMetadataFromServer() {
            Debug.Log("#LoadPartMetadataFromServer");
            //if (Data.Instance.userData.IsLogged()) {
                FirebaseStoryMakerDBManager.Instance.LoadBodypartPresetMetadataFromServer(OnLoadPresetMetadataFromServer);
            //}
        }

        public void OnLoadPresetMetadataFromServer(Dictionary<string, ServerPartMetaData> spmd) {
            Debug.Log("#OnLoadPresetMetadataFromServer");
            serverPartsMetaData = spmd;
            LoadPresetsFromServer();
        }

        int loadedParts = 0;
        void LoadPresetsFromServer() {
            if (loadedParts + 1 >= BoardItems.Characters.CharacterPartsHelper.GetServerPartsLength()) 
                return;
            loadedParts++;
            string partName = BoardItems.Characters.CharacterPartsHelper.GetServerUniquePartsId(loadedParts);
            //Debug.Log("#partName "+partName + ":  " + loadedParts);
            if (partName == null) {
                LoadPresetsFromServer();
                return;
            }
            FirebaseStoryMakerDBManager.Instance.LoadBodypartPresetsFromServer(loadedParts, OnLoadPresetsFromServer);
        }
        
        void OnLoadPresetsFromServer(int partId, Dictionary<string, SOPartServerData> data) {            
            if (data != null) {
                GetPreset(partId).Clear();
                LoadBodyPartsFromServer(data);
            }
            LoadPresetsFromServer();
        }
        public void SaveCharacter(Texture2D tex)
        {
            CharacterData wd;
            if (currentID == "" || currentID == null)
            {
                wd = new CharacterData();
                wd.id = "";
            }
            else
                wd = GetUserCharacter(currentID);
            CharacterManager cm = UIManager.Instance.boardUI.characterManager;
            wd.bg = Data.Instance.palettesManager.bgColorName;
            wd.armsColor = cm.GetArmsColor();
            wd.legsColor = cm.GetLegsColor();
            wd.eyebrowsColor = cm.GetEyebrowsColor();
            wd.thumb = tex;
            wd.items = new List<SavedIData>();
            foreach (ItemInScene iInScene in UIManager.Instance.boardUI.items.all)
            {
                if (iInScene.data.part == parts.FOOT_LEFT || iInScene.data.part == parts.HAND_LEFT)
                {
                    // los mirrors los ignora:
                } else
                {
                    int newPartID = (int)iInScene.data.part;

                    SavedIData sd = new SavedIData();
                    sd.part = newPartID;
                    sd.id = iInScene.data.id;
                    sd.position = iInScene.data.position;
                    sd.rotation = iInScene.data.rotation;
                    sd.scale = iInScene.data.scale;
                    sd.anim = iInScene.data.anim;
                    sd.color = iInScene.data.colorName;
                    sd.galleryID = iInScene.data.galleryID;
                    wd.items.Add(sd);
                }
            }
            currentCharacter = wd;
            if (wd.id == "")
            {
                userCharacters.Add(wd);
                FirebaseStoryMakerDBManager.Instance.SaveToServer(MetadataTypes.characters.ToString(), wd.GetServerData(), OnCharacterSavedToServer);
            }
            else
            {
                FirebaseStoryMakerDBManager.Instance.UpdateDataToServer(MetadataTypes.characters.ToString(), wd.id, wd.GetServerData(), OnCharacterSavedToServer);
            }
            
        }
        public void SavePartCharacter(Texture2D tex, CharacterPartsHelper.parts part)
        {
            CharacterData wd = new CharacterData();
            CharacterManager cm = UIManager.Instance.boardUI.characterManager;

            wd.thumb = tex;
            wd.items = new List<SavedIData>();
            int partID = 0;
            foreach (ItemInScene iInScene in UIManager.Instance.boardUI.items.all)
            {
                if (iInScene.data.part == part)
                {
                    partID = (int)part;
                    SavedIData sd = new SavedIData();
                    sd.part = partID;
                    sd.id = iInScene.data.id;
                    sd.position = iInScene.data.position;
                    sd.rotation = iInScene.data.rotation;
                    sd.scale = iInScene.data.scale;
                    sd.anim = iInScene.data.anim;
                    sd.color = iInScene.data.colorName;
                    sd.galleryID = iInScene.data.galleryID;
                    wd.items.Add(sd);
                }
            }
            currentCharacter = wd;
            string type = "bodypart";
            print("save part " + partID);
           if (Data.Instance.userData.isAdmin) { // is a part preset;
                if (loadedPresetId == "") {
                    AddPart(partID, wd);
                    FirebaseStoryMakerDBManager.Instance.SaveBodypartPresetToServer((wd as SOPartData).GetServerData(), type, BoardItems.Characters.CharacterPartsHelper.GetServerPartsId(partID), OnPresetSavedToServer);
                } else {
                    FirebaseStoryMakerDBManager.Instance.UpdateBodypartPresetToServer(loadedPresetId, type, (wd as SOPartData).GetServerData(), BoardItems.Characters.CharacterPartsHelper.GetServerPartsId(partID), OnPresetSavedToServer);
                }
            }  
        }
        string SetRealFloat(float f)
        {
            string result = f.ToString("F4", CultureInfo.InvariantCulture);
            return result;
        }
        void OnCharacterSavedToServer(bool succes, string id)
        {
            currentCharacter.id = id;
            currentID = id;

            ServerCharacterMetaData swmd = new ServerCharacterMetaData();

            swmd.AddCreator(Data.Instance.userData.userDataInDatabase.uid);
            swmd.thumb = System.Convert.ToBase64String(currentCharacter.thumb.EncodeToPNG());
            swmd.userID = Data.Instance.userData.userDataInDatabase.uid;
            FirebaseStoryMakerDBManager.Instance.SaveMetadataToServer(MetadataTypes.characters.ToString(), currentID, swmd);

            OpenCharacterDetail(currentCharacter);
        }

        void OnPresetSavedToServer(bool succes, string id, string partId) {            
            loadedPresetId = id;
            if (loadedPresetId == "") {
                Debug.Log("#Error en update preset metadata");
                return;
            }
            ServerPartMetaData swmd = new ServerPartMetaData();
            swmd.thumb = System.Convert.ToBase64String(currentCharacter.thumb.EncodeToPNG());
            swmd.partID = partId;
            FirebaseStoryMakerDBManager.Instance.SaveBodypartPresetMetadataToServer(id, swmd);

            OpenCharacterDetail(currentCharacter);
        }

        void OpenCharacterDetail(CharacterData wd)
        {
            UIManager.Instance.ShowWorkDetail(wd);            
        }
      
        public void OnLoadCharacterDataFromServer(List<CharacterMetaData> sfds)
        {            
            charactersMetaData = sfds;
            userCharactersMetaData = charactersMetaData.FindAll(x => x.userID == Data.Instance.userData.userDataInDatabase.uid);
            userCharacters = new();
            FirebaseStoryMakerDBManager.Instance.LoadUserAssetsFromServer(MetadataTypes.characters.ToString(), LoadCharactersFromServer);
        }

        void LoadCharactersFromServer(Dictionary<string, CharacterServerData> data)
        {
            foreach (KeyValuePair<string, CharacterServerData> e in data)
            {
                //Debug.Log("#LoadCharactersFromServer " + e.Key + ": " + e.Value);
                CharacterData wd = new CharacterData();
                wd.id = e.Key;
                wd.LoadServerData(e.Value);
                wd.thumb = userCharactersMetaData.Find(x => x.id == wd.id)?.thumb;
                userCharacters.Add(wd);
            }
            LoadPartMetadataFromServer();
        }

        void LoadBodyPartsFromServer(Dictionary<string, SOPartServerData> data) {
            foreach (KeyValuePair<string, SOPartServerData> e in data) {
                //Debug.Log("#LoadCharactersFromServer " + e.Key + ": " + e.Value);
                SOPartData wd = new SOPartData();
                wd.id = e.Key;
                wd.LoadServerData(e.Value);
                wd.thumb = new Texture2D(1, 1);
                wd.thumb.LoadImage(System.Convert.FromBase64String(serverPartsMetaData[wd.id].thumb));
                AddPart(wd.items[0].part, wd);
            }
        }
        
        public string GetCurrent()
        {
            return currentID;
        }
        public CharacterData GetUserCharacter(string id)
        {
            return userCharacters.Find(x => x.id == id);
        }

        public CharacterData SetCurrentID(string id)
        {
            CharacterData chd = userCharacters.Find(x => x.id == id);
            if(chd != null)
                currentID = id;
            //  Debug.Log(currentID);
            return chd;
        }

        public void LoadOthersCharacter(string id, System.Action<CharacterData> OnDone) {
            currentID = "";
            //  Debug.Log(currentID);
            if(othersCharacters==null)
                othersCharacters = new List<CharacterData>();
            CharacterData chd = othersCharacters.Find(x => x.id == id);
            if (chd != null)
                OnDone(chd);
            else {
                CharacterMetaData chmd = charactersMetaData.Find(x => x.id == id);
                if (chmd == null) {
                    Debug.Log("Fail getting Character Meta Data");
                    return;
                }

                FirebaseStoryMakerDBManager.Instance.LoadCharacterFromServer(id, (success, key, data) => {
                    if (success) {
                        CharacterData chD = new CharacterData();
                        chD.id = key;
                        chD.LoadServerData(data);
                        chD.thumb = userCharactersMetaData.Find(x => x.id == chD.id)?.thumb;
                        othersCharacters.Add(chD);
                        OnDone(chD);
                    } else {
                        Debug.Log("Fail getting Character Data");
                        return;
                    }

                }, chmd.userID);
                //Debug.Log("#LoadCharactersFromServer " + e.Key + ": " + e.Value);
                
            }

        }

        public void ResetCurrentID()
        {
            currentID = "";
        }

        public void DeleteCharacter(string id)
        {
            //CharacterData wd = characters.Find(x => x.id == id);
            //characters.Remove(wd);
           // PlayerPrefs.DeleteKey("Work_" + id);
            //  PlayerPrefs.DeleteKey("PkpkShared_" + id);
           // PersistWorksIds();
        }


        public bool HasAnims(string id)
        {
            CharacterData wd = userCharacters.Find(x => x.id == id);
            bool hasAnims = false;
            foreach (SavedIData item in wd.items)
            {
                if (item.anim != AnimationsManager.anim.NONE)
                {
                    hasAnims = true;
                    return hasAnims;
                }
            }
            return hasAnims;
        }


        void AddPart(int partID, SOPartData wd)
        {
            switch (partID)
            {
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.HEAD: //head
                    heads.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.BODY: //belly
                    bellies.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.HAND: //hand
                    hands.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.HAND_LEFT: //hand
                    hands.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.FOOT: //foot
                    feet.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.FOOT_LEFT: //foot
                    feet.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.HAIR: //hairs
                    hairs.Add(wd);
                    break;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.FACE: //faces
                    faces.Add(wd);
                    break;
            }
        }
        public List<SOPartData> GetPreset(int partID)
        {
            switch (partID)
            {
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.HEAD: //head
                    return heads;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.BODY: //belly
                    return bellies;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.HAND: //hand
                    return hands;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.HAND_LEFT: //hand
                    return hands;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.FOOT: //foot
                    return feet;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.FOOT_LEFT: //foot
                    return feet;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.HAIR: //hairs
                    return hairs;
                case (int)BoardItems.Characters.CharacterPartsHelper.parts.FACE: //faces
                    return faces;
            }
            return null;
        }
        void OnPresetLoaded(string presetId) {
            Debug.Log("# OnPresetLoaded");
            loadedPresetId = presetId;

            UIManager.Instance.undoManager.OnNewStep();
        }

        void OnCharacterReset() {
            Debug.Log("# OnNewCharacter");
            loadedPresetId = "";
        }
        public void OnPresetReset()
        {
            Debug.Log("# On Preset Reseted");
            loadedPresetId = "";
        }
    }

}
