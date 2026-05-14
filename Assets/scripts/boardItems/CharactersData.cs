using BoardItems.BoardData;
using BoardItems.Characters;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UI;
using UnityEditor;
using UnityEngine;
using Yaguar.Auth;
using Yaguar.StoryMaker.DB;
using static BoardItems.Characters.CharacterPartsHelper;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace BoardItems
{
    public class CharactersData : MonoBehaviour {
        [field:SerializeField] public Vector2Int ThumbSize { get; private set; }

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

        string initTimeStamp;

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
        System.Action<bool, string> OnSaved;
        public void SaveCharacter(Texture2D tex, System.Action<bool, string> OnSaved)
        {
            this.OnSaved = OnSaved;
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
            wd.thumb = TextureUtils.GPUScaleTexture(tex, ThumbSize.x, ThumbSize.y);
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
            if (currentID == "")
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

            wd.thumb = TextureUtils.GPUScaleTexture(tex,ThumbSize.x,ThumbSize.y);
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
            string tstamp = DateTime.UtcNow.ToString("o");

            if (currentID == "") // is new character
            {
                userCharactersMetaData.Add(new CharacterMetaData() { id = id, userID = Data.Instance.userData.userDataInDatabase.uid, thumb = currentCharacter.thumb, timestamp = tstamp  });
            }
            else
            {
                CharacterMetaData chmd = userCharactersMetaData.Find(x => x.id == currentID);
                chmd.thumb = currentCharacter.thumb;
                chmd.timestamp = tstamp;
            }

            userCharactersMetaData = userCharactersMetaData.OrderByDescending(x => x.timestamp).ToList();

            currentCharacter.id = id;
            currentID = id;

            ServerCharacterMetaData swmd = new ServerCharacterMetaData();

            swmd.AddCreator(Data.Instance.userData.userDataInDatabase.uid);
            swmd.userID = Data.Instance.userData.userDataInDatabase.uid;
            swmd.timestamp = tstamp;
            FirebaseStoryMakerDBManager.Instance.SaveMetadataToServer(MetadataTypes.characters.ToString(), currentID, swmd);

            FirebaseStoryMakerDBManager.Instance.UploadTexture(currentCharacter.thumb, MetadataTypes.characters.ToString(), currentCharacter.id, Data.Instance.userData.userDataInDatabase.uid);

           // OpenCharacterDetail(currentCharacter);
            OnSaved?.Invoke(succes, id);
        }

        void OnPresetSavedToServer(bool succes, string id, string partId) {
            string tstamp = DateTime.UtcNow.ToString("o");
            loadedPresetId = id;
            if (loadedPresetId == "") {
                Debug.Log("#Error en update preset metadata");
                return;
            }
            ServerPartMetaData swmd = new ServerPartMetaData();
            swmd.thumb = System.Convert.ToBase64String(currentCharacter.thumb.EncodeToPNG());
            swmd.partID = partId;
            swmd.timestamp = tstamp;
            FirebaseStoryMakerDBManager.Instance.SaveBodypartPresetMetadataToServer(id, swmd);

            OpenCharacterDetail(currentCharacter);
        }

        void OpenCharacterDetail(CharacterData wd)
        {
            UIManager.Instance.ShowWorkDetail(wd);            
        }
      
        public void OnLoadCharacterDataFromServer(List<CharacterMetaData> sfds)
        {            
            charactersMetaData = sfds.OrderByDescending(x => x.timestamp).ToList();
            /*int count = 0;
            foreach (CharacterMetaData uc in charactersMetaData) {
                if (count > 0) {
                    FirebaseStoryMakerDBManager.Instance.UploadTexture(uc.thumb, "characters", uc.id, uc.userID, () => {
                        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("metadata/characters/" + uc.id + "/thumb");
                        reference.RemoveValueAsync().ContinueWithOnMainThread(task => {
                            if (task.IsFaulted || task.IsCanceled) {
                                Debug.Log("#Delete Character thumb FAIL");
                                Debug.Log(task.Exception);
                            } else {
                                Debug.Log("#Delete Character thumb metadata/characters/" + uc.id);
                            }
                        });
                    });
                }
                count++;
            }*/
            foreach (CharacterMetaData cmd in charactersMetaData) {
                FirebaseStoryMakerDBManager.Instance.DownloadTexture(MetadataTypes.characters.ToString(), cmd.id, (tex) => {
                    cmd.thumb = tex;
                }, cmd.userID);
            }
            userCharactersMetaData = charactersMetaData.FindAll(x => x.userID == Data.Instance.userData.userDataInDatabase.uid);
            userCharacters = new();
            FirebaseStoryMakerDBManager.Instance.LoadUserAssetsFromServer(MetadataTypes.characters.ToString(), LoadCharactersFromServer);

            initTimeStamp = DateTime.UtcNow.ToString("o");
            var charactersMetadata = FirebaseDatabase.DefaultInstance.GetReference("metadata/characters/");
            charactersMetadata.ChildAdded += OnCharacterAdded;
            charactersMetadata.ChildChanged += OnCharacterChanged;
            charactersMetadata.ChildRemoved += OnCharacterRemoved;
        }

        void OnCharacterAdded(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            DataSnapshot snapshot = args.Snapshot;
            if (snapshot.Exists) {
                if (!snapshot.HasChild("timestamp"))
                    return;

                string timestamp = snapshot.Child("timestamp").Value as string;
                if (timestamp == null || timestamp == "" || String.Compare(timestamp, initTimeStamp) < 0)
                    return;

                Debug.Log("% OnCharacterChanged: " + timestamp + " > " + initTimeStamp);
                CharacterMetaData cmd = charactersMetaData.Find(x => x.id == snapshot.Key);
                if (cmd != null) {
                    SetCharacterChanged(cmd, snapshot);
                } else {

                    cmd = new CharacterMetaData();
                    cmd.id = snapshot.Key;
                    cmd.userID = snapshot.Child("userID").Value as string;
                    cmd.creators = new List<string>();
                    if (snapshot.HasChild("timestamp"))
                        cmd.timestamp = snapshot.Child("timestamp").Value as string;
                    else
                        cmd.timestamp = DateTime.MinValue.ToUniversalTime().ToString("o");

                    if (snapshot.HasChild("creators")) {
                        foreach (var uid in snapshot.Child("creators").Children)
                            cmd.creators.Add(uid.Value as string);
                    }
                    
                    FirebaseStoryMakerDBManager.Instance.DownloadTexture(MetadataTypes.characters.ToString(), cmd.id, (tex) => {
                        cmd.thumb = tex;
                        Events.OnCharacterMetadataAdded(cmd);
                    }, cmd.userID);
                    charactersMetaData.Insert(0, cmd);

                    if (cmd.userID == Data.Instance.userData.userDataInDatabase.uid)
                        userCharactersMetaData.Insert(0, cmd);
                    
                }
            }
        }

        void OnCharacterChanged(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            Debug.Log("% OnCharacterChanged: " + args.Snapshot.GetRawJsonValue());

            DataSnapshot snapshot = args.Snapshot;
            if (snapshot.Exists) {
                CharacterMetaData cmd = charactersMetaData.Find(x => x.id == snapshot.Key);
                if (cmd == null) {
                    return;
                }

                SetCharacterChanged(cmd, snapshot);
                /*if (sfd.userID == Data.Instance.userData.userDataInDatabase.uid)
                    userFilmsData.Insert(0, fd);*/
            }
        }

        void SetCharacterChanged(CharacterMetaData fd, DataSnapshot child) {
            if (child.HasChild("timestamp"))
                fd.timestamp = child.Child("timestamp").Value as string;
            else
                fd.timestamp = DateTime.MinValue.ToUniversalTime().ToString("o");                        
            
            FirebaseStoryMakerDBManager.Instance.DownloadTexture(MetadataTypes.characters.ToString(), fd.id, (tex) => {
                fd.thumb = tex;
                Events.OnCharacterMetadataUpdated(fd);
            }, fd.userID);

            charactersMetaData = charactersMetaData.OrderByDescending(x => x.timestamp).ToList();
            userCharactersMetaData = userCharactersMetaData.OrderByDescending(x => x.timestamp).ToList();
            

        }

        void OnCharacterRemoved(object sender, ChildChangedEventArgs args) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            Debug.Log("% OnCharacterRemoved: " + args.Snapshot.GetRawJsonValue());

            DataSnapshot snapshot = args.Snapshot;
            if (snapshot.Exists) {
                CharacterMetaData fd = charactersMetaData.Find(x => x.id == snapshot.Key);
                if (fd != null) {
                    charactersMetaData.Remove(fd);
                    userCharactersMetaData.Remove(fd);
                    Events.OnCharacterMetadataRemoved(fd.id);
                }
            }
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

        public void SetCurrentID(string id)
        {
            if(id == "")
            {
                currentID = "";
            }
            //CharacterData chd = userCharacters.Find(x => x.id == id);
            //if(chd != null)
                currentID = id;
            //  Debug.Log(currentID);
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
                    OnDone(null);
                    return;
                }

                FirebaseStoryMakerDBManager.Instance.LoadCharacterFromServer(id, (success, key, data) => {
                    if (success) {
                        CharacterData chD = othersCharacters.Find(x => x.id == key);
                        if(chD != null) {
                            Debug.Log("& CharacterData != null");
                            OnDone(chD);
                            return;
                        }
                        Debug.Log("& CharacterData == null");
                        chD = new CharacterData();
                        chD.id = key;
                        chD.LoadServerData(data);
                        chD.thumb = charactersMetaData.Find(x => x.id == chD.id)?.thumb;
                        othersCharacters.Add(chD);
                        OnDone(chD);
                    } else {
                        Debug.Log("Fail getting Character Data");
                        OnDone(null);
                    }

                }, chmd.userID);
                
                Debug.Log("& LoadOthersCharacter " + id);
                
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
        public void Duplicate(string id, System.Action<bool, string> OnDuplicated)
        {
            print("duplicate " + id);
            CharacterMetaData ch = charactersMetaData.Find(x => x.id == id);
            if(ch == null)
            {
                Debug.LogError("No se ha encontrado el character a duplicar");
                OnDuplicated(false, null);
                return;
            }
            userCharactersMetaData.Add(ch);

            FirebaseStoryMakerDBManager.Instance.LoadCharacterFromServer(id, (success, key, data) => {
                if (success)
                {
                    CharacterData chD = othersCharacters.Find(x => x.id == key);
                    if (chD != null)
                    {
                        Debug.Log("& CharacterData != null");
                        return;
                    }
                    Debug.Log("& CharacterData == null");
                    chD = new CharacterData();
                    chD.id = key;
                    chD.LoadServerData(data);
                    chD.thumb = charactersMetaData.Find(x => x.id == chD.id)?.thumb;

                    userCharacters.Add(chD);
                    FirebaseStoryMakerDBManager.Instance.SaveToServer(MetadataTypes.characters.ToString(), chD.GetServerData(), OnDuplicated);
                }
                else
                {
                    Debug.Log("Fail getting Character Data");
                }

            }, null);
        }
    }

}
