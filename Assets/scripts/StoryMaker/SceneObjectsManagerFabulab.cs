using BoardItems.Characters;
using BoardItems.SceneObjects;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace Yaguar.StoryMaker.Editor
{
    public class SceneObjectsManagerFabulab : SceneObjectsManager
    {
        [SerializeField] protected Objeto background_to_instantiate;

        [SerializeField] Transform bgContainer;
        [SerializeField] GameObject bgMask;
        [field:SerializeField] public BackgroundLighting BackgroundLighting {  get; private set; }

        void Start()
        {
            StoryMakerEvents.SetBackgroundLights += SetBackgroundLights;
            StoryMakerEvents.AddAllSceneObjects += AddAllSceneObjects;
            StoryMakerEvents.AddSceneObject += AddSceneObject;
            StoryMakerEvents.SetSceneObject += SetSceneObject;
            StoryMakerEvents.RemoveSceneObject += RemoveSceneObject;
            StoryMakerEvents.OnMovieOver += OnMovieOver;
            StoryMakerEvents.OnMoviePaused += OnMovieOver;
            StoryMakerEvents.Restart += ShowBgMask;
            StoryMakerEvents.ClearScene += HideBgMask;
            SOAvatarData data = new SOAvatarData();
        }
        void OnDestroy()
        {
            StoryMakerEvents.SetBackgroundLights -= SetBackgroundLights;
            StoryMakerEvents.AddAllSceneObjects -= AddAllSceneObjects;
            StoryMakerEvents.AddSceneObject -= AddSceneObject;
            StoryMakerEvents.SetSceneObject -= SetSceneObject;
            StoryMakerEvents.RemoveSceneObject -= RemoveSceneObject;
            StoryMakerEvents.OnMovieOver -= OnMovieOver;
            StoryMakerEvents.OnMoviePaused -= OnMovieOver;
            StoryMakerEvents.Restart -= ShowBgMask;
            StoryMakerEvents.ClearScene -= HideBgMask;
        }
        private void SetBackgroundLights()
        {
            int value = ScenesManagerFabulab.Instance.GetActiveScene().lightingValue;
            print("SetBackgroundLights " + value);
            SpriteRenderer sr = bgMask.GetComponent<SpriteRenderer>();
            Color c = BackgroundLighting.GetStepColor(value);
            sr.color = c;
        }
        protected virtual void AddAllSceneObjects(List<SOData> elementsToAdd) {
            foreach (SOData data in elementsToAdd) {
                Debug.Log(data is SOAvatarFabulabData);
                AddSceneObject(data);
            }

            foreach(SceneObject so in sceneObjects)
                so.gameObject.SetActive(false);
        }

        public override void AddSceneObject(SOData data)
        {
            Debug.Log("AddSceneObject: " + data.id + " " + data);
            
            selected = null;

            if (data is SOAvatarFabulabData avatarData) {
                Debug.Log("& is SOAvatarFabulabData");
                SceneObject avatar = Instantiate(avatar_to_instantiate);
                AddToContainer(avatar, avatarData);

                avatar.gameObject.name = "Avatar_" + avatarData.id + "_" + avatarData.itemName;
                avatar.Init(avatarData);
                inputsManager.ResetAll();
                CharacterManager characterManager = avatar.GetComponent<AvatarFabulab>().characterManager;
                characterManager.characterID = avatarData.id;
                Events.LoadBoardItemForStory(characterManager, avatarData.id);
                selected = avatar;
                selectedAvatar = avatar;

                Debug.Log("& Anim: " + avatarData.anim);
                Debug.Log("& Emoji: " + avatarData.emoji);                

                Events.OnCharacterAnim(avatarData.id, avatarData.anim);
                Events.OnCharacterExpression(avatarData.id, avatarData.emoji);
            } else if (data is SODataFabulab soData) {
                SceneObject prop = Instantiate(object_to_instantiate);
                AddToContainer(prop, soData);

                prop.gameObject.name = "Prop_" + soData.id + "_" + soData.itemName;
                prop.Init(soData);
                inputsManager.ResetAll();
                SceneObjectManager objectManager = prop.GetComponent<Prop>().objectManager;
                objectManager.id = soData.id;
                Events.LoadBoardItemForStory(objectManager, soData.id);
                selected = prop;
            } else if (data is SOBGData soBgData) {
                bgData = soBgData;
                SceneObject bg = Instantiate(background_to_instantiate);
                SetBackground(bg);

                bg.gameObject.name = "BG_" + soBgData.id;
                bg.Init(soBgData);
                inputsManager.ResetAll();
                SceneObjectManager objectManager = bg.GetComponent<Background>().objectManager;
                objectManager.id = soBgData.id;
                bgContainer.localScale = new Vector3(2.4f, 2.4f, 2.4f);
                Events.LoadBgForStory(objectManager, soBgData.id, bgContainer, objectManager.GetContainer());
            } else if (data is SOWordBalloonData soWBD) {
                SceneObject wordBalloon = Instantiate(objectSignal_to_instantiate[0]);
                AddToContainer(wordBalloon, soWBD);

                wordBalloon.gameObject.name = "WordBalloon_" + soWBD.id + "_" + soWBD.itemName;
                wordBalloon.Init(soWBD);
                inputsManager.ResetAll();
                selected = wordBalloon;
            } else if (data is SOWordBoxData soWBoxD) {
                SceneObject wordBox = Instantiate(objectSignal_to_instantiate[1]);
                wordBox.transform.SetParent(container);                
                wordBox.gameObject.name = "WordBox_" + soWBoxD.id + "_" + soWBoxD.itemName;
                wordBox.Init(soWBoxD);
                SetWordBox(wordBox);
                inputsManager.ResetAll();
                selected = wordBox;
            }

            if (selected != null)
                sceneObjects.Add(selected);
        }

        public virtual void SetSceneObject(SOData data) {

            SceneObject so = GetSceneObjectInScene(data);
            ApplyData(so, data);
            so.gameObject.SetActive(true);
            if (data is SOAvatarFabulabData avatarData) {                
                SOAvatarFabulabData sOData = so.GetData() as SOAvatarFabulabData;

                Events.OnCharacterAnim(sOData.id, avatarData.anim);
                Events.OnCharacterExpression(sOData.id, avatarData.emoji);
            }
        }

        override protected void RemoveSceneObject()
        {
            if (ScenesManagerFabulab.Instance.StillExistInOtherScenes(selected.GetData())) {
                selected.gameObject.SetActive(false);
            }else
                base.RemoveSceneObject();
            //UIManager.Instance.mainMenu.CloseAll();
        }
        public override Avatar GetAvatarInSceneById(string id) {
            foreach (SceneObject a in sceneObjects) {
                if (a is AvatarFabulab) {
                    if (a.GetData().id == id)
                        return a as AvatarFabulab;
                }
            }
            return null;
        }

        public override SceneObject GetSceneObjectInScene(SOData soData) {
            foreach (SceneObject so in sceneObjects) {

                /*if (so != null && (so.GetData().id == soData.id && soData is SOAvatarData && so.GetData() is SOAvatarData) ||
                    
                    //si es un objeto chequea por posicion! quizas haya que mejorar esto...
                    //so.GetData().itemName == soData.itemName && 
                    (soData is SODataFabulab && so.GetData() is SODataFabulab && so.GetData().pos.Equals(soData.pos)) ||

                    (soData is SOWordBalloonData && so.GetData() is SOWordBalloonData && so.GetData().pos.Equals(soData.pos))  ||

                    (soData is SOWordBoxData && so.GetData() is SOWordBoxData && soData.id == so.GetData().id)
                    
                )
                    return so;*/

                if (so != null && so.GetData().itemName == soData.itemName)
                    return so;
            }

            return null;
        }

        protected void SetBackground(SceneObject newSO) {
            Utils.RemoveAllChildsIn(bgContainer);
            newSO.transform.SetParent(bgContainer);
            newSO.transform.localPosition = Vector3.zero;
            newSO.transform.localEulerAngles = Vector3.zero;
            newSO.transform.localScale = Vector3.one;
        }
        protected void SetWordBox(SceneObject newSO) {
            float yPos = 0f;
            if(newSO.GetData() is SOWordBoxData soWBD) {
                yPos = soWBD.id == WordBox.boxTypes.top.ToString() ? 41.5f : -8.35f;
            }
            newSO.transform.localPosition = new Vector3(0, yPos, 0);
            newSO.transform.localEulerAngles = Vector3.zero;
            newSO.transform.localScale = Vector3.one*2.7f;
        }

        public override void ClearScene() {
            base.ClearScene();
            Utils.RemoveAllChildsIn(bgContainer);
        }

        void OnMovieOver() {
            //Debug.Log("& OnMovieOver");
            foreach(SceneObject so in sceneObjects) {
                if(so.GetData() is SOAvatarFabulabData)
                    so.GetComponent<AvatarFabulab>().characterManager.StopAnims();
            }
        }

        void ShowBgMask() {
            bgMask.SetActive(true);
        }

        void HideBgMask() {
            bgMask.SetActive(false);
        }
    }
}
