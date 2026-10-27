using BoardItems.Characters;
using BoardItems.SceneObjects;
using UnityEngine;
using System.Collections.Generic;

namespace Yaguar.StoryMaker.Editor
{
    public class SceneObjectsManager : MonoBehaviour
    {
        [field:SerializeField] public List<SceneObject> sceneObjects { private set; get; }
        [SerializeField] public Transform container;

        [SerializeField] Avatar avatar_to_instantiate;
        [SerializeField] Prop object_to_instantiate;
        [SerializeField] WordBalloon[] objectSignal_to_instantiate;
        [SerializeField] Background background_to_instantiate;

        [SerializeField] Transform bgContainer;
        [SerializeField] GameObject bgMask;
        [field:SerializeField] public BackgroundLighting BackgroundLighting {  get; private set; }
        [field: SerializeField] public FontAssets FontAssets{ get; private set; }
        [field: SerializeField] public InputsManager inputsManager { private set; get; }
        [field: SerializeField] public SOBGData bgData { private set; get; }

        public SceneObject selected;

        // True while the scenario is updated from the scenes, so that it is not taken as a user edit
        bool applyingScenes;

        ScenesManager scenesManager => ScenesManager.Instance;

        void Start()
        {
            StoryMakerEvents.ChangeFont += ChangeFont;
            StoryMakerEvents.SetBackgroundLights += SetBackgroundLights;
            StoryMakerEvents.AddAllSceneObjects += AddAllSceneObjects;
            StoryMakerEvents.AddSceneObject += AddNewSceneObject;
            StoryMakerEvents.SetSceneObject += SetSceneObject;
            StoryMakerEvents.RemoveSceneObject += RemoveSceneObject;
            StoryMakerEvents.ReplaceSceneObjectFromTo += ReplaceSceneObjectFromTo;
            StoryMakerEvents.ReplaceSceneObject += ReplaceSceneObject;
            StoryMakerEvents.OnMovieOver += OnMovieOver;
            StoryMakerEvents.OnMoviePaused += OnMovieOver;
            StoryMakerEvents.Restart += ShowBgMask;
            StoryMakerEvents.ClearScene += HideBgMask;
            Events.OnCharacterAnim += OnCharacterAnim;
            Events.OnCharacterExpression += OnCharacterExpression;
        }
        void OnDestroy()
        {
            StoryMakerEvents.ChangeFont -= ChangeFont;
            StoryMakerEvents.SetBackgroundLights -= SetBackgroundLights;
            StoryMakerEvents.AddAllSceneObjects -= AddAllSceneObjects;
            StoryMakerEvents.AddSceneObject -= AddNewSceneObject;
            StoryMakerEvents.SetSceneObject -= SetSceneObject;
            StoryMakerEvents.RemoveSceneObject -= RemoveSceneObject;
            StoryMakerEvents.ReplaceSceneObject -= ReplaceSceneObject;
            StoryMakerEvents.ReplaceSceneObjectFromTo -= ReplaceSceneObjectFromTo;
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
            StoryMakerEvents.OnMoviePaused -= OnMovieOver;
            StoryMakerEvents.Restart -= ShowBgMask;
            StoryMakerEvents.ClearScene -= HideBgMask;
            Events.OnCharacterAnim -= OnCharacterAnim;
            Events.OnCharacterExpression -= OnCharacterExpression;
        }
        private void SetBackgroundLights()
        {
            int value = ScenesManager.Instance.GetActiveScene().lightingValue;
            SpriteRenderer sr = bgMask.GetComponent<SpriteRenderer>();
            Color c = BackgroundLighting.GetStepColor(value);
            sr.color = c;
        }
        void AddAllSceneObjects(List<SOData> elementsToAdd) {
            foreach (SOData data in elementsToAdd)
                AddSceneObject(data);

            foreach(SceneObject so in sceneObjects)
                so.gameObject.SetActive(false);
        }

        // An object added by the user: it is shown and added to the active scene
        void AddNewSceneObject(SOData data) {
            AddSceneObject(data);
            if (data is SOBGData)
                scenesManager.SetActiveBackground(data.id);
            else
                scenesManager.AddToActiveScene(data);
        }

        // Saves into the active scene the edits made to a scene object
        public void Commit(SceneObject so) {
            if (so != null)
                scenesManager.UpdateInActiveScene(so.GetData());
        }

        // Shows a scene object, without changing the scenes
        public void AddSceneObject(SOData data)
        {
            applyingScenes = true;
            selected = null;

            if (data is SOAvatarData avatarData) {
                Avatar avatar = Instantiate(avatar_to_instantiate);
                AddToContainer(avatar, avatarData);

                avatar.gameObject.name = "Avatar_" + avatarData.id + "_" + avatarData.itemName;
                avatar.Init(avatarData);
                inputsManager.ResetAll();
                CharacterManager characterManager = avatar.characterManager;
                characterManager.characterID = avatarData.id;
                Events.LoadBoardItemForStory(characterManager, avatarData.id);
                selected = avatar;

                Events.OnCharacterAnim(avatarData.id, avatarData.anim);
                Events.OnCharacterExpression(avatarData.id, avatarData.emoji);
            } else if (data is SOPropData soData) {
                Prop prop = Instantiate(object_to_instantiate);
                AddToContainer(prop, soData);

                prop.gameObject.name = "Prop_" + soData.id + "_" + soData.itemName;
                prop.Init(soData);
                inputsManager.ResetAll();
                SceneObjectManager objectManager = prop.objectManager;
                objectManager.id = soData.id;
                Events.LoadBoardItemForStory(objectManager, soData.id);
                selected = prop;
            } else if (data is SOBGData soBgData) {
                bgData = soBgData;
                Background bg = Instantiate(background_to_instantiate);
                SetBackground(bg);

                bg.gameObject.name = "BG_" + soBgData.id;
                bg.Init(soBgData);
                inputsManager.ResetAll();
                SceneObjectManager objectManager = bg.objectManager;
                objectManager.id = soBgData.id;
                bgContainer.localScale = new Vector3(2.4f, 2.4f, 2.4f);
                Events.LoadBgForStory(objectManager, soBgData.id, bgContainer, objectManager.GetContainer());
            } else if (data is SOWordBalloonData soWBD) {
                WordBalloon wordBalloon = Instantiate(objectSignal_to_instantiate[0]);
                AddToContainer(wordBalloon, soWBD);

                wordBalloon.gameObject.name = "WordBalloon_" + soWBD.id + "_" + soWBD.itemName;
                wordBalloon.Init(soWBD);
                inputsManager.ResetAll();
                selected = wordBalloon;
            }

            if (selected != null)
                sceneObjects.Add(selected);
            applyingScenes = false;
        }

        // Shows the objects of a scene. previousScene is the one shown until now (null to refresh all):
        // its objects missing in this scene are turned off, and the background is set only if it changes
        public void ShowScene(SceneData scene, SceneData previousScene) {
            if (previousScene != null && sceneObjects.Count > 0) {
                foreach (SceneElement element in previousScene.GetScenesElements()) {
                    if (!scene.Contains(element.data.itemName))
                        TurnOff(element.GetSOData());
                }
            }

            foreach (SceneElement element in scene.GetScenesElements())
                SetSceneObject(element.GetSOData());

            if (!string.IsNullOrEmpty(scene.bgID) && (previousScene == null || previousScene.bgID != scene.bgID))
                AddSceneObject(new SOBGData { id = scene.bgID });
        }

        // Shows a scene object as it is in the active scene
        public void SetSceneObject(SOData data) {

            SceneObject so = GetSceneObjectInScene(data);
            so.gameObject.SetActive(true);
            ApplyData(so, data);
            if (data is SOAvatarData avatarData) {
                applyingScenes = true;
                Events.OnCharacterAnim(so.GetData().id, avatarData.anim);
                Events.OnCharacterExpression(so.GetData().id, avatarData.emoji);
                applyingScenes = false;
            } else if (data is SOWordBalloonData soWBD) {
                WordBalloon wordBalloon = so as WordBalloon;
                wordBalloon.SetField(soWBD.inputValue);
                wordBalloon.SetDirection(soWBD.direction);
                wordBalloon.SetFont(soWBD.fontId);
            }
        }

        void OnCharacterAnim(string characterID, string anim) {
            CommitAvatars(characterID, data => data.anim = anim);
        }

        void OnCharacterExpression(string characterID, string emoji) {
            CommitAvatars(characterID, data => data.emoji = emoji);
        }

        // An action or expression chosen for a character is saved for its avatars in the scenario
        void CommitAvatars(string characterID, System.Action<SOAvatarData> apply) {
            if (applyingScenes)
                return;
            foreach (SceneObject so in sceneObjects) {
                if (so is Avatar && so.gameObject.activeInHierarchy && so.GetData().id == characterID) {
                    apply((SOAvatarData)so.GetData());
                    Commit(so);
                }
            }
        }

        void AddToContainer(SceneObject newSO, SOData data)
        {
            newSO.transform.SetParent(container);
            ApplyData(newSO, data);
        }

        public void ApplyData(SceneObject so, SOData data = null) {
            if (data == null)
                data = so.GetData();
            else {
                SOData sod = so.GetData();
                sod.pos = data.pos;
                sod.rot = data.rot;
                sod.size = data.size;
                sod.goLeft = data.goLeft;
            }

            so.transform.localPosition = data.pos.ToVector3();
            so.transform.localEulerAngles = new Vector3(0, 0, data.rot);
            if (data.size == 0)
                data.size = 1;
            so.transform.localScale = new Vector3(data.size, data.size, data.size);
            so.FlipX(data.goLeft);
        }

        void ChangeFont(int id)
        {
            WordBalloon wordBaloon = selected.GetComponent<WordBalloon>();
            if(wordBaloon != null) {
                wordBaloon.SetFont(id);
                Commit(wordBaloon);
            }
        }
        public void ForceZ(float value)
        {
            selected.ForceZ(value);
            Commit(selected);
        }

        void RemoveSceneObject()
        {
            scenesManager.RemoveFromActiveScene(selected.GetData());
            if (scenesManager.StillExistInOtherScenes(selected.GetData()))
                selected.gameObject.SetActive(false);
            else
                DestroySelected();
        }
        void DestroySelected()
        {
            StoryMakerEvents.HideSoButtons();
            sceneObjects.Remove(selected);
            Destroy(selected.gameObject);
            selected = null;
        }

        void ReplaceSceneObjectFromTo(string original, string duplicated)
        {
            selected = sceneObjects.Find(so => so.GetData().id == original);
            ReplaceSceneObject(duplicated);
        }
        // Replaces the selected character or object with another one. Every instance of it is replaced,
        // in all the keyframes, also the ones added again in other keyframes
        void ReplaceSceneObject(string id) {
            if (!(selected is Avatar || selected is Prop))
                return;

            bool isAvatar = selected is Avatar;
            string oldId = selected.GetData().id;
            List<SceneObject> instances = sceneObjects.FindAll(so => so != null && (isAvatar ? so is Avatar : so is Prop) && so.GetData().id == oldId);
            string itemNamePrefix = Utils.GetUniqueDateTimeId();

            StoryMakerEvents.HideSoButtons();
            SceneObject selectedInstance = selected;
            SceneObject newSelected = null;
            for (int i = 0; i < instances.Count; i++) {
                SceneObject instance = instances[i];
                SOData data = isAvatar ? new SOAvatarData() : new SOPropData();
                data.id = id;
                data.itemName = instances.Count == 1 ? itemNamePrefix : itemNamePrefix + "_" + i;
                scenesManager.ReplaceItem(oldId, instance.GetData().itemName, id, data.itemName);

                sceneObjects.Remove(instance);
                Destroy(instance.gameObject);
                AddSceneObject(data);
                // Hidden until a keyframe that has it is shown
                selected.gameObject.SetActive(false);
                if (instance == selectedInstance)
                    newSelected = selected;
            }
            selected = newSelected;
            StoryMakerEvents.RewindStory();
        }

        public void TurnOff(SOData data)
        {
            SceneObject soToTurnOff = GetSceneObjectInScene(data);
            if (soToTurnOff != null)
                soToTurnOff.gameObject.SetActive(false);
        }

        public Avatar GetAvatarInSceneByItemName(string itemName) {
            foreach (SceneObject a in sceneObjects) {
                if (a is Avatar avatar && a.GetData().itemName == itemName)
                    return avatar;
            }
            return null;
        }

        public SceneObject GetSceneObjectInScene(SOData soData) {
            foreach (SceneObject so in sceneObjects) {
                if (so != null && so.GetData().itemName == soData.itemName)
                    return so;
            }
            return null;
        }

        void SetBackground(SceneObject newSO) {
            Utils.RemoveAllChildsIn(bgContainer);
            newSO.transform.SetParent(bgContainer);
            newSO.transform.localPosition = Vector3.zero;
            newSO.transform.localEulerAngles = Vector3.zero;
            newSO.transform.localScale = Vector3.one;
        }

        public void ResetScene() {
            foreach (SceneObject so in sceneObjects)
                so.gameObject.SetActive(false);
        }

        public void ClearScene() {
            foreach (Transform child in container)
                Destroy(child.gameObject);
            sceneObjects.Clear();

            if (StoryMakerEvents.isEditing)
                gameObject.SetActive(false);
            else
                Utils.RemoveAllChildsIn(bgContainer);
        }

        void OnMovieOver() {
            foreach(SceneObject so in sceneObjects) {
                if (so is Avatar avatar)
                    avatar.characterManager.StopAnims();
            }
        }

        void ShowBgMask() {
            bgMask.SetActive(true);
        }

        void HideBgMask() {
            bgMask.SetActive(false);
        }

        public void TurnOfAllTexts() {
            foreach (SceneObject so in sceneObjects) {
                if (so is WordBalloon)
                   so.gameObject.SetActive(false);
            }
        }
    }
}
