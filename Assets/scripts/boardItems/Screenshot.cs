using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BoardItems.UI;

namespace BoardItems
{
    public class Screenshot : MonoBehaviour
    {
        public Vector2Int thumbRes;
        public Camera cameraToScreen;
        private bool takeShot = false;
        private bool copyTex = false;
        private Texture2D texture;
        private System.Action<Texture2D> CopyTexture;

        // Start is called before the first frame update
        void Start()
        {
            if (cameraToScreen == null)
                cameraToScreen = GetComponent<Camera>();
        }

        public void TakeShot(System.Action<Texture2D> copytex)
        {
            takeShot = true;
            // Debug.Log("TAKE SHOT");
            CopyTexture = copytex;
        }

        public void TakeShot(Vector2Int size, System.Action<Texture2D> copytex)
        {
            thumbRes = size;
            takeShot = true;
            // Debug.Log("TAKE SHOT");
            CopyTexture = copytex;
        }

        void LateUpdate()
        {

            if (copyTex)
            {
                Events.OnLoading(true);
                CopyTexture(texture);
                copyTex = false;
            }

            if (takeShot)
            {
                Debug.Log(Screen.width + " x " + Screen.height);
                float res = 1f * Screen.height / Screen.width;
                RenderTexture rt = new RenderTexture(thumbRes.x, (int)(thumbRes.x * res), 32);
                texture = new Texture2D((int)(thumbRes.x * 0.6f), (int)(thumbRes.x * 0.6f), TextureFormat.RGB24, false);
                if (Screen.width * 0.6f < Screen.height)
                {
                    res = 1f * Screen.width / Screen.height;
                    rt = new RenderTexture((int)(thumbRes.x * res), thumbRes.y, 32);
                    texture = new Texture2D(thumbRes.y, thumbRes.y, TextureFormat.RGB24, false);
                }

                cameraToScreen.targetTexture = rt;

                Color[] pixels = Enumerable.Repeat(UIManager.Instance.boardUI.cam.backgroundColor, thumbRes.x * thumbRes.y).ToArray();
                texture.SetPixels(pixels);

                cameraToScreen.Render();
                RenderTexture.active = rt;
                if (Screen.width * 0.6f >= Screen.height)
                    texture.ReadPixels(new Rect(0, 0, rt.width * 0.6f, rt.height), 0, (int)(((rt.width * 0.6f) - rt.height) * 0.5f));
                else
                    texture.ReadPixels(new Rect(0, 0, rt.width * 0.6f, rt.height), (int)((rt.height - (rt.width * 0.6f)) * 0.5f), 0);

                texture.Apply();

                cameraToScreen.targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                Destroy(rt);

                takeShot = false;
                copyTex = true;
            }
        }
    }

}