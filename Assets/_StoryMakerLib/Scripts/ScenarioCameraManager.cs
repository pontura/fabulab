using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class ScenarioCameraManager : MonoBehaviour
    {
        [SerializeField] Camera cam;

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
            print("SetCamData "+pos + "  zoom : " +zoom );
        }
    }
}
