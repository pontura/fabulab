using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class FilmMakerManager : MonoBehaviour
    {
        [field:SerializeField] public states State { private set; get;}
        public enum states
        {
            STOPPED,
            PLAYING
        }
        [SerializeField] protected GameObject panel;
        [SerializeField] protected Button nextButton;
        [SerializeField] protected Button prevButton;
        [SerializeField] protected Button playButton;
        [SerializeField] protected Button deleteButton;

        [SerializeField] protected TimelineInSceneUI timelineInSceneUI;
        [SerializeField] protected Timeline timeline;
        [SerializeField] public bool isEditing;

        public event Action<Action<bool>> DeleteDialog;
        public event Action OnMaxFrames;

        protected static FilmMakerManager mInstance = null;
        public static FilmMakerManager Instance
        {
            get
            {
                return mInstance;
            }
        }
        protected virtual void Awake()
        {
            if (!mInstance)
                mInstance = this;
            
            StoryMakerEvents.OnTimelinePlay += OnTimelinePlay;
            StoryMakerEvents.Restart += Restart;
        }
        protected virtual void OnDestroy()
        {
            StoryMakerEvents.OnTimelinePlay -= OnTimelinePlay;
            StoryMakerEvents.Restart -= Restart;
            DeleteDialog = null;
            OnMaxFrames = null;
        }

        public void Show(bool enable)
        {
            panel.SetActive(enable);
        }
        
        protected void Restart()
        {
            Debug.Log("Restart");
            //timeline.Reset();
            timelineInSceneUI.RefreshField(1);
        }
        public virtual void OnTimelinePlay(bool isOn)
        {
            if (isOn)
            {
                //Events.OnAddScore(1);
                Play();
            }
            else
            {
                Stop();
            }
        }

        protected void Play() {
            if (isEditing)
                StoryMakerEvents.OnSaveScene();
            State = states.PLAYING;
            timeline.InitPlaying();
            StoryMakerEvents.ReorderSceneObjectsInZ();
        }

        protected void Stop() {
            playButton.GetComponent<PlayButton>().SetButtons(false);
            State = states.STOPPED;
            iTween.Stop();
        }

        protected void Update()
        {
            if (State == states.STOPPED)
                return;

            timeline.OnUpdate();
        }



        public void SetOn(bool isOn)
        {
            if (isOn)
            {
                panel.SetActive(true);
                timelineInSceneUI.Init();
                SetButtons();
            }
            else
            {
                panel.SetActive(false);
            }

        }
        protected virtual void SetButtons()
        {
            int total = ScenesManager.Instance.Scenes.Count;
            if (ScenesManager.Instance.currentSceneId == 1)
                prevButton.interactable = false;
            else
                prevButton.interactable = true;

            if (total > 1 || !isEditing)
                playButton.interactable = true;
            // else
            // playButton.interactable = false;

            if (ScenesManager.Instance.currentSceneId >= total)
                nextButton.interactable = false;
            else
                nextButton.interactable = true;

            if (total > 1)
                deleteButton.interactable = true;
            else
                deleteButton.interactable = false;

            timelineInSceneUI.RefreshField(ScenesManager.Instance.currentSceneId);


        }
        public virtual void New()
        {
            Debug.Log(ScenesManager.Instance == null);

            if (timeline.all.Count >= ScenesManager.Instance.MaxKeyframes)
            {
                RunOnMaxFramesEvent();
                return;
            }
            //Events.OnAddScore(2);
            StoryMakerEvents.OnSaveScene();
            //ScenesManager.Instance.OnSaveScene();
            ScenesManager.Instance.currentSceneId++;
            ScenesManager.Instance.AddNewScene(ScenesManager.Instance.currentSceneId);
            SetButtons();
            timeline.AddNewKeyframe();
            timeline.JumpTo(ScenesManager.Instance.currentSceneId);
        }

        protected void RunOnMaxFramesEvent() {
            if(OnMaxFrames!=null)
                OnMaxFrames();
        }

        public void Delete()
        {
            if (DeleteDialog != null)
                DeleteDialog(OnDelete);
            else
                OnDelete(true);
        }
        public virtual void OnDelete(bool doIt)
        {
            if (doIt)
            {
                if (Scenario.Instance != null)
                {
                    Scenario.Instance.sceneObejctsManager.ClearScene();
                }

                ScenesManager.Instance.RemoveScene(ScenesManager.Instance.currentSceneId);
                int lastSceneId = ScenesManager.Instance.currentSceneId;
                if (ScenesManager.Instance.currentSceneId > 1)
                    ScenesManager.Instance.currentSceneId--;

                SetButtons();
                ScenesManager.Instance.SetSceneObjectsIntoScenenario(lastSceneId);

                timeline.RemoveKeyframe();

                timeline.JumpTo(ScenesManager.Instance.currentSceneId);
            }
        }
        public virtual void Next()
        {
            StoryMakerEvents.OnSaveScene();
            //ScenesManager.Instance.OnSaveScene();
            int lastSceneId = ScenesManager.Instance.currentSceneId;
            ScenesManager.Instance.currentSceneId++;
            SetScene(lastSceneId);
            timeline.JumpTo(ScenesManager.Instance.currentSceneId);
        }
        public virtual void Prev()
        {
            StoryMakerEvents.OnSaveScene();
            //ScenesManager.Instance.OnSaveScene();
            int lastSceneId = ScenesManager.Instance.currentSceneId;
            ScenesManager.Instance.currentSceneId--;
            SetScene(lastSceneId);
            timeline.JumpTo(ScenesManager.Instance.currentSceneId);
        }
        protected virtual void SetScene(int lastSceneId)
        {
            int total = ScenesManager.Instance.Scenes.Count();
            int nextSceneid = ScenesManager.Instance.currentSceneId + 1;

            if (State == states.PLAYING && nextSceneid <= total)
            {
                string backgroundID = ScenesManager.Instance.GetBackground(ScenesManager.Instance.currentSceneId);
                string nextBackgroundID = ScenesManager.Instance.GetBackground(nextSceneid);
                print(ScenesManager.Instance.currentSceneId + " nextSceneid : " + nextSceneid + " backgroundID: " + backgroundID + " : " + nextBackgroundID);
                if (backgroundID == nextBackgroundID)
                {
                    float delay = timeline.keyframe_duration - (timeline.keyframe_duration / 3);
                    StartCoroutine(MoveAvatarsAfter(delay));
                }
            }

            if (ScenesManager.Instance.currentSceneId > total)
                ScenesManager.Instance.currentSceneId = total;
            else if (ScenesManager.Instance.currentSceneId <= 1)
                ScenesManager.Instance.currentSceneId = 1;

            SetButtons();

            ScenesManager.Instance.SetSceneObjectsIntoScenenario(lastSceneId);
            StoryMakerEvents.ReorderSceneObjectsInZ();
        }
        protected virtual IEnumerator MoveAvatarsAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (State == states.PLAYING)
                ScenesManager.Instance.GetActiveScene().MoveElements(timeline.keyframe_duration - delay);
        }
        public virtual void JumpTo(int keyframeID)
        {
            Debug.Log("#JumpTo");
            int lastSceneId = ScenesManager.Instance.currentSceneId;
            ScenesManager.Instance.currentSceneId = keyframeID;
            SetScene(lastSceneId);
        }
    }
}
