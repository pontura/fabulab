using UI;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class CamManagerUI : MonoBehaviour
    {
        public ToggleButton[] cams;
        public ShotSubButtons subPanel;
        [SerializeField] CamerasEditorUI camerasEditorUI;
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
            
            Open(camData);
        }
        public void Open(CamData camData)
        {
            this.camData = camData;
            gameObject.SetActive(true);
          
            id = SetID(camData);

            foreach(ToggleButton tb in cams)
                tb.Force(false);
                
            print("open id:" +id  + " zoom: " +  camData.zoom);    

            cams[id].Force(true);
            StoryMakerEvents.ActivateCamDataEditionDrag(true);
        }
        int SetID(CamData camData)
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
            Events.OnPopupTopSignalText(Data.Instance.settings.camDatas[id].name);

            this.id = id;
            CamData setttingsCamData = Data.Instance.settings.camDatas[id];            
            StoryMakerEvents.SetCamDataEdition(setttingsCamData.pos, setttingsCamData.zoom);
            ScenesManagerFabulab.Instance.GetActiveScene().camData = camData;
            Open(camData);
        }
        public void Close()
        {        
            subPanel.Reset();
            gameObject.SetActive(false);
            subPanel.Close();
            StoryMakerEvents.ActivateCamDataEditionDrag(false);
            if(camData.pos != camerasEditorUI.normalizedPos)
            {
                camData.pos = camerasEditorUI.normalizedPos;
                print("_________" + camData.pos);
                camerasEditorUI.Reset();
            }
        }
    }
}