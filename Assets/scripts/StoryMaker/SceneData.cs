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
            SOData soData = new SOPropData();
            //soData.customization = data.customizationData;
            soData.goLeft = data.goLeft;
            soData.force_z = data.force_z;
            SetSOData(soData);            
            return soData;
        }

        public virtual void SetSOData(SOData soData) {
            soData.id = data.id;
            soData.pos = data.pos;
            soData.rot = data.rot;
            soData.size = data.size;
            soData.itemName = data.itemName;
            soData.force_z = data.force_z;
            soData.goLeft = data.goLeft;
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
            SOData soData = new SOAvatarData();
            //soData.customization = data.customizationData;
            soData.goLeft = data.goLeft;
            (soData as SOAvatarData).anim = anim;
            (soData as SOAvatarData).emoji = emoji;
            SetSOData(soData);
            return soData;
        }
    }

    [Serializable]
    public class SceneElementTextInput : SceneElement
    {
        public string input;
        public int direction;
        public int fontId;

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
            SOWordBalloonData soData = new SOWordBalloonData();
            soData.inputValue = input;
            soData.direction = direction;
            soData.fontId = fontId;
            SetSOData(soData);
            return soData;
        }
    }

    [Serializable]
    public class SceneData
    {
        public string bgID;
        public bool transition;
        public CamData camData;
        public float duration;
        public string lightingId;
        public int lightingValue;
        [SerializeField] List<SceneElement> scenesElements;

        public void Init() {
            scenesElements = new List<SceneElement>();
            if (ScenesManager.Instance != null) {
                duration = ScenesManager.Instance.Keyframe_default_duration;
                BgLigthtingPalette bglp = Scenario.Instance.sceneObjectsManager.BackgroundLighting.GetDefaultPalette();
                lightingId = bglp.Id;
                lightingValue = bglp.DefaultStep;
                transition = true;
                camData = new CamData();
            }
        }

        public SceneData Clone() {
            SceneData nuevo = new SceneData();
            nuevo.Init();
            foreach (SceneElement item in scenesElements)
                nuevo.scenesElements.Add(item);
            nuevo.bgID = bgID;
            nuevo.transition = transition;
            nuevo.duration = duration;
            nuevo.lightingId = lightingId;
            nuevo.lightingValue = lightingValue;
            nuevo.camData = new CamData();
            nuevo.camData.pos = camData.pos;
            nuevo.camData.zoom = camData.zoom;
            nuevo.camData.tween = camData.tween;
            return nuevo;
        }

        public void CopyScenesElements(List<SceneElement> se) {
            scenesElements = se;
        }
        public void Reset() {
            scenesElements = new List<SceneElement>();
        }
        public List<SceneElement> GetScenesElements() {
            return scenesElements;
        }

        public void AddSO(SOData soData)
        {
            if (scenesElements == null)
                scenesElements = new List<SceneElement>();

            if (scenesElements.Any(e => e.data.itemName == soData.itemName))
                return;

            SceneElement sceneElement;
            if (soData is SOAvatarData sOAvatar) {
                sceneElement = new SceneElementAvatar {
                    type = SceneElementType.AVATAR,
                    anim = sOAvatar.anim,
                    emoji = sOAvatar.emoji
                };
            } else if (soData is SOWordBalloonData soWBD) {
                sceneElement = new SceneElementTextInput {
                    type = SceneElementType.WORD_BALLOON,
                    input = soWBD.inputValue,
                    direction = soWBD.direction,
                    fontId = soWBD.fontId
                };
            } else {
                sceneElement = new SceneElement { type = SceneElementType.PROP };
            }

            sceneElement.data = new SOData();
            sceneElement.data.id = soData.id;
            sceneElement.data.itemName = soData.itemName;
            sceneElement.data.pos = soData.pos;
            sceneElement.data.rot = soData.rot;
            sceneElement.data.size = soData.size;
            sceneElement.data.goLeft = soData.goLeft;
            sceneElement.data.force_z = soData.force_z;

            scenesElements.Add(sceneElement);
        }

        // Turns off the scene objects of the previous keyframe that are not present in this one
        public void DeleteChangedSO(int lastSceneId)
        {
            if (Scenario.Instance && Scenario.Instance.sceneObjectsManager.sceneObjects.Count == 0)
                return;
            List<SceneElement> oldData = GetScenesElementsOf(lastSceneId);
            if (oldData == null || scenesElements == null)
                return;
            foreach (SceneElement data in oldData)
            {
                if (!scenesElements.Any(s => s.data.itemName == data.data.itemName))
                    Scenario.Instance.sceneObjectsManager.TurnOff(data.GetSOData());
            }
        }

        public void AddSceneObjects(int lastSceneId)
        {
            if (scenesElements == null)
                return;

            foreach (SceneElement data in scenesElements)
                StoryMakerEvents.SetSceneObject(data.GetSOData());

            if (lastSceneId > 0) {
                if (bgID != ScenesManager.Instance.Scenes[lastSceneId - 1].bgID)
                    SetBG();
            } else
                SetBG();
        }

        void SetBG()
        {
            if (string.IsNullOrEmpty(bgID))
                return;
            SOBGData bgData = new SOBGData();
            bgData.id = bgID;
            StoryMakerEvents.AddSceneObject(bgData);
        }

        // Tweens avatars and props towards their position, size and rotation in the next keyframe
        public void MoveElements(float timeToNextFrame)
        {
            if (!transition)
                return;

            MovementManager movementManager = Scenario.Instance.movementManager;
            foreach (SceneElement data in scenesElements)
            {
                if (data.type != SceneElementType.AVATAR && data.type != SceneElementType.PROP)
                    continue;

                SOData soData = data.GetSOData();
                SOData nextData = GetInNextScene(data.type, soData.itemName);
                if (nextData == null)
                    continue;

                if (!soData.pos.Equals(nextData.pos)) {
                    if (data.type == SceneElementType.AVATAR)
                        movementManager.MoveCharacter(soData.itemName, nextData.pos.ToVector3(), timeToNextFrame);
                    else
                        movementManager.MoveElement(soData, nextData.pos.ToVector3(), timeToNextFrame);
                }
                if (!soData.size.Equals(nextData.size))
                    movementManager.ScaleElement(soData, nextData.size, timeToNextFrame);

                if (!soData.rot.Equals(nextData.rot))
                    movementManager.RotateElement(soData, nextData.rot, timeToNextFrame);
            }
        }

        SOData GetInNextScene(SceneElementType type, string itemName) {
            SceneData nextSD = ScenesManager.Instance.GetNextActiveScene();
            if (nextSD == null || nextSD.scenesElements == null)
                return null;
            foreach (SceneElement s in nextSD.scenesElements) {
                if (s.type == type && s.data.itemName == itemName)
                    return s.GetSOData();
            }
            return null;
        }

        static List<SceneElement> GetScenesElementsOf(int sceneID)
        {
            if (sceneID < 1 || sceneID > ScenesManager.Instance.Scenes.Count)
                return null;
            return ScenesManager.Instance.Scenes[sceneID - 1].GetScenesElements();
        }
    }
}
