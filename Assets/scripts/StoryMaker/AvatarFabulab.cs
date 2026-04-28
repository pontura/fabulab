using BoardItems.Characters;
using Google.MiniJSON;
using UI.MainApp;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Yaguar.StoryMaker.Editor
{
    public class AvatarFabulab : Avatar
    {

        [SerializeField] GameObject character_to_instantiate;
        [field:SerializeField] public CharacterManager characterManager { private set; get; }

        public BordersCreator Borders { private set; get; }

        private void Start()
        {
            StoryMakerEvents.SetAvatarData += SetData;
            Events.OnCharacterAnim += OnCharacterAnim;
            Events.OnCharacterExpression += OnCharacterExpression;
        }
        private void OnDestroy()
        {
            StoryMakerEvents.SetAvatarData -= SetData;
            Events.OnCharacterAnim -= OnCharacterAnim;
            Events.OnCharacterExpression -= OnCharacterExpression;
        }

        public override void OnInit() {

                        
            if (asset != null)
                Destroy(asset.gameObject);

            asset = Instantiate(character_to_instantiate, transform);
            //asset.transform.SetParent(transform);
            asset.transform.localEulerAngles = Vector3.zero;
            asset.transform.localPosition = Vector3.zero;
            asset.SetActive(true);
            characterManager = asset.GetComponent<CharacterManager>();
            characterManager.Init();

            Invoke("Delayed", 0.1f);            
        }
        void Delayed()
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            Bounds worldBounds = new Bounds(transform.position, Vector3.zero);
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>()) {
                worldBounds.Encapsulate(sr.bounds);
            }

            Vector3[] corners = new Vector3[8];
            Vector3 ext = worldBounds.extents;
            Vector3 cen = worldBounds.center;
            int i = 0;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                        corners[i++] = cen + Vector3.Scale(ext, new Vector3(x, y, z));

            Bounds localBounds = new Bounds(transform.InverseTransformPoint(corners[0]), Vector3.zero);
            for (int j = 1; j < corners.Length; j++) {
                localBounds.Encapsulate(transform.InverseTransformPoint(corners[j]));
            }

            collider.offset = localBounds.center;
            collider.size = localBounds.size;

            foreach (Collider2D coll in GetComponentsInChildren<Collider2D>()) {
                if (coll == collider) 
                    continue;
                coll.enabled = false;
            }
        }        

        public override void StopDrag() {
            if(Mathf.Approximately(initDragPos.x, transform.localPosition.x) &&
               Mathf.Approximately(initDragPos.y, transform.localPosition.y)) {
                StoryMakerEvents.ShowSoButtons(Input.mousePosition, data);
                if (Borders == null) {
                    Borders = gameObject.AddComponent<BordersCreator>();
                    Borders.Init(GetComponent<BoxCollider2D>());
                }
                Borders.Show(true);
            } /*else {
                Debug.Log("% mouse pos change: " + transform.localPosition + " = " + initDragPos);                
            }*/
            //StoryMakerEvents.ShowSoButtons(Scenario.Instance.Cam.WorldToScreenPoint(transform.position), data);
            if (Vector2.Distance(initDragPos, transform.position) < MIN_DISTANCE) {
                transform.position = initDragPos;
                data.pos = new V3(initDragPos.x, initDragPos.y, data.pos.z);
            }

            isBeingHeld = false;
            StoryMakerEvents.ReorderSceneObjectsInZ();
            //borders.Show(false);
        }
        public override void Run() {
            characterManager.SetAnim(Data.Instance.characterAnimsManager.defaultRun.name);
        }

        public override void Walk() {
            characterManager.SetAnim(Data.Instance.characterAnimsManager.defaultWalk.name);
        }

        void OnCharacterExpression(string _characterID, string exp) {
            if (data.id != _characterID) return;
            (data as SOAvatarFabulabData).emoji = exp;
        }

        void OnCharacterAnim(string _characterID, string anim) {
            if(data.id != _characterID) return;
            (data as SOAvatarFabulabData).anim = anim;
        }
    }
}
