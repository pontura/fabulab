using BoardItems.Characters;
using Google.MiniJSON;
using UI.MainApp;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class AvatarFabulab : Avatar
    {

        [SerializeField] GameObject character_to_instantiate;
        [field:SerializeField] public CharacterManager characterManager { private set; get; }

        BordersCreator borders;

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
        }

        public override void  BeginDrag() {
            base.BeginDrag();
            if (borders == null) {
                borders = gameObject.AddComponent<BordersCreator>();
                borders.Init(GetComponent<BoxCollider2D>());
            }

            borders.Show(true);
        }

        public override void StopDrag() {            
            if (initDragPos.Equals(transform.localPosition))
                StoryMakerEvents.ShowSoButtons(Scenario.Instance.Cam.WorldToScreenPoint(transform.position), data);
            isBeingHeld = false;
            StoryMakerEvents.ReorderSceneObjectsInZ();
            borders.Show(false);
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
