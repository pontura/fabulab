using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class ScenarioCameraManager : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] Vector2 limitsCamZoom1;
        float defaultZoom = 60;
        void Start()
        {
            StoryMakerEvents.SetCamData += SetCamData;
            StoryMakerEvents.OnTimelinePlay += OnTimelinePlay;
        }
        void OnDestroy()
        {
            StoryMakerEvents.SetCamData -= SetCamData;
            StoryMakerEvents.OnTimelinePlay -= OnTimelinePlay;
        }

        private void OnTimelinePlay(bool play)
        {
            if(!play)
            {
                 cam.orthographicSize = defaultZoom;
                 Vector3 camPos = Vector2.zero;
            }
        }
        private void SetCamData(Vector2 pos, float zoom, bool tween)
        {
            ApplyZoom1(pos, zoom);
        }
        public void OnUpdate(Vector2 pos, float zoom)
        {
            ApplyZoom1(pos, zoom);
        }
        void ApplyZoom1(Vector2 pos, float zoom)
        {
            cam.orthographicSize = zoom;
            Vector3 camPos = cam.transform.position;

            if(zoom == defaultZoom) 
                camPos = Vector2.zero;
            else{
                camPos.x = limitsCamZoom1.x * pos.x;
                camPos.y = (limitsCamZoom1.y * pos.y) + 10;
            }
            
            cam.transform.position = camPos;
            print("OnUpdate CamData "+ pos + " camPos: " + camPos  + "  zoom : " + zoom );
        }
    }
}
