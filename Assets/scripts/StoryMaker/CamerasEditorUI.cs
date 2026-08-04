using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class CamerasEditorUI : MonoBehaviour
    {
        [SerializeField] GameObject[] editorCams;
        [SerializeField] bool canBeDrag;
        [SerializeField] float zoom;
        [SerializeField] float fullZoom;
        [SerializeField] GameObject editingBG;
        [SerializeField] GameObject arrows;

        GameObject zoomGO;
        float limitZoom;
        Vector2 intialMousePos;
        public Vector2 normalizedPos;
        bool isEditing;

        void Start()
        {
            ActivateCamDataEditionDrag(false);
            fullZoom = Data.Instance.settings.camDatas[0].zoom;
            StoryMakerEvents.OnTimelinePlay += OnTimelinePlay;
            StoryMakerEvents.SetCamDataEdition +=SetCamDataEdition;
            StoryMakerEvents.ActivateCamDataEditionDrag += ActivateCamDataEditionDrag;
        }
        void OnDestroy()
        {
            StoryMakerEvents.OnTimelinePlay -= OnTimelinePlay;
            StoryMakerEvents.SetCamDataEdition -=SetCamDataEdition;
            StoryMakerEvents.ActivateCamDataEditionDrag -= ActivateCamDataEditionDrag;
        }
        public void Init(bool isEditing)
        {
            this.isEditing = isEditing;
            foreach(GameObject go in editorCams)
                go.SetActive(false); 
        }
        public void Show(bool isOn)
        {
            if(!isEditing) isOn = false;
            print("CamerasEditorUI " + isOn);
            gameObject.SetActive(isOn);
        }
        public void Reset()
        {
            normalizedPos = Vector2.zero;
        }
        private void ActivateCamDataEditionDrag(bool canBeDrag)
        {
            editingBG.SetActive(canBeDrag);
            this.canBeDrag = canBeDrag;
            arrows.SetActive(canBeDrag);
        }

        private void OnTimelinePlay(bool isPlay)
        {
            if(!isEditing) {Show(false);return;}
            print("OnTimelinePlay " +isPlay);
            Show(!isPlay);
            if(!isPlay)
            {
                bool tween = true;
                StoryMakerEvents.SetCamData(
                    Data.Instance.settings.camDatas[0].pos, 
                    Data.Instance.settings.camDatas[0].zoom,
                    tween
                    );// resetea el zoom en el edit:
      
            }       
        }

        private void SetCamDataEdition(Vector2 normalizedPos, float zoom)
        {
            this.zoom = zoom;
            int id = 0;            
            foreach(GameObject go in editorCams)
            {
                if(Data.Instance.settings.camDatas[id].zoom == zoom)
                {
                    zoomGO = go;                    
                    limitZoom = Data.Instance.settings.limitZooms[id];
                    go.SetActive(true);
                }
                else 
                    go.SetActive(false);                
                id++;                
            }
            if(zoomGO == null) return;
            RectTransform rt = zoomGO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(normalizedPos.x*limitZoom, normalizedPos.y*limitZoom); 
        }
        void Update()
        {
            if(canBeDrag && zoom != fullZoom)
            {
                if(Input.GetMouseButtonDown(0))
                {                    
                    intialMousePos = Input.mousePosition;
                } else if(Input.GetMouseButton(0))
                {
                    Vector2 newPos = Input.mousePosition;

                    if(intialMousePos != newPos)
                    {
                        UpdatePosition(newPos-intialMousePos);
                        intialMousePos = newPos;
                    }
                }
                else if(Input.GetMouseButtonUp(0))
                {
                    intialMousePos = Vector2.zero;
                }
            }
        }
        void UpdatePosition(Vector2 newPos)
        {
            RectTransform rt = zoomGO.GetComponent<RectTransform>();
            Vector2 pos = rt.anchoredPosition;            
            pos.x += newPos.x;
            pos.y += newPos.y;

            if(pos.x<-limitZoom) pos.x = -limitZoom;
            else if(pos.x>limitZoom) pos.x = limitZoom;
            
            if(pos.y<-limitZoom) pos.y = -limitZoom;
            else if(pos.y>limitZoom) pos.y = limitZoom;

            rt.anchoredPosition = pos;
            
            normalizedPos = new Vector2(pos.x/limitZoom, pos.y/limitZoom);
        }
    }
}
