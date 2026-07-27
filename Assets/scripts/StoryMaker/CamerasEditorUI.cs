using System;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class CamerasEditorUI : MonoBehaviour
    {
        [SerializeField] GameObject[] editorCams;

        void Start()
        {
            StoryMakerEvents.OnTimelinePlay += OnTimelinePlay;
            StoryMakerEvents.SetCamDataEdition +=SetCamDataEdition;
        }
        void OnDestroy()
        {
            StoryMakerEvents.OnTimelinePlay -= OnTimelinePlay;
            StoryMakerEvents.SetCamDataEdition -=SetCamDataEdition;
        }

        private void OnTimelinePlay(bool isPlay)
        {
            print("OnTimelinePlay " +isPlay);
            gameObject.SetActive(!isPlay);
        }

        private void SetCamDataEdition(Vector2 pos, int zoom)
        {
            int id = 0;
            foreach(GameObject go in editorCams)
            {
                if(Data.Instance.settings.camDatas[id].zoom == zoom)
                    go.SetActive(true);
                else 
                    go.SetActive(false);                
                id++;                
            }
        }
    }
}
