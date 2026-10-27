using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class MovementManager : MonoBehaviour
    {
        [SerializeField] float minDistanceToMove = 1.5f;
        [SerializeField] float velocityThresh = 18f;

        // Tweens the avatars and props of a scene towards their position, size and rotation in the next one
        public void MoveElements(SceneData scene, SceneData nextScene, float timeToNextFrame) {
            if (!scene.transition || nextScene == null)
                return;

            foreach (SceneElement element in scene.GetScenesElements()) {
                if (element.type != SceneElementType.AVATAR && element.type != SceneElementType.PROP)
                    continue;

                SceneElement nextElement = nextScene.GetElement(element.data.itemName);
                if (nextElement == null || nextElement.type != element.type)
                    continue;

                SOData soData = element.GetSOData();
                SOData nextData = nextElement.GetSOData();
                if (!soData.pos.Equals(nextData.pos)) {
                    if (element.type == SceneElementType.AVATAR)
                        MoveCharacter(soData.itemName, nextData.pos.ToVector3(), timeToNextFrame);
                    else
                        MoveElement(soData, nextData.pos.ToVector3(), timeToNextFrame);
                }
                if (!soData.size.Equals(nextData.size))
                    ScaleElement(soData, nextData.size, timeToNextFrame);

                if (!soData.rot.Equals(nextData.rot))
                    RotateElement(soData, nextData.rot, timeToNextFrame);
            }
        }

        public void MoveCharacter(string itemName, Vector3 to, float timeToNextFrame) {
            Avatar avatar = Scenario.Instance.sceneObjectsManager.GetAvatarInSceneByItemName(itemName);

            if (avatar == null)
                return;

            float distanceToMove = Vector3.Distance(avatar.transform.position, to);
            if (distanceToMove  < minDistanceToMove)
                return;
            if (distanceToMove/timeToNextFrame > velocityThresh)
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
            //print(distanceToMove + "   esta en: " + avatar.transform.position + "va a : " + to + "    id : " + avatar.GetData().id + " time " + timeToNextFrame);
        }

        public void MoveElement(SOData soData, Vector3 to, float timeToNextFrame) {
            SceneObject so = Scenario.Instance.sceneObjectsManager.GetSceneObjectInScene(soData);
            if (so == null) {
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

        }

        public void ScaleElement(SOData soData, float to, float timeToNextFrame) {
            SceneObject so = Scenario.Instance.sceneObjectsManager.GetSceneObjectInScene(soData);
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
            SceneObject so = Scenario.Instance.sceneObjectsManager.GetSceneObjectInScene(soData);
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
