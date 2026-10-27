using UnityEngine;
using System.Collections.Generic;

namespace Yaguar.StoryMaker.Editor
{
    public static class StoryMakerEvents
    {
        public static System.Action<Vector2, float, bool> SetCamData;
        public static System.Action<Vector2, float> SetCamDataEdition;
        public static System.Action<bool> ActivateCamDataEditionDrag;
        public static bool isEditing;
        public static void SetEditing(bool editing)
        {
            isEditing = editing;
        }
        public static System.Action UpdateDraw = delegate { };
        public static System.Action<WordBalloon> OnInputField = delegate { };

        public static System.Action OnStartNewStory = delegate { };

        public static System.Action OnLoadFilm = delegate { };
        public static System.Action<int> ChangeSpeed = delegate { };
        public static System.Action RewindStory = delegate { };
        public static System.Action OnStoryChanged = delegate { };
        public static System.Action<bool> ShowSaveBtn = delegate { };
        public static System.Action ReorderSceneObjectsInZ = delegate { };
        public static System.Action<SceneObject> OnStopDraw = delegate { };

        public static System.Action<Vector3, SOData> ShowSoButtons = delegate { };
        public static System.Action HideSoButtons = delegate { };

        public static System.Action<bool> OnTimelinePlay = delegate { };
        public static System.Action OnMovieOver = delegate { };
        public static System.Action OnMoviePaused = delegate { };

        public static System.Action Restart = delegate { };
        public static System.Action ClearScene = delegate { };
        public static System.Action HideScene = delegate { };

        public static System.Action EditActions = delegate { };
        public static System.Action EditExpressions = delegate { };

        public static System.Action NoneItemSelected = delegate { };

        public static System.Action<bool> EnableInputManager = delegate { };

        public static System.Action<bool> EnableStoryEdition = delegate { };
    }
}
