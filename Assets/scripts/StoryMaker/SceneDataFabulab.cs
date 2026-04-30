using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public enum SceneElementType {
        AVATAR,
        PROP,
        WORD_BALLOON,
        WORD_BOX
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

        public virtual SOData GetSOData() {
            SOData soData = new SODataFabulab();
            //soData.customization = data.customizationData;
            soData.goLeft = data.goLeft;
            SetSOData(soData);            
            return soData;
        }

        public virtual void SetSOData(SOData soData) {
            soData.id = data.id;
            soData.pos = data.pos;
            soData.rot = data.rot;
            soData.size = data.size;
            soData.itemName = data.itemName;
        }

    }

    [Serializable]
    public class SceneElementAvatar : SceneElement
    {
        public string anim;
        public string emoji;

        public override bool Equals(object obj) {
            if (obj is SceneElementAvatar other) {

                if (this.type != other.type)
                    return false;

                /*if (!string.Equals(this.customizationData, other.customizationData))
                    return false;*/

                if (this.data == null && other.data == null)
                    return true;
                if (this.data == null || other.data == null)
                    return false;

                return this.data.Equals(other.data)
                    && string.Equals(this.anim, other.anim)
                    && string.Equals(this.emoji, other.emoji);
            }
            return false;
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 31 + type.GetHashCode();
            //hash = hash * 31 + (customizationData?.GetHashCode() ?? 0);
            hash = hash * 31 + (data?.GetHashCode() ?? 0);
            hash = hash * 31 + (anim?.GetHashCode() ?? 0);
            hash = hash * 31 + (emoji?.GetHashCode() ?? 0);
            return hash;
        }

        public override SOData GetSOData() {
            SOData soData = new SOAvatarFabulabData();
            //soData.customization = data.customizationData;
            soData.goLeft = data.goLeft;
            (soData as SOAvatarFabulabData).anim = anim;
            (soData as SOAvatarFabulabData).emoji = emoji;
            SetSOData(soData);
            return soData;
        }
    }

    [Serializable]
    public class SceneElementTextInput : SceneElement
    {
        public string input;

        public override bool Equals(object obj) {
            if (obj is SceneElementTextInput other) {

                if (this.type != other.type)
                    return false;

                /*if (!string.Equals(this.customizationData, other.customizationData))
                    return false;*/

                if (this.data == null && other.data == null)
                    return true;
                if (this.data == null || other.data == null)
                    return false;

                return this.data.Equals(other.data)
                    && string.Equals(this.input, other.input);
            }
            return false;
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 31 + type.GetHashCode();
            //hash = hash * 31 + (customizationData?.GetHashCode() ?? 0);
            hash = hash * 31 + (data?.GetHashCode() ?? 0);
            hash = hash * 31 + (input?.GetHashCode() ?? 0);
            return hash;
        }

        public override SOData GetSOData() {
            Debug.Log("% GetSOData type: " + type);
            SOData soData = null;
            if (type == SceneElementType.WORD_BALLOON) {
                soData = new SOWordBalloonData();                
            } 
            else if (type == SceneElementType.WORD_BOX) {
                soData = new SOWordBalloonData();
            }
            soData.goLeft = data.goLeft;
            SetSOData(soData);
            return soData;            
        }
    }

    [Serializable]
    public class SceneDataFabulab : SceneData
    {
        public bool transition;
        public float duration;
        public string lightingId;
        public int lightingValue;
        [SerializeField] protected new List<SceneElement> scenesElements;

        public override void Init() {
            scenesElements = new List<SceneElement>();
            if (ScenesManagerFabulab.Instance != null) {
                duration = ScenesManagerFabulab.Instance.Keyframe_default_duration;                
                BgLigthtingPalette bglp = (Scenario.Instance.sceneObejctsManager as SceneObjectsManagerFabulab).BackgroundLighting.GetDefaultPalette();
                lightingId = bglp.Id;
                lightingValue = bglp.DefaultStep;
                transition = true;
            }
        }

        public new SceneDataFabulab Clone() {
            SceneDataFabulab nuevo = new SceneDataFabulab();
            nuevo.Init();
            foreach (SceneElement item in scenesElements)
                nuevo.scenesElements.Add(item);
            nuevo.bgID = bgID;
            nuevo.transition = transition;
            nuevo.duration = duration;
            nuevo.lightingId = lightingId;
            nuevo.lightingValue = lightingValue;
            return nuevo;
        }

        public void CopyScenesElements(List<SceneElement> se) { 
            scenesElements = se;
        }
        public override void Reset() {
            scenesElements = new List<SceneElement>();
        }
        public void AddSO(SOData soData)
        {
            if (scenesElements.Any(e => e.GetSOData().itemName == soData.itemName))
                return;

            SceneElement sceneElement = new SceneElement();
            if (soData is SOAvatarFabulabData sOAvatar) {
                Debug.Log("$ is SOAvatarFabulabData");
                sceneElement = new SceneElementAvatar();
                sceneElement.type = SceneElementType.AVATAR;
                (sceneElement as SceneElementAvatar).anim = sOAvatar.anim;
                (sceneElement as SceneElementAvatar).emoji = sOAvatar.emoji;
                Debug.Log("$ anim: " + sOAvatar.anim + " emoji:" + sOAvatar.emoji);
            } else if (soData is SOWordBalloonData soWBD) {
                sceneElement = new SceneElementTextInput();
                sceneElement.type = SceneElementType.WORD_BALLOON;
                (sceneElement as SceneElementTextInput).input = soWBD.inputValue;
            } 
            //else if (soData is SOWordBoxData soWBox) {
            //    sceneElement = new SceneElementTextInput();
            //    sceneElement.type = SceneElementType.WORD_BOX;
            //    (sceneElement as SceneElementTextInput).input = soWBox.inputValue;
            //} 
            else if (soData is SODataFabulab) {
                sceneElement.type = SceneElementType.PROP;
            }
                
            sceneElement.data = new SOData();
            sceneElement.data.id = soData.id;
            sceneElement.data.itemName = soData.itemName;
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
        
        protected bool IsThisDataDifferentToPreviousFrame(SceneElement data, int lastSceneId)
        {
            List<SceneElement> lastAll = GetLastKeyframeAllData(lastSceneId);
            if (lastAll == null)
                return true;
            foreach (SceneElement s in lastAll)
            {
                if (s.Equals(data))
                    return false;
            }
            return true;
        }

        protected bool HasSameAvatar(SceneElement data, int lastSceneId) {
            if(data.type!=SceneElementType.AVATAR)
                return false;

            List<SceneElement> lastAll = GetLastKeyframeAllData(lastSceneId);
            if (lastAll == null)
                return false;

            foreach (SceneElement s in lastAll) {
                if (s.type==SceneElementType.AVATAR && s.data.id == data.data.id)
                    return true;
            }
            return false;
        }
        protected void TurnOffItemsNoExistsInThisScene(SceneElement data)
        {
            if (scenesElements == null)
                return;
            bool itemShowContinue = false;
            foreach (SceneElement s in scenesElements)
            {
                if (s.data.itemName == data.data.itemName)
                    itemShowContinue = true;
            }
            if (!itemShowContinue)
                Scenario.Instance.sceneObejctsManager.TurnOff(data.GetSOData());
        }
        protected new List<SceneElement> GetLastKeyframeAllData(int otherSceneID)
        {
            if (otherSceneID < 1)
                return null;
            else if (otherSceneID > ScenesManagerFabulab.Instance.Scenes.Count)
                return null;
            return ScenesManagerFabulab.Instance.Scenes[otherSceneID-1].GetScenesElements();
        }
        public new void DeleteChangedSO(int lastSceneId)
        {
            //Debug.Log("& DeleteChangedSO");
            if (Scenario.Instance && Scenario.Instance.sceneObejctsManager.sceneObjects.Count == 0)
                return;
            List<SceneElement> oldData = GetLastKeyframeAllData(lastSceneId);
            if (oldData != null)
            {
                foreach (SceneElement data in oldData)
                    TurnOffItemsNoExistsInThisScene(data);
            }
        }
        public new void AddSceneObjects(int lastSceneId)
        {

            if (scenesElements == null)
                return;

            foreach (SceneElement data in scenesElements)
            {
                AddSOToScenenario(data);

                /*bool DataDiferent = IsThisDataDifferentToPreviousFrame(data, lastSceneId);
                Debug.Log("# DataDiferent: " + DataDiferent);
                if (DataDiferent)
                {
                    if (HasSameAvatar(data, lastSceneId)) {
                        AvatarFabulab avatar = Scenario.Instance.sceneObejctsManager.GetAvatarInSceneById(data.data.id) as AvatarFabulab;
                        if (avatar != null) {
                            SOAvatarFabulabData sOData = avatar.GetData() as SOAvatarFabulabData;
                            SetSOData(sOData, data);
                            Scenario.Instance.sceneObejctsManager.ApplyData(avatar);

                            Events.OnCharacterAnim(sOData.id, (data as SceneElementAvatar).anim);
                            Events.OnCharacterExpression(sOData.id, (data as SceneElementAvatar).emoji);
                        }
                    } else {
                        TurnOffSO(data);
                        AddSOToScene(data);
                    }
                }*/

            }

            if (lastSceneId > 0) {
                if (bgID != ScenesManagerFabulab.Instance.Scenes[lastSceneId - 1].bgID)
                        SetBG();
            }else
                SetBG();

            StoryMakerEvents.SceneCompleteLoading();
        }
        
        protected virtual void AddSOToScenenario(SceneElement data)
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

            } else if (data.type == SceneElementType.WORD_BALLOON) {
                soData = new SOWordBalloonData();
                (soData as SOWordBalloonData).inputValue = (data as SceneElementTextInput).input;
            } 
            else if (data.type == SceneElementType.WORD_BOX) {
                soData = new SOWordBalloonData();
                (soData as SOWordBalloonData).inputValue = (data as SceneElementTextInput).input;
            } 
            else if (data.type == SceneElementType.PROP) {
                soData = new SODataFabulab();
            }
            data.SetSOData(soData);
            
            if (soData != null)
            {
                Debug.Log("# AddSOToScenenario");
                StoryMakerEvents.SetSceneObject(soData);
                //Scenario.Instance.sceneObejctsManager.AddSceneObject(soData);
            }
        }
                
        public override void MoveElements(float timeToNextFrame)
        {
            if (!transition)
                return;

            foreach (SceneElement data in scenesElements)
            {
                if (data.type == SceneElementType.AVATAR || data.type == SceneElementType.PROP) {
                    SOData soData = null;
                    soData = GetSOData(data);
                    if (soData != null) {
                        Vector3 to = ElementShouldMoveTo(data.type, soData, soData.itemName);
                        if (to != Vector3.zero) {
                            if (data.type == SceneElementType.AVATAR) {
                                Scenario.Instance.movementManager.MoveCharacter(soData.id, to, timeToNextFrame);
                            } else if (data.type == SceneElementType.PROP) {
                                Scenario.Instance.movementManager.MoveElement(soData, to, timeToNextFrame);
                            }
                        }
                        float scaleTo = ElementShouldScaleTo(data.type, soData, soData.itemName);
                        if (scaleTo != 0)
                            Scenario.Instance.movementManager.ScaleElement(soData, scaleTo, timeToNextFrame);

                        float rotateTo = ElementShouldRotateTo(data.type, soData, soData.itemName);
                        if (rotateTo != 0)
                            Scenario.Instance.movementManager.RotateElement(soData, rotateTo, timeToNextFrame);
                    }
                }
            }
        }       

        protected virtual Vector3 ElementShouldMoveTo(SceneElementType type, SOData actualData, string itemName) {
            SceneData nextSD = ScenesManagerFabulab.Instance.GetNextActiveScene();
            if (nextSD == null)
                return Vector3.zero;
            List<SceneElement> nextAll = (nextSD as SceneDataFabulab).scenesElements;
            if (nextAll == null)
                return Vector2.zero;
            foreach (SceneElement s in nextAll) {
                if (s.type == type) {
                    SOData soData = GetSOData(s);
                    if (soData != null && soData.itemName == itemName) {
                        if (actualData.pos.Equals(soData.pos))
                            return Vector3.zero;
                        else {
                            return soData.pos.ToVector3();
                        }
                    }
                }
            }
            return Vector3.zero;
        }

        protected virtual float ElementShouldScaleTo(SceneElementType type, SOData actualData, string itemName) {
            SceneData nextSD = ScenesManagerFabulab.Instance.GetNextActiveScene();
            if (nextSD == null)
                return 0f;
            List<SceneElement> nextAll = (nextSD as SceneDataFabulab).scenesElements;
            if (nextAll == null)
                return 0f;
            foreach (SceneElement s in nextAll) {
                if (s.type == type) {
                    SOData soData = GetSOData(s);
                    if (soData != null && soData.itemName == itemName) {
                        if (actualData.size.Equals(soData.size))
                            return 0f;
                        else {
                            return soData.size;
                        }
                    }
                }
            }
            return 0f;
        }

        protected virtual float ElementShouldRotateTo(SceneElementType type, SOData actualData, string itemName) {
            SceneData nextSD = ScenesManagerFabulab.Instance.GetNextActiveScene();
            if (nextSD == null)
                return 0f;
            List<SceneElement> nextAll = (nextSD as SceneDataFabulab).scenesElements;
            if (nextAll == null)
                return 0f;
            foreach (SceneElement s in nextAll) {
                if (s.type == type) {
                    SOData soData = GetSOData(s);
                    if (soData != null && soData.itemName == itemName) {
                        if (actualData.rot.Equals(soData.rot))
                            return 0f;
                        else {
                            return soData.rot;
                        }
                    }
                }
            }
            return 0f;
        }

        protected SOData GetSOData(SceneElement data)
        {            
            SOData soData = null;
            if (data.type == SceneElementType.AVATAR)
            {
                soData = new SOAvatarFabulabData();
                //soData.customization = data.customizationData;
                soData.goLeft = data.data.goLeft;
            } else if (data.type == SceneElementType.PROP) {
                soData = new SODataFabulab();
                //soData.customization = data.customizationData;
                soData.goLeft = data.data.goLeft;
            }
            data.SetSOData(soData);
            return soData;
        }

        public new List<SceneElement> GetScenesElements() {
            return scenesElements;
        }
    }
}
