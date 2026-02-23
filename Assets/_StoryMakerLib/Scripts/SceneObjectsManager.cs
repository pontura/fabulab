using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;


namespace Yaguar.StoryMaker.Editor
{
    public class SceneObjectsManager : MonoBehaviour
    {
        [field:SerializeField] public List<SceneObject> sceneObjects { private set; get; }
        [SerializeField] public Transform container;

        [SerializeField] protected Avatar avatar_to_instantiate;
        [SerializeField] protected Objeto object_to_instantiate;

        [SerializeField] protected ObjectSignal[] objectSignal_to_instantiate;
        [SerializeField] protected ObjectFinalSignal[] finalSignal_to_instantiate;

        [SerializeField] protected SpriteRenderer background;

        [HideInInspector]
        public SceneObject selected;

        [HideInInspector]
        public SceneObject selectedAvatar;

        [HideInInspector]
        public Avatar avatar;
        [field: SerializeField] public InputsManager inputsManager { protected set; get; }

        [field: SerializeField] public SOBGData bgData { protected set; get; }

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
        public void ResetScene()
        {
            foreach(Transform child in container)
            {
                Destroy(child.gameObject);
            }
            //YaguarUtils.RemoveAllChildsIn(container);
            sceneObjects.Clear();
        }
        public virtual void SetAvatarData(Avatar avatar, SOAvatarData data)
        {
           
        }        

        public void DeleteSceneObject(SOData data)
        {
            SceneObject soToRemove = GetSceneObjectInScene(data);
            if (soToRemove == null)
                return;

            sceneObjects.Remove(soToRemove);
            Destroy(soToRemove.gameObject);
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
        }

        protected void AddToContainer(SceneObject newSO, SOData data)
        {
            selected.transform.SetParent(container);
            selected.transform.localPosition = data.pos;
            selected.transform.localEulerAngles = new Vector3(0, 0, data.rot);
            if (data.size == 0)
                data.size = 1;
            selected.transform.localScale = new Vector3(data.size, data.size, data.size);
        }
        public SceneObject GetSceneObjectInScene(SOData soData)
        {
            foreach (SceneObject so in sceneObjects)
            {
                if (so != null &&
                    (so.GetData().id == soData.id && soData is SOAvatarData && so.GetData() is SOAvatarData)
                    ||
                    (
                    //si es un objeto chequea por posicion! quizas haya que mejorar esto...
                    //so.GetData().itemName == soData.itemName && 
                    soData is SOIconData && so.GetData() is SOIconData && so.GetData().pos == soData.pos)
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

        public Avatar GetAvatarInSceneById(int id)
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
