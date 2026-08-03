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
                cam.transform.position = new Vector3(0,  3.34f, cam.transform.position.z);
            }
        }
        private void SetCamData(Vector2 pos, float zoom, bool tween)
        {
            ApplyZoom1(pos, zoom);
        }
        public void OnUpdate(Vector2 pos, float zoom)
        {
            ApplyZoom1(pos, zoom);
            print("OnUpdate CamData "+ pos + "  zoom : " + zoom );
        }
        void ApplyZoom1(Vector2 pos, float zoom)
        {
            if(zoom == 0) zoom = defaultZoom;
            cam.orthographicSize = zoom;         
            cam.transform.position = pos;
            print("OnUpdate " + pos);
        }
    }
}
