using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using BoardItems.UI;
using BoardItems;

namespace uGIF
{
	public class CaptureToGIF : MonoBehaviour
	{
        public string filename;
        public int baseFrate = 40;
		public float frameRate = 20;
        public float frameRateWebGL = 20;
        public bool capture;
		public int downscale = 1;
		public float captureTime = 10;
        public int quality = 10; // 1 best - 20 worst
        public int qualityWebGL = 10; // 1 best - 20 worst
        public bool useBilinearScaling = true;
        public bool globalColorTable = true;
        public Color32 transparentColor;
        Vector2Int gifRes;
        public Vector2Int gifResMobile;
        public Vector2Int gifResWebGL;
        public Camera cameraToScreen;

        List<Image> frames;

        float res;
        Texture2D colorBuffer;
        float period;
        float T = 0;
        float startTime = 0;
        bool capturing;

        int frameCount;
        int totalFrameCount;
        int totalFrames;
        int frameFactor;

        RenderTexture rt;

        System.Action onCaptureEnd;

        [System.NonSerialized]
		public byte[] bytes = null;

		void Start ()
		{
            Events.CaptureGif += CaptureGif;
#if UNITY_WEBGL && !UNITY_EDITOR
            period = 1f / frameRateWebGL;
            gifRes = gifResWebGL;
#else
            period = 1f / frameRate;
            gifRes = gifResMobile;
#endif
            Init();
            //colorBuffer = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);

            //startTime = Time.time;

        }

        void Init() {
            res = 1f * Screen.height / Screen.width;
            rt = new RenderTexture(gifRes.x, (int)(gifRes.x * res), 32);
            colorBuffer = new Texture2D((int)(gifRes.x * 0.6f), (int)(gifRes.x * 0.6f), TextureFormat.RGB24, false);
            if (Screen.width * 0.6f < Screen.height) {
                res = 1f * Screen.width / Screen.height;
                rt = new RenderTexture((int)(gifRes.x * res), gifRes.y, 32);
                colorBuffer = new Texture2D(gifRes.y, gifRes.y, TextureFormat.RGB24, false);
            }
        }

        void CaptureGif(string id, System.Action callback) {
            if (Mathf.Abs((1f * Screen.height / Screen.width) - res) > 0.001f) {
                Debug.Log("HAS RESIZE: "+res+" - "+ (1f * Screen.height / Screen.width));
                Init();
            }
            frames = new List<Image>();
            frameCount = 0;
            totalFrameCount = 0;
#if UNITY_WEBGL && !UNITY_EDITOR
            totalFrames = (int)(frameRateWebGL * captureTime);
            frameFactor = baseFrate / (int)frameRateWebGL;
#else
            totalFrames = (int)(frameRate * captureTime);
            frameFactor = baseFrate / (int)frameRate;
#endif
            onCaptureEnd = callback;
            string folder = Path.Combine(Application.persistentDataPath, "Gifs");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            filename = Path.Combine(folder, "gif_" + id + ".gif");
            capture = true;
            UIManager.Instance.boardUI.items.ResetAllAnims();
        }

        private void Update() {
            if (capture) {
                capture = false;
                startTime = Time.time;
                capturing = true;
                //Data.Instance.shareManager.gifState = ShareManager.GIFSTATES.CAPTURING;
                Debug.Log("Capture...");
                Time.captureFramerate = baseFrate;
            }
        }

        public void Encode ()
		{
            //Events.OnEndCaptureGif();
            
            bytes = null;
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(__Encode());
#else
            var thread = new Thread (_Encode);
            thread.Start();
            StartCoroutine(WaitForBytes());
#endif
        }

        IEnumerator WaitForBytes() {
            while (bytes == null) 
                yield return null;            

            System.IO.File.WriteAllBytes (filename, bytes);
            bytes = null;
           // Data.Instance.shareManager.gifState = ShareManager.GIFSTATES.DONE;
            Debug.Log("Done...");
            onCaptureEnd();
        }

        public void _Encode ()
		{
			capturing = false;

			var ge = new GIFEncoder ();
			ge.useGlobalColorTable = globalColorTable;
			ge.repeat = 0;
#if UNITY_WEBGL && !UNITY_EDITOR
			ge.FPS = frameRateWebGL;
#else
            ge.FPS = frameRate;
#endif
            ge.transparent = transparentColor;
			ge.dispose = 1;
            ge.quality = quality;

			var stream = new MemoryStream ();
            Debug.Log("stream");
			ge.Start (stream);
            Debug.Log("start");
            int i = 0;
            foreach (var f in frames) {
                //Debug.Log("frame"+i);
                i++;
                if (downscale != 1) {
					if(useBilinearScaling) {
						f.ResizeBilinear(f.width/downscale, f.height/downscale);
					} else {
						f.Resize (downscale);
					}
				}
				f.Flip ();
				ge.AddFrame (f);
			}
            Debug.Log("finish");
            ge.Finish ();
            Debug.Log("GetBuffer");
            bytes = stream.GetBuffer ();
            Debug.Log("Close");
            stream.Close();
           // Data.Instance.shareManager.gifState = ShareManager.GIFSTATES.SAVING;
            Debug.Log("Saving...");
        }

        IEnumerator __Encode() {
            capturing = false;

            var ge = new GIFEncoder();
            ge.useGlobalColorTable = globalColorTable;
            ge.repeat = 0;
#if UNITY_WEBGL && !UNITY_EDITOR
			ge.FPS = frameRateWebGL;
#else
            ge.FPS = frameRate;
#endif
            ge.transparent = transparentColor;
            ge.dispose = 1;
            ge.quality = quality;

            var stream = new MemoryStream();
            Debug.Log("stream");
            ge.Start(stream);
            Debug.Log("start");
            int i = 0;
            foreach (var f in frames) {
                //Debug.Log("frame"+i);
                i++;
                if (downscale != 1) {
                    if (useBilinearScaling) {
                        f.ResizeBilinear(f.width / downscale, f.height / downscale);
                    } else {
                        f.Resize(downscale);
                    }
                }
                f.Flip();
                ge.AddFrame(f);
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("finish");
            ge.Finish();
            Debug.Log("GetBuffer");
            bytes = stream.GetBuffer();
            Debug.Log("Close");
            stream.Close();
          //  Data.Instance.shareManager.gifState = ShareManager.GIFSTATES.SAVING;
            Debug.Log("Saving...");
            StartCoroutine(WaitForBytes());
        }

        //void FixedUpdate() {
        void LateUpdate() {

            if (capturing) {
                //Debug.Break();
                /*T += Time.deltaTime;
                if (T >= period) {
                    T = 0;*/
                if (totalFrameCount % frameFactor == 0) { 
                    RenderTexture rt = new RenderTexture(gifRes.x, (int)(gifRes.x * res), 32);
                    if (Screen.width * 0.6f < Screen.height)
                        rt = new RenderTexture((int)(gifRes.x * res), gifRes.y, 32);

                    cameraToScreen.targetTexture = rt;

                    Color[] pixels = Enumerable.Repeat(UIManager.Instance.boardUI.cam.backgroundColor, gifRes.x * gifRes.y).ToArray();
                    colorBuffer.SetPixels(pixels);

                    cameraToScreen.Render();
                    RenderTexture.active = rt;

                    if (Screen.width * 0.6f >= Screen.height)
                        colorBuffer.ReadPixels(new Rect(0, 0, rt.width * 0.6f, rt.height), 0, (int)(((rt.width * 0.6f) - rt.height) * 0.5f));
                    else
                        colorBuffer.ReadPixels(new Rect(0, 0, rt.width * 0.6f, rt.height), (int)((rt.height - (rt.width * 0.6f)) * 0.5f), 0);

                    colorBuffer.Apply();
                    frames.Add(new Image(colorBuffer));                    

                    cameraToScreen.targetTexture = null;
                    RenderTexture.active = null; // JC: added to avoid errors
                    Destroy(rt);
                    frameCount++;
                }
                UIManager.Instance.boardUI.items.NextStepAnims(totalFrameCount,baseFrate);
                totalFrameCount++;
                //if (Time.time > (startTime + captureTime)) {
                if (frameCount >= totalFrames) { 
                    capture = false;
                 //   Data.Instance.shareManager.gifState = ShareManager.GIFSTATES.ENCODING;
                    Debug.Log("Encoding...");
                    Time.captureFramerate = 0;
                    Encode();
                }
            }
        }
	}
}