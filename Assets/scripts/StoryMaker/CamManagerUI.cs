
using UI;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class CamManagerUI : MonoBehaviour
    {
        public ToggleButton[] cams;
        public ShotSubButtons subPanel;
        public GameObject camPanel;
        public GameObject[] camPanelCams;

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
        void OnDisable()
        {
            camPanel.SetActive(false);     
        }
        int id;
        public void Open(CamData camData)
        {
            print("open " + camData.zoom);

            gameObject.SetActive(true);
            camPanel.SetActive(true);         
            camPanel.gameObject.SetActive(true);
          
            id = SetID(camData);

            foreach(GameObject cd in camPanelCams)
                cd.SetActive(false);

            camPanelCams[id].gameObject.SetActive(true);

            foreach(ToggleButton tb in cams)
                tb.Force(false);

            cams[id].Force(true);
        }
        int SetID(CamData camData)
        {              
            int _id = 0;
            foreach(CamData cd in Data.Instance.settings.camDatas)
            {
                if(cd.zoom == camData.zoom)                    
                    return id;
                _id++;
            }
            return 0;
        }
        public void OnClicked(int id)
        {
            this.id = id;
            Open(Data.Instance.settings.camDatas[id]);
        }
        public void Close()
        {        
            subPanel.Reset();
            gameObject.SetActive(false);
            subPanel.Close();

        }
    }
}