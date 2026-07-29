using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class ScenarioCameraManager : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] Vector2 limitsCamZoom1;

        void Start()
        {
            StoryMakerEvents.SetCamData +=SetCamData;
        }
        void OnDestroy()
        {
            StoryMakerEvents.SetCamData -=SetCamData;
        }
        private void SetCamData(Vector2 pos, int zoom)
        {
            if(zoom == 0)
                zoom = 60;

            cam.orthographicSize = zoom;
            Vector3 camPos = cam.transform.position;
            if(zoom == 60) pos = Vector2.zero;

            camPos.x = limitsCamZoom1.x * pos.x;
            camPos.y = limitsCamZoom1.y * pos.y;

            cam.transform.position = camPos;
            print("SetCamData "+ pos + "  zoom : " + zoom );
        }
    }
}
