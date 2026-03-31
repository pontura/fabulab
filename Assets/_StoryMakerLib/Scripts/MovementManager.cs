using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class MovementManager : MonoBehaviour
    {
        public void MoveCharacter(string avatarID, Vector3 to, float timeToNextFrame)
        {
            Avatar avatar = Scenario.Instance.sceneObejctsManager.GetAvatarInSceneById(avatarID);
            if (avatar == null)
                return;

            float distanceToMove = Vector3.Distance(avatar.transform.position, to);
            if (distanceToMove < 1.5f)
                return;
            if (distanceToMove > 5)
                avatar.Run();
            else
                avatar.Walk();

            if (to.x < avatar.transform.position.x) {
                avatar.FlipX(true);
            } else {
                avatar.FlipX(false);
            }


            iTween.MoveTo(avatar.gameObject, iTween.Hash(
                    "islocal", true,
                    "position", to,
                    "time", timeToNextFrame,
                    "easetype", "linear",
                    "oncomplete", "OnCompleteMoveing",
                    "oncompletetarget", this.gameObject
                    ));
            print(distanceToMove + "   esta en: " + avatar.transform.position + "va a : " + to + "    id : " + avatar.GetData().id + " time " + timeToNextFrame);
        }

        public void MoveElement(SOData soData, Vector3 to, float timeToNextFrame) {
            Debug.Log("&& MoveElement: "+soData.pos.ToString()+" : "+to);
            SceneObject so = Scenario.Instance.sceneObejctsManager.GetSceneObjectInScene(soData);
            if (so == null) {
                Debug.Log("& SO null");
                return;
            }

            iTween.MoveTo(so.gameObject, iTween.Hash(
                    "islocal", true,
                    "position", to,
                    "time", timeToNextFrame,
                    "easetype", "linear",
                    "oncomplete", "OnCompleteMoveing",
                    "oncompletetarget", this.gameObject
                    ));

            print("& Esta en: " + so.transform.position + "va a : " + to + "    id : " + so.GetData().id + " time " + timeToNextFrame);
        }

        public void ScaleElement(SOData soData, float to, float timeToNextFrame) {
            Debug.Log("&& ScaleElement");
            SceneObject so = Scenario.Instance.sceneObejctsManager.GetSceneObjectInScene(soData);
            if (so == null)
                return;

            int signX = so.transform.localScale.x < 0 ? -1 : 1;

            Vector3 scale = new Vector3(to * signX, to, to);
            iTween.ScaleTo(so.gameObject, iTween.Hash(
                    "islocal", true,
                    "scale", scale,
                    "time", timeToNextFrame,
                    "easetype", "linear",
                    "oncomplete", "OnCompleteMoveing",
                    "oncompletetarget", this.gameObject
                    ));
        }

        public void RotateElement(SOData soData, float to, float timeToNextFrame) {
            Debug.Log("&& RotateElement");
            SceneObject so = Scenario.Instance.sceneObejctsManager.GetSceneObjectInScene(soData);
            if (so == null)
                return;

            Vector3 rotation = new Vector3(0f, 0f, to);
            iTween.RotateTo(so.gameObject, iTween.Hash(
                    "islocal", true,
                    "rotation", rotation,
                    "time", timeToNextFrame,
                    "easetype", "linear",
                    "oncomplete", "OnCompleteMoveing",
                    "oncompletetarget", this.gameObject
                    ));
        }



    }
}
