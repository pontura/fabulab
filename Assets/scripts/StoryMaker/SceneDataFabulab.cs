using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using BoardItems.BoardData;

namespace Yaguar.StoryMaker.Editor
{
    public enum SceneElementType {
        AVATAR,
        OBJECT
    }

    [Serializable]
    public class SceneElement
    {
        public SOData data;
        public SceneElementType type;

        public override bool Equals(object obj) {
            if (obj is SceneElement other) {
                
                if (this.type != other.type)
                    return false;

                /*if (!string.Equals(this.customizationData, other.customizationData))
                    return false;*/

                if (this.data == null && other.data == null)
                    return true;
                if (this.data == null || other.data == null)
                    return false;

                return this.data.Equals(other.data);
            }
            return false;
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 31 + type.GetHashCode();
            //hash = hash * 31 + (customizationData?.GetHashCode() ?? 0);
            hash = hash * 31 + (data?.GetHashCode() ?? 0);
            return hash;
        }

    }

    [Serializable]
    public class SceneElementAvatar : SceneElement
    {
        public CharacterAnims.anims anim;
        public CharacterExpressions.expressions emoji;
    }

    [Serializable]
    public class SceneDataFabulab : SceneData
    {
        [SerializeField] protected new List<SceneElement> scenesElements;

        public new SceneDataFabulab Clone() {
            SceneDataFabulab nuevo = new SceneDataFabulab();
            nuevo.Init();
            foreach (SceneElement item in scenesElements)
                nuevo.scenesElements.Add(item);
            nuevo.bgID = bgID;
            return nuevo;
        }
        public override void Reset() {
            scenesElements = new List<SceneElement>();
        }
        public void AddSO(SOData soData)
        {
            SceneElement sceneElement = new SceneElement();
            if (soData is SOAvatarFabulabData sOAvatar) {
                Debug.Log("$ is SOAvatarFabulabData");
                sceneElement = new SceneElementAvatar();
                (sceneElement as SceneElementAvatar).anim = sOAvatar.anim;
                (sceneElement as SceneElementAvatar).emoji = sOAvatar.emoji;
                Debug.Log("$ "+sOAvatar.anim.ToString() + " " + sOAvatar.emoji.ToString());
            }
            sceneElement.data = new SOData();
            sceneElement.data.id = soData.id;
            sceneElement.data.pos = soData.pos;
            sceneElement.data.rot = soData.rot;
            sceneElement.data.size = soData.size;
            sceneElement.data.goLeft = soData.goLeft;
            
            /*if (soData is SOAvatarFabulabData avatarFabulabData)
            {
                sceneElement.customizationData = avatarFabulabData.customization;
            }
            else
            {
                
                data += "o" + stringSeparator;
                data += soData.itemName;
                if (soData is SOInputData)
                {
                    (soData as SOInputData).SetIcon();
                    data += "*" + soData.customizationSerialized;
                }
                (soData as SOIconData).SetIcon();
                
            }*/

            if(scenesElements==null)
                scenesElements = new List<SceneElement>();
            
            if(sceneElement is SceneElementAvatar)
                Debug.Log("$ sceneElement is SceneElementAvatar");

            scenesElements.Add(sceneElement);
        }
        
        protected bool IsThisDataDifferentToPreviousFrame(SceneElement data, bool next)
        {
            List<SceneElement> lastAll = GetLastKeyframeAllData(next);
            if (lastAll == null)
                return true;
            foreach (SceneElement s in lastAll)
            {
                if (s.Equals(data))
                    return false;
            }
            return true;
        }

        protected bool HasSameAvatar(SceneElement data, bool next) {
            if(data.type!=SceneElementType.AVATAR)
                return false;

            List<SceneElement> lastAll = GetLastKeyframeAllData(next);
            if (lastAll == null)
                return false;

            foreach (SceneElement s in lastAll) {
                if (s.type==SceneElementType.AVATAR && s.data.id == data.data.id)
                    return true;
            }
            return false;
        }
        protected void DeleteItemsNoLongerExists(SceneElement data)
        {
            if (scenesElements == null)
                return;
            bool itemShowContinue = false;
            foreach (SceneElement s in scenesElements)
            {
                if (s.Equals(data) || (data.type==SceneElementType.AVATAR && s.data.id == data.data.id))
                    itemShowContinue = true;
            }
            if (!itemShowContinue)
                DeleteSO(data);
        }
        protected new List<SceneElement> GetLastKeyframeAllData(bool next)
        {
            int otherSceneID = ScenesManagerFabulab.Instance.currentSceneId - 2;
            if (!next)
                otherSceneID = ScenesManagerFabulab.Instance.currentSceneId;

            if (otherSceneID < 0)
                return null;
            else if (otherSceneID >= ScenesManagerFabulab.Instance.Scenes.Count)
                return null;
            return ScenesManagerFabulab.Instance.Scenes[otherSceneID].GetScenesElements();
        }
        public new void DeleteChangedSO(bool next)
        {
            if (Scenario.Instance && Scenario.Instance.sceneObejctsManager.sceneObjects.Count == 0)
                return;
            List<SceneElement> oldData = GetLastKeyframeAllData(next);
            if (oldData != null)
            {
                foreach (SceneElement data in oldData)
                    DeleteItemsNoLongerExists(data);
            }
        }
        public new void AddSceneObjects(bool next)
        {

            if (scenesElements == null)
                return;

            foreach (SceneElement data in scenesElements)
            {
                bool DataDiferent = IsThisDataDifferentToPreviousFrame(data, next);
                Debug.Log("# DataDiferent: " + DataDiferent);
                if (DataDiferent)
                {
                    if (HasSameAvatar(data, next)) {
                        AvatarFabulab avatar = Scenario.Instance.sceneObejctsManager.GetAvatarInSceneById(data.data.id) as AvatarFabulab;
                        SOAvatarFabulabData sOData = avatar.GetData() as SOAvatarFabulabData;
                        SetSOData(sOData, data);
                        Scenario.Instance.sceneObejctsManager.ApplyData(avatar);
                        
                        Debug.Log("$$ " + (data as SceneElementAvatar).anim.ToString() + " " + (data as SceneElementAvatar).emoji.ToString());
                        Events.OnCharacterAnim(sOData.id, (data as SceneElementAvatar).anim);
                        Events.OnCharacterExpression(sOData.id, (data as SceneElementAvatar).emoji);
                    } else {
                        DeleteSO(data);
                        AddSOToScene(data);
                    }
                }

            }
            SetBG();
            StoryMakerEvents.SceneCompleteLoading();
        }
        protected void DeleteSO(SceneElement data)
        {
            SOData soData = null;
            if (data.type == SceneElementType.AVATAR)
            {
                soData = new SOAvatarFabulabData();
                SetSOData(soData, data);
            }
            else if (data.type == SceneElementType.OBJECT)
            {
                
            }
            Scenario.Instance.sceneObejctsManager.DeleteSceneObject(soData);
            // Events.DeleteSceneObject(soData);
        }
        protected virtual void AddSOToScene(SceneElement data)
        {
            if (data is SceneElementAvatar)
                Debug.Log("$ data is SceneElementAvatar");

            SOData soData = null;
            if (data.type==SceneElementType.AVATAR)
            {
                soData = new SOAvatarFabulabData();

                soData.goLeft = data.data.goLeft;

                (soData as SOAvatarFabulabData).anim = (data as SceneElementAvatar).anim;
                (soData as SOAvatarFabulabData).emoji = (data as SceneElementAvatar).emoji;

                /*if (soData is SOAvatarFabulabData avatarFabulabData)
                    avatarFabulabData.customization = data.customizationData;*/

                //StoryMakerEvents.SetNewAvatarCustomization(data);

                SetSOData(soData, data);
            }
            else if (data.type == SceneElementType.OBJECT) {
                /*soData = new SOIconData();
                SetSOData(soData, arr);
                string itemName = arr[8];
                string[] serializedValueArr = itemName.Split("*"[0]);
                if (serializedValueArr.Length > 1)
                {
                    soData.customizationSerialized = serializedValueArr[1];
                    itemName = serializedValueArr[0];
                }
                soData.itemName = itemName;
                (soData as SOIconData).SetIcon();*/
            }
            
            if (soData != null)
            {
                Debug.Log("# AddSOToScene");
                StoryMakerEvents.AddSceneObject(soData);
                //Scenario.Instance.sceneObejctsManager.AddSceneObject(soData);
            }
        }
        protected new void SetBG()
        {
            /*## IMPLEMENTAR
            SOBGData bgData = new SOBGData();
            bgData.id = bgID;
            StoryMakerEvents.AddSceneObject(bgData);
            */
        }
        protected void SetSOData(SOData soData, SceneElement sceneElement)
        {
            soData.id = sceneElement.data.id;
            soData.pos = sceneElement.data.pos;
            soData.rot = sceneElement.data.rot;
            soData.size = sceneElement.data.size;
            soData.itemName = sceneElement.data.itemName;
        }        

        public override void MakeCharactersWalk(float timeToNextFrame)
        {
            Debug.Log(scenesElements);
            foreach (SceneElement data in scenesElements)
            {
                SOData soAvatarToMove = GetAvatarFromData(data);
                if (soAvatarToMove != null)
                {
                    Vector3 to = CharacterShouldWalkTo(soAvatarToMove, soAvatarToMove.id);
                    if (to != Vector3.zero)
                        Scenario.Instance.movementManager.MoveCharacter(soAvatarToMove.id, to, timeToNextFrame);
                }
            }
        }
        protected new Vector3 CharacterShouldWalkTo(SOData actualData, string avatarID)
        {
            SceneData nextSD = ScenesManagerFabulab.Instance.GetNextActiveScene();
            if (nextSD == null)
                return Vector3.zero;
            List<SceneElement> nextAll = (nextSD as SceneDataFabulab).scenesElements;
            if (nextAll == null)
                return Vector2.zero;
            foreach (SceneElement s in nextAll)
            {
                SOData soData = GetAvatarFromData(s);
                if (soData != null && soData.id == avatarID)
                {
                    if (actualData.size != soData.size || actualData.pos == soData.pos)
                        return Vector3.zero;
                    else {
                        Debug.Log("# CharacterShouldWalkTo: " + soData.pos);
                        return soData.pos;
                    }
                }
            }
            return Vector3.zero;
        }
        protected SOData GetAvatarFromData(SceneElement data)
        {            
            SOData soData = null;
            if (data.type == SceneElementType.AVATAR)
            {
                soData = new SOAvatarFabulabData();
                //soData.customization = data.customizationData;
                soData.goLeft = data.data.goLeft;
                SetSOData(soData, data);
            }
            return soData;
        }

        public new List<SceneElement> GetScenesElements() {
            return scenesElements;
        }
    }
}
