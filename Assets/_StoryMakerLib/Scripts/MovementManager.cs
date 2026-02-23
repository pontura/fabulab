using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class MovementManager : MonoBehaviour
    {
        public void MoveCharacter(int avatarID, Vector3 to, float timeToNextFrame)
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

            
            iTween.MoveTo(avatar.gameObject, iTween.Hash(
                "position", to,
                "time", timeToNextFrame,
                "easetype", "linear",
                "oncomplete", "OnCompleteMoveing",
                "oncompletetarget", this.gameObject
                ));
            print(distanceToMove + "   esta en: " + avatar.transform.position + "va a : " + to + "    id : " + avatar.GetData().id + " time " + timeToNextFrame);
        }

    }
}
