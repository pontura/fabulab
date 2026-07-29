using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class CamManagerUI : MonoBehaviour
    {
        public ToggleButton[] cams;
        public ShotSubButtons subPanel;
        [SerializeField] CamerasEditorUI camerasEditorUI;
        [SerializeField] Toggle tweebBtn;
        CamData camData;
        int id;

        public void Init()
        {
            gameObject.SetActive(false);            
        }
        void Awake()
        {
            int id = 0;
            foreach(ToggleButton tb in cams)
            {
                tb.InitButton(id, OnClicked);
                id++;
            }
        }
        public void OpenCam()
        {
            camData = ScenesManagerFabulab.Instance.Scenes[ScenesManagerFabulab.Instance.currentSceneId-1].camData;
              
            StoryMakerEvents.SetCamDataEdition(camData.pos, camData.zoom);
            
            gameObject.SetActive(true);
          
            id = GetID(camData);
            tweebBtn.SetIsOnWithoutNotify(camData.tween);
            tweebBtn.isOn = camData.tween;

            ZoomSelection();
         
            StoryMakerEvents.ActivateCamDataEditionDrag(true);
        }
        void ZoomSelection()
        {
            foreach(ToggleButton tb in cams)
                tb.Force(false);
                
            print("open id:" +id  + " zoom: " +  camData.zoom);    

            cams[id].Force(true);
        }
        int GetID(CamData camData)
        {              
            int _id = 0;
            foreach(CamData cd in Data.Instance.settings.camDatas)
            {
                if(cd.zoom == camData.zoom)                    
                    return _id;
                _id++;
            }
            return 0;
        }
        public void OnClicked(int id)
        {
            CamData setttingsCamData = Data.Instance.settings.camDatas[id];  
            Events.OnPopupTopSignalText(setttingsCamData.name);

            this.id = id;       
            camData.zoom = setttingsCamData.zoom;   
            StoryMakerEvents.SetCamDataEdition(camData.pos, camData.zoom);
            ZoomSelection();
        }
        public void Close()
        {        
            subPanel.Reset();
            gameObject.SetActive(false);
            subPanel.Close();
            StoryMakerEvents.ActivateCamDataEditionDrag(false);
            camData.tween = tweebBtn.isOn;
            if(camData.pos != camerasEditorUI.normalizedPos)
            {
                camData.pos = camerasEditorUI.normalizedPos;
                print("_________" + camData.pos);
                camerasEditorUI.Reset();
            }
        }
    }
}