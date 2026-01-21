using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace Yaguar.StoryMaker.Editor
{
    public static class StoryMakerEvents
    {

        public static System.Action<int, SOActionData> OnAction = delegate { };
        public static System.Action<int, SOExpData> OnExpression = delegate { };
        public static System.Action<ObjectSignal> OnInputField = delegate { };

        public static System.Action<SOData> AddSceneObject = delegate { };
        public static System.Action<SOData> DeleteSceneObject = delegate { };
        public static System.Action RemoveSceneObject = delegate { };
        public static System.Action OnLoadFilm = delegate { };
        public static System.Action ResetScenario = delegate { };
        public static System.Action<int> ChangeSpeed = delegate { };
        public static System.Action OnSaveScene = delegate { };
        public static System.Action ReorderSceneObjectsInZ = delegate { };

        public static System.Action<Vector3, SOData> ShowSoButtons = delegate { };
        public static System.Action HideSoButtons = delegate { };

        public static System.Action<int> JumpTo = delegate { };
        public static System.Action<bool> OnTimelinePlay = delegate { };
        public static System.Action OnMovieOver = delegate { };

        public static System.Action SceneCompleteLoading = delegate { };

        public static System.Action<string> SetNewAvatarCustomization = delegate { };

        public static System.Action Restart = delegate { };

        public static System.Action EditCustomize = delegate { };
        public static System.Action EditActions = delegate { };
        public static System.Action EditExpressions = delegate { };

        public static System.Action<SOData> SetAvatarData = delegate { };

        public static System.Action NoneAvatarSelected = delegate { };
    }
}
