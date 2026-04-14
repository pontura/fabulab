using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

namespace Yaguar.StoryMaker.Editor
{
    [Serializable]
    public class SceneData
    {
        public string bgID;
        protected List<string> scenesElements;
        string stringSeparator = "|";
        string stringCustomizerSeparator = "*";
        string serialSeparator = "#";
        string soSeparator = "&";

        public virtual void Init()
        {
            scenesElements = new List<string>();
        }

        public virtual SceneData Clone()
        {
            SceneData nuevo = new SceneData();
            nuevo.Init();
            foreach (string item in scenesElements)
                nuevo.scenesElements.Add(item);
            nuevo.bgID = bgID;
            return nuevo;
        }

        public virtual void Reset()
        {
            scenesElements = new List<string>();
        }
        public virtual void AddSO(SOData soData, string customizerData)
        {
            string data = "";
            data += soData.id + stringSeparator;
            data += soData.pos.x.ToString(CultureInfo.InvariantCulture) + stringSeparator;
            data += soData.pos.y.ToString(CultureInfo.InvariantCulture) + stringSeparator;
            data += soData.pos.z.ToString(CultureInfo.InvariantCulture) + stringSeparator;
            data += soData.rot.ToString(CultureInfo.InvariantCulture) + stringSeparator;
            data += soData.size.ToString(CultureInfo.InvariantCulture) + stringSeparator;
            data += soData.goLeft + stringSeparator;
            if (soData is SOAvatarData)
            {
                data += "a" + stringSeparator;
                data += customizerData;
            }
            else
            {
                data += "o" + stringSeparator;
                data += soData.itemName;
                if (soData is SOInputData)
                {
                    //(soData as SOInputData).SetIcon();
                    data += "*" + soData.customization;
                }
                if (soData is SOIconData)
                    (soData as SOIconData).SetIcon();
            }
            scenesElements.Add(data);
        }
        public void EditSO(SOData soData)
        {
        }
        protected bool IsThisDataDifferentToPreviousFrame(string data, int lastSceneId)
        {
            List<string> lastAll = GetLastKeyframeAllData(lastSceneId);
            if (lastAll == null)
                return true;
            foreach (string s in lastAll)
            {
                if (s == data)
                    return false;
            }
            return true;
        }
        protected void DeleteItemsNoLongerExists(string data)
        {
            if (scenesElements == null)
                return;
            bool itemShowContinue = false;
            foreach (string s in scenesElements)
            {
                if (s == data)
                    itemShowContinue = true;
            }
            if (!itemShowContinue)
                DeleteSO(data);
        }
        protected List<string> GetLastKeyframeAllData(int otherSceneID)
        {            

            if (otherSceneID < 1)
                return null;
            else if (otherSceneID >= ScenesManager.Instance.Scenes.Count)
                return null;
            return ScenesManager.Instance.Scenes[otherSceneID-1].scenesElements;
        }
        public void DeleteChangedSO(int lastSceneId)
        {
            if (Scenario.Instance && Scenario.Instance.sceneObejctsManager.sceneObjects.Count == 0)
                return;
            List<string> oldData = GetLastKeyframeAllData(lastSceneId);
            if (oldData != null)
            {
                foreach (string data in oldData)
                    DeleteItemsNoLongerExists(data);
            }
        }
        public void AddSceneObjects(int lastSceneId)
        {

            if (scenesElements == null)
                return;

            foreach (string data in scenesElements)
            {
                bool DataDiferent = IsThisDataDifferentToPreviousFrame(data, lastSceneId);
                if (DataDiferent)
                {
                    DeleteSO(data);
                    AddSO(data);
                }

            }
            SetBG();
            StoryMakerEvents.SceneCompleteLoading();
        }
        protected void DeleteSO(string data)
        {
            SOData soData = null;
            string[] arr = data.Split(stringSeparator[0]);
            if (arr[7] == "a")
            {
                soData = new SOAvatarData();
                SetSOData(soData, arr);
            }
            else if (arr[7] == "o")
            {
                soData = new SOIconData();
                SetSOData(soData, arr);
            }
            Scenario.Instance.sceneObejctsManager.DeleteSceneObject(soData);
            // Events.DeleteSceneObject(soData);
        }
        protected virtual void AddSO(string data)
        {
            string[] arr = data.Split(stringSeparator[0]);
            SOData soData = null;
            if (arr[7] == "a")
            {
                soData = new SOAvatarData();

                soData.goLeft = false;
                if (arr[6].ToString() != "False")
                    soData.goLeft = true;

                SetCustomizerBySerialization(soData, arr[8]);
                SetSOData(soData, arr);
            }
            else if (arr[7] == "o")
            {
                soData = new SOIconData();
                SetSOData(soData, arr);
                string itemName = arr[8];
                string[] serializedValueArr = itemName.Split("*"[0]);
                if (serializedValueArr.Length > 1)
                {
                    soData.customization = serializedValueArr[1];
                    itemName = serializedValueArr[0];
                }
                soData.itemName = itemName;
                (soData as SOIconData).SetIcon();
            }
            
            if (soData != null)
            {
                StoryMakerEvents.AddSceneObject(soData);
                //Scenario.Instance.sceneObejctsManager.AddSceneObject(soData);
            }
        }
        protected void SetBG()
        {   if (bgID == null || bgID.Length == 0)
                return;
            SOBGData bgData = new SOBGData();
            bgData.id = bgID;
            StoryMakerEvents.AddSceneObject(bgData);
        }
        protected void SetSOData(SOData soData, string[] arr)
        {
            soData.id = arr[0];
            soData.pos = new V3(float.Parse(arr[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(arr[2], CultureInfo.InvariantCulture.NumberFormat), float.Parse(arr[3], CultureInfo.InvariantCulture.NumberFormat));
            soData.rot = float.Parse(arr[4], CultureInfo.InvariantCulture.NumberFormat);
            soData.size = float.Parse(arr[5], CultureInfo.InvariantCulture.NumberFormat);
            soData.itemName = arr[8];
        }
        protected void SetCustomizerBySerialization(SOData soData, string data)
        {
            soData.customization = data;
            StoryMakerEvents.SetNewAvatarCustomization(data);
        }

        public string Serialize()
        {
            string sceneData = bgID + serialSeparator;
            for (int i = 0; i < scenesElements.Count; i++)
            {
                sceneData += scenesElements[i];
                if (i < scenesElements.Count - 1)
                    sceneData += soSeparator;
            }
            return sceneData;
        }

        public void Deserialize(string s)
        {
            scenesElements = new List<string>();
            string[] arr = s.Split(serialSeparator[0]);
            bgID = arr[0];
            string[] arr2 = arr[1].Split(soSeparator[0]);
            for (int i = 0; i < arr2.Length; i++)
            {
                if (arr2[i] != "")
                    scenesElements.Add(arr2[i]);
            }
        }
        public virtual void MoveElements(float timeToNextFrame)
        {
            Debug.Log(scenesElements);
            foreach (string data in scenesElements)
            {
                SOData soAvatarToMove = GetAvatarFromSring(data);
                if (soAvatarToMove != null)
                {
                    Vector3 to = CharacterShouldWalkTo(soAvatarToMove, soAvatarToMove.id);
                    if (to != Vector3.zero)
                        Scenario.Instance.movementManager.MoveCharacter(soAvatarToMove.id, to, timeToNextFrame);
                }
            }
        }
        protected Vector3 CharacterShouldWalkTo(SOData actualData, string avatarID)
        {
            SceneData nextSD = ScenesManager.Instance.GetNextActiveScene();
            if (nextSD == null)
                return Vector3.zero;
            List<string> nextAll = nextSD.scenesElements;
            if (nextAll == null)
                return Vector2.zero;
            foreach (string s in nextAll)
            {
                SOData soData = GetAvatarFromSring(s);
                if (soData != null && soData.id == avatarID)
                {
                    if (actualData.size != soData.size || actualData.pos.Equals(soData.pos))
                        return Vector3.zero;
                    else
                        return soData.pos.ToVector3();
                }
            }
            return Vector3.zero;
        }
        protected SOData GetAvatarFromSring(string data)
        {
            string[] arr = data.Split(stringSeparator[0]);
            SOData soData = null;
            if (arr[7] == "a")
            {
                soData = new SOAvatarData();
                soData.customization = arr[8];
                soData.goLeft = false;
                if (arr[6].ToString() != "False")
                    soData.goLeft = true;
                SetSOData(soData, arr);
            }
            return soData;
        }

        public List<string> GetScenesElements() {
            return scenesElements;
        }

    }
}
