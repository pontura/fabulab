using System.Collections;
using UI.MainApp;
using UI.MainApp.Home.User;
using UnityEngine;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class FilmMakerManagerFabulab : FilmMakerManager
    {
        [SerializeField] protected Button newButton;
        [SerializeField] protected float delayFactor;
        [SerializeField] Toggle toggleTransition;
        [SerializeField] GameObject durationBtn;
        [SerializeField] VideoPlayerFabulab videoPlayerFabulab;
        [SerializeField] GameSelector gameSelector;
        [SerializeField] CamerasEditorUI camerasEditorUI;
        [SerializeField] ScenarioCameraManager scenarioCameraManager;

         float defaultZoom = 60;
        [SerializeField] Vector2 limitsCamZoom1;
        
        

        protected override void Awake() {
            base.Awake();
            StoryMakerEvents.EnableStoryEdition += EnableStoryEdition;
            StoryMakerEvents.OnTimelineSetJump += OnTimelineSetJump;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            StoryMakerEvents.EnableStoryEdition -= EnableStoryEdition;
            StoryMakerEvents.OnTimelineSetJump -= OnTimelineSetJump;
        }
        void EnableStoryEdition(bool enable) {
            isEditing = enable;
            deleteButton.gameObject.SetActive(enable);
            newButton.gameObject.SetActive(enable);
            //hamburguerButton.gameObject.SetActive(enable);
            toggleTransition.gameObject.SetActive(enable);
            videoPlayerFabulab.Show(!enable);
            durationBtn.gameObject.SetActive(enable);

            scenarioCameraManager.Init(enable);
            camerasEditorUI.Init(enable);
            camerasEditorUI.Show(enable);

            isPlayingGame = Data.Instance.gamesManager.IsEditing();
            print("StoryMakerEvents.EnableStoryEdition " + enable + " isPlayingGame: " + isPlayingGame);

            if(isPlayingGame)
                gameSelector.Show(false);  
            else
                gameSelector.Show(Data.Instance.gamesManager.activaGameData != "");

            if(enable || Data.Instance.gamesManager.IsEditing())
            {
                GetComponent<StoryEditorScreen>().Init();
                timeline.GetComponent<Animator>().Play("edit");
            }
            else
                timeline.GetComponent<Animator>().Play("player");

            timeline.EnableStoryEdition(enable);
            //  buttonsGroup.spacing = enable ? 5 : 10;
        }

        protected override void SetButtons()
        {
            int total = ScenesManagerFabulab.Instance.Scenes.Count;
            if (ScenesManagerFabulab.Instance.currentSceneId == 1)
                prevButton.interactable = false;
            else
                prevButton.interactable = true;

            if (total > 1 || !isEditing)
                playButton.interactable = true;
            // else
            // playButton.interactable = false;

            if (ScenesManagerFabulab.Instance.currentSceneId >= total)
                nextButton.interactable = false;
            else
                nextButton.interactable = true;

            if (total > 1)
                deleteButton.interactable = true;
            else
                deleteButton.interactable = false;


        }
        public override void New()
        {
            if (State == states.PLAYING)
                return;

            if (timeline.all.Count >= ScenesManagerFabulab.Instance.MaxKeyframes)
            {
                RunOnMaxFramesEvent();                
                return;
            }
            //Events.OnAddScore(2);
            StoryMakerEvents.OnSaveScene();

            //ScenesManagerFabulab.Instance.OnSaveScene();
            ScenesManagerFabulab.Instance.currentSceneId++;
            ScenesManagerFabulab.Instance.AddNewScene(ScenesManagerFabulab.Instance.currentSceneId);
            (Scenario.Instance.sceneObejctsManager as SceneObjectsManagerFabulab).TurnOfAllTexts();
            SetButtons();
            timeline.AddNewKeyframe();
            timeline.JumpTo(ScenesManagerFabulab.Instance.currentSceneId);
        }
        
        public override void OnDelete(bool doIt)
        {
            if (State == states.PLAYING)
                return;

            if (doIt)
            {
                if (Scenario.Instance != null)
                {
                    Scenario.Instance.sceneObejctsManager.ResetScene();
                }

                ScenesManagerFabulab.Instance.RemoveScene(ScenesManagerFabulab.Instance.currentSceneId);
                int lastSceneId = ScenesManagerFabulab.Instance.currentSceneId;
                if (ScenesManagerFabulab.Instance.currentSceneId > 1)
                    ScenesManagerFabulab.Instance.currentSceneId--;

                SetButtons();
                ScenesManagerFabulab.Instance.SetSceneObjectsIntoScenenario();

                timeline.RemoveKeyframe();

                timeline.JumpTo(ScenesManagerFabulab.Instance.currentSceneId);
            }
        }
        public override void Next()
        {
            Stop();


            if(isEditing)
            {                
                StoryMakerEvents.OnTimelinePlay(false); 
                StoryMakerEvents.OnSaveScene();
            }

            int lastSceneId = ScenesManagerFabulab.Instance.currentSceneId;
            
            int totalScenes = ScenesManagerFabulab.Instance.Scenes.Count;

            if (ScenesManagerFabulab.Instance.currentSceneId < totalScenes)
            {
                ScenesManagerFabulab.Instance.currentSceneId++;
                SetScene(lastSceneId);
                timeline.JumpTo(ScenesManagerFabulab.Instance.currentSceneId);
            }
        }
        public void PrevAll()
        {
            Stop();
            StoryMakerEvents.OnSaveScene();
            int lastSceneId = ScenesManagerFabulab.Instance.currentSceneId;
            ScenesManagerFabulab.Instance.currentSceneId = 1;
            SetScene(lastSceneId);
            timeline.JumpTo(ScenesManagerFabulab.Instance.currentSceneId);
        }
        public void NextAll()
        {
            Stop();
            StoryMakerEvents.OnSaveScene();
            int lastSceneId = ScenesManagerFabulab.Instance.currentSceneId;
            ScenesManagerFabulab.Instance.currentSceneId = ScenesManagerFabulab.Instance.Scenes.Count;
            SetScene(lastSceneId);
            timeline.JumpTo(ScenesManagerFabulab.Instance.currentSceneId);
        }
        public override void Prev()
        {
            Stop();
            StoryMakerEvents.OnSaveScene();
            //ScenesManagerFabulab.Instance.OnSaveScene();
            int lastSceneId = ScenesManagerFabulab.Instance.currentSceneId;
            ScenesManagerFabulab.Instance.currentSceneId--;
            if (ScenesManagerFabulab.Instance.currentSceneId < 1) ScenesManagerFabulab.Instance.currentSceneId = 1;
            SetScene(lastSceneId);
            timeline.JumpTo(ScenesManagerFabulab.Instance.currentSceneId);
        }

        void OnTimelineSetJump(int lastSceneId) {
            Debug.Log("& OnTimelineSetJump");
            Stop();
            SetButtons();

            if (ScenesManagerFabulab.Instance.GetActiveScene() != null) {
                toggleTransition.isOn = ScenesManagerFabulab.Instance.GetActiveScene().transition;
                StoryMakerEvents.SetBackgroundLights();
            }

            ScenesManagerFabulab.Instance.SetSceneObjectsIntoScenenario(lastSceneId);
            StoryMakerEvents.ReorderSceneObjectsInZ();

            Invoke(nameof(SetPaused), Time.deltaTime);
        }

        public override void OnTimelinePlay(bool isOn) {
            base.OnTimelinePlay(isOn);
            if (!isOn)
                Invoke(nameof(SetPaused), Time.deltaTime);
        }
        

        protected override void SetScene(int lastSceneId)
        {
            int total = ScenesManagerFabulab.Instance.Scenes.Count;
            int nextSceneid = ScenesManagerFabulab.Instance.currentSceneId + 1;
            SceneDataFabulab aciveScene = ScenesManagerFabulab.Instance.GetActiveScene() ;
            if (aciveScene!= null)
                timeline.keyframe_duration = Mathf.Max(aciveScene.duration, 0.5f);

            if (State == states.PLAYING && nextSceneid <= total)
            {
                string backgroundID = ScenesManagerFabulab.Instance.GetBackground(ScenesManagerFabulab.Instance.currentSceneId);
                string nextBackgroundID = ScenesManagerFabulab.Instance.GetBackground(nextSceneid);
                string prevBackgroundID = "";

                if(ScenesManagerFabulab.Instance.currentSceneId>1)
                    prevBackgroundID = ScenesManagerFabulab.Instance.GetBackground(ScenesManagerFabulab.Instance.currentSceneId - 1);

                print(ScenesManagerFabulab.Instance.currentSceneId + " nextSceneid : " + nextSceneid + " backgroundID: " + backgroundID + " : " + nextBackgroundID);
                if (backgroundID == nextBackgroundID)
                {
                    float delay = timeline.keyframe_duration * delayFactor;
                    Debug.Log("# Delay: " + delay);
                    StartCoroutine(MoveAvatarsAfter(delay));
                    StartCoroutine(MoveCamera(timeline.keyframe_duration));
                } else if (backgroundID != prevBackgroundID)
                {
                    SetCameraNewScene();
                }
            } else if(State == states.STOPPED) {
                Invoke(nameof(SetPaused), Time.deltaTime);
            } else
            {
                MoveCamSingleFrame();
            }

            if (ScenesManagerFabulab.Instance.currentSceneId > total)
                ScenesManagerFabulab.Instance.currentSceneId = total;
            else if (ScenesManagerFabulab.Instance.currentSceneId <= 1)
                ScenesManagerFabulab.Instance.currentSceneId = 1;

            SetButtons();

            if (ScenesManagerFabulab.Instance.GetActiveScene() != null) {
                toggleTransition.isOn = aciveScene.transition;
                StoryMakerEvents.SetBackgroundLights();
            }

            ScenesManagerFabulab.Instance.SetSceneObjectsIntoScenenario(lastSceneId);
            StoryMakerEvents.ReorderSceneObjectsInZ();
        }

        void SetPaused() {
         //   Debug.Log("& SetPaused");
            iTween.Stop();
            StoryMakerEvents.OnMoviePaused();
        }
        
       

        public override void MoveCamSingleFrame()
        {
            CamData currentScene = ScenesManagerFabulab.Instance.Scenes[ScenesManagerFabulab.Instance.currentSceneId-1].camData;
            Vector2 pos_from = GetPos(currentScene.pos, currentScene.zoom);
            scenarioCameraManager.OnUpdate(pos_from, currentScene.zoom);
        }
        void SetCameraNewScene()
        { 
            print("___________SetCameraNewScene");
            CamData currentScene = ScenesManagerFabulab.Instance.Scenes[ScenesManagerFabulab.Instance.currentSceneId-1].camData;
            if(currentScene.zoom < 1) currentScene.zoom = defaultZoom;
            scenarioCameraManager.OnUpdate(currentScene.pos, currentScene.zoom);
        }
        protected override IEnumerator MoveCamera(float duration)
        { 
            print("MoveCamera currentSceneId" + ScenesManagerFabulab.Instance.currentSceneId + " ScenesManagerFabulab.Instance.Scenes.Count_ " + ScenesManagerFabulab.Instance.Scenes.Count);
            if(ScenesManagerFabulab.Instance.currentSceneId>=ScenesManagerFabulab.Instance.Scenes.Count) 
                yield return null;
            else{
                CamData currentScene = ScenesManagerFabulab.Instance.Scenes[ScenesManagerFabulab.Instance.currentSceneId-1].camData;
                CamData nextScene = ScenesManagerFabulab.Instance.Scenes[ScenesManagerFabulab.Instance.currentSceneId].camData;

                 if(currentScene.zoom < 1) currentScene.zoom = defaultZoom;
                 if(nextScene.zoom < 1) nextScene.zoom = defaultZoom;

                print("______ currentScene pos:" + currentScene.pos + "  nextScene.pos : " + nextScene.pos  + " currentScene.zoom:" + currentScene.zoom);
                if(!currentScene.tween)
                {
                    Vector2 pos_from = GetPos(currentScene.pos, currentScene.zoom);
                    scenarioCameraManager.OnUpdate(pos_from, currentScene.zoom);
                }   
                else{
                    float a = 0;

                    Vector2 pos_from = GetPos(currentScene.pos, currentScene.zoom);
                    Vector2 pos_to = GetPos(nextScene.pos, nextScene.zoom);

                    while(a<duration)
                    {
                        float d = a/duration;
                        Vector2 pos = Vector2.Lerp(pos_from, pos_to, d);
                        float zoom = Mathf.Lerp(currentScene.zoom, nextScene.zoom, d);
                       

                        scenarioCameraManager.OnUpdate(pos, zoom);
                        yield return new WaitForEndOfFrame();
                        a += Time.deltaTime;
                    }
                }
            }
        }
        Vector2 GetPos(Vector2 pos, float zoom)
        {
            if(zoom == defaultZoom || zoom == 0)
            {
                return Data.Instance.settings.camDatas[0].pos;
            }
            else{
                pos.x = limitsCamZoom1.x * pos.x;
                pos.y = (limitsCamZoom1.y * pos.y) + 10;
            }
            return pos;
        }
        protected override IEnumerator MoveAvatarsAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (State == states.PLAYING)
                ScenesManagerFabulab.Instance.GetActiveScene().MoveElements(timeline.keyframe_duration - delay);
        }
        public override void JumpTo(int keyframeID)
        {
          //  Debug.Log("#JumpTo");
            int lastSceneId = ScenesManagerFabulab.Instance.currentSceneId;
            ScenesManagerFabulab.Instance.currentSceneId = keyframeID;
            SetScene(lastSceneId);
        }

        public void OnTransitionChange() {
            if(ScenesManagerFabulab.Instance!=null && ScenesManagerFabulab.Instance.GetActiveScene()!=null)
                ScenesManagerFabulab.Instance.GetActiveScene().transition = toggleTransition.isOn;
        }
    }
}
