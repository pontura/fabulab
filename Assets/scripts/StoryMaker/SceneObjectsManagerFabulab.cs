using BoardItems.Characters;
using BoardItems.SceneObjects;
using UnityEngine;


namespace Yaguar.StoryMaker.Editor
{
    public class SceneObjectsManagerFabulab : SceneObjectsManager
    {
        [SerializeField] protected Objeto background_to_instantiate;

        [SerializeField] public Transform bgContainer;

        void Start()
        {
            StoryMakerEvents.AddSceneObject += AddSceneObject;
            StoryMakerEvents.DeleteSceneObject += DeleteSceneObject;
            StoryMakerEvents.RemoveSceneObject += RemoveSceneObject;
            SOAvatarData data = new SOAvatarData();
        }
        void OnDestroy()
        {
            StoryMakerEvents.AddSceneObject -= AddSceneObject;
            StoryMakerEvents.DeleteSceneObject -= DeleteSceneObject;
            StoryMakerEvents.RemoveSceneObject -= RemoveSceneObject;
        }
        
        public override void AddSceneObject(SOData data)
        {
            Debug.Log("AddSceneObject: " + data.id + " " + data);
            
            selected = null;

            if (data is SOAvatarFabulabData avatarData)
            {
                SceneObject avatar = Instantiate(avatar_to_instantiate);
                AddToContainer(avatar, avatarData);

                avatar.gameObject.name = "Avatar_" + avatarData.id;
                avatar.Init(avatarData);
                inputsManager.ResetAll();
                CharacterManager characterManager = avatar.GetComponent<AvatarFabulab>().characterManager;
                characterManager.characterID = avatarData.id;
                Events.LoadBoardItemForStory(characterManager, avatarData.id);
                selected = avatar;
                selectedAvatar = avatar;

                if (avatarData.anim == CharacterAnims.anims.edit)
                    avatarData.anim = CharacterAnims.anims.idle;
                Events.OnCharacterAnim(avatarData.id,avatarData.anim);
                Events.OnCharacterExpression(avatarData.id, avatarData.emoji);
            }else if (data is SODataFabulab soData) {
                SceneObject prop = Instantiate(object_to_instantiate);
                AddToContainer(prop, soData);

                prop.gameObject.name = "Prop_" + soData.id;
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
                Events.LoadBoardItemForStory(objectManager, soBgData.id);
            }

            if(selected != null) 
                sceneObjects.Add(selected);
        }

        override protected void RemoveSceneObject()
        {
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
                if (so != null &&
                    (so.GetData().id == soData.id && soData is SOAvatarData && so.GetData() is SOAvatarData)
                    ||
                    (
                    //si es un objeto chequea por posicion! quizas haya que mejorar esto...
                    //so.GetData().itemName == soData.itemName && 
                    soData is SODataFabulab && so.GetData() is SODataFabulab && so.GetData().pos.Equals(soData.pos))
                    )
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
        
        public override void ResetScene() {
            base.ResetScene();
            Utils.RemoveAllChildsIn(bgContainer);
        }
    }
}
