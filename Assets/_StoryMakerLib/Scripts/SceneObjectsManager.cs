using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class SceneObjectsManager : MonoBehaviour
    {
        [field:SerializeField] public List<SceneObject> sceneObjects { private set; get; }
        [SerializeField] public Transform container;

        [SerializeField] protected Avatar avatar_to_instantiate;
        [SerializeField] protected Objeto object_to_instantiate;

        [SerializeField] protected Objeto[] objectSignal_to_instantiate;
        [SerializeField] protected Objeto[] finalSignal_to_instantiate;

        [SerializeField] protected SpriteRenderer background;

        //[HideInInspector]
        public SceneObject selected;

        [HideInInspector]
        public SceneObject selectedAvatar;

        [HideInInspector]
        public Avatar avatar;
        [field: SerializeField] public InputsManager inputsManager { protected set; get; }

        [field: SerializeField] public SOBGData bgData { protected set; get; }

        void Start()
        {
            StoryMakerEvents.SetSceneObject += AddSceneObject;
            StoryMakerEvents.RemoveSceneObject += RemoveSceneObject;
            SOAvatarData data = new SOAvatarData();
        }
        void OnDestroy()
        {
            StoryMakerEvents.SetSceneObject -= AddSceneObject;
            StoryMakerEvents.RemoveSceneObject -= RemoveSceneObject;
        }
        public virtual void ClearScene()
        {
            foreach(Transform child in container)
            {
                Destroy(child.gameObject);
            }
            sceneObjects.Clear();
        }

        public virtual void ResetScene() {
            foreach (SceneObject si in sceneObjects) {
                si.gameObject.SetActive(false);
            }
        }

        public virtual void SetAvatarData(Avatar avatar, SOAvatarData data)
        {
           
        }        

        public void TurnOff(SOData data)
        {
            SceneObject soToTurnOff = GetSceneObjectInScene(data);
            Debug.Log("$$$ soToRemove " + (soToTurnOff == null));
            if (soToTurnOff == null)
                return;

            soToTurnOff.gameObject.SetActive(false);
            /*sceneObjects.Remove(soToTurnOff);
            Destroy(soToTurnOff.gameObject);*/
        }
        public virtual void AddSceneObject(SOData data)
        {
            Debug.Log("# AddSceneObject");
            if (data is SOAvatarData)
            {
                
            }
            else if (data is SOIconData)
            {
                
            }
            else if (data is SOBGData)
            {
                /*bgData = Data.Instance.items.GetBackground(data.id);
                background.sprite = bgData.icon;*/
                return;
            }
            sceneObjects.Add(selected);
        }

        protected virtual void RemoveSceneObject()
        {
            StoryMakerEvents.HideSoButtons();
            sceneObjects.Remove(selected);
            Destroy(selected.gameObject);
            selected = null;
        }

        protected void AddToContainer(SceneObject newSO, SOData data)
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
            }

            so.transform.localPosition = data.pos.ToVector3();
            so.transform.localEulerAngles = new Vector3(0, 0, data.rot);
            if (data.size == 0)
                data.size = 1;
            so.transform.localScale = new Vector3(data.size, data.size, data.size);
        }

        public virtual SceneObject GetSceneObjectInScene(SOData soData)
        {
            foreach (SceneObject so in sceneObjects)
            {
                if (so != null &&
                    (so.GetData().id == soData.id && soData is SOAvatarData && so.GetData() is SOAvatarData)
                    ||
                    (
                    //si es un objeto chequea por posicion! quizas haya que mejorar esto...
                    //so.GetData().itemName == soData.itemName && 
                    soData is SOIconData && so.GetData() is SOIconData && so.GetData().pos.Equals(soData.pos))
                    )
                    return so;
            }

            return null;
        }
        public List<SOAvatarData> GetAvatarsInScene()
        {
            List<SOAvatarData> avatars = new List<SOAvatarData>();
            foreach (SceneObject so in sceneObjects)
            {
                if (so.GetData() is SOAvatarData)
                    avatars.Add(so.GetData() as SOAvatarData);
            }
            return avatars;
        }

        public virtual Avatar GetAvatarInSceneById(string id)
        {
            foreach (SceneObject a in sceneObjects)
            {
                if (a is Avatar)
                {
                    if (a.GetData().id == id)
                        return a as Avatar;
                }
            }
            return null;
        }
    }
}
