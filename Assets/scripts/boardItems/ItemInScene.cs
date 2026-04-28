using BoardItems.Characters;
using System.Collections.Generic;
using UI;
using UI.MainApp;
using UnityEngine;

namespace BoardItems
{
    public class ItemInScene : MonoBehaviour
    {
        [SerializeField] ItemInScene mirror;
        public ItemData data;
        public Rigidbody2D rb;
        [SerializeField] Collider2D collider;
        // [SerializeField] List<Collider2D> colliders;
        [SerializeField] SpriteRenderer sprite;
        ItemInSceneAnims anims;
        public bool itemInsideContainer; // si el item esta dentro de un objeto compuesto

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody2D>();

            rb.constraints = RigidbodyConstraints2D.FreezeAll;

            if(collider == null)
                collider = GetComponent<Collider2D>();

            if(sprite == null)
                sprite = GetComponentInChildren<SpriteRenderer>();

            //print("ItemInScene Init " + gameObject.name);
        }
        private void Start()
        {
            if (GetComponent<BoardItemManager>() != null) return;// "Object inside an other object";
            
            if (data != null)
                SetColor(data.colorName);
        }
        public void Appear(float offsetY = 8)
        {
            Vector3 from = data.position;
            from.y -= offsetY;
            transform.localPosition = from;
        }
        public void AppearAction()
        {            
            if (anims == null)
                 anims = gameObject.AddComponent<ItemInSceneAnims>();

            anims.Appear(data);
        }
        public void SetCollider(bool isOn)
        {
            if (collider == null)
                collider = GetComponent<Collider2D>();
            if(collider != null)
                collider.enabled = isOn;
        }
        public void RotateSetValue(float v)
        {
            float newAngle = transform.localEulerAngles.z + v;
            data.rotation = new Vector3(0, 0, newAngle);
            transform.localEulerAngles = data.rotation;
            //AudioManager.Instance.sfxManager.SetPitch((newAngle % 360) * 0.0056f);
        }

        public void ScaleSetValue(float v)
        {
            print("ScaleSetValue " + v + transform.gameObject.name);
            float nenewValue = Mathf.Min(transform.localScale.x + (v / 100), Data.Instance.settings.maxScale);
            if (nenewValue < -1 * Data.Instance.settings.maxScale)
                nenewValue = -1 * Data.Instance.settings.maxScale;
            else if (nenewValue > Data.Instance.settings.maxScale)
                nenewValue = Data.Instance.settings.maxScale;

#if !UNITY_WEBGL && !UNITY_EDITOR
        else if ((nenewValue > -1 * Data.Instance.settings.minScale) && (nenewValue <= 0))
            nenewValue = Data.Instance.settings.minScale;
        else if ((nenewValue < Data.Instance.settings.minScale) && (nenewValue >= 0))
            nenewValue = Data.Instance.settings.minScale;
#endif

            data.scale = new Vector3(nenewValue, nenewValue, 1);
            transform.localScale = data.scale;
           // AudioManager.Instance.sfxManager.SetPitch(Data.Instance.settings.maxScale - Mathf.Abs(nenewValue));
        }
        float timer;
        public void StartBeingDrag()
        {
            SetBorders(true);
            timer = Time.time;
            print("StartBeingDrag");
            rb = gameObject.GetComponent<Rigidbody2D>();

            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody2D>();

                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.isKinematic = true;

            if(collider != null)
                collider.enabled = false;
            SetCollider(true);

            //if(bpOver != null)
            //    Events.OnNewBodyPartSelected(bpOver);
        }
        public void StopBeingDrag()
        {
            SetBorders(false);
            rb.isKinematic = false;
            collider.enabled = true;
            print("StopBeingDrag"); 
        }
        public void SetPos(Vector3 pos, bool snap = false)
        {
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        }
        public void SetPosByData()
        {
            transform.localPosition = data.position;
        }
        public void SetColor(PalettesManager.colorNames name)
        {
            if (sprite == null) return;
           // print("SetColor item in scene " + name + " sprite: " + sprite + " data.colorName " + data.colorName);
            data.colorName = name;
            List<Color> allColors = Data.Instance.palettesManager.GetColorsByName(name);
            
            sprite.color = allColors[0];
        }
        public bool IsBeingUse()
        {
            return data.part != CharacterPartsHelper.parts.none;
        }
        public bool IsInActiveBodyPart(BodyPart bodyPart)
        {
            if ((int)bodyPart.part == (int)UIManager.Instance.zoomManager.currentZoom)
                return true;
            return false;
        }
        public bool IsOverBodyPart()
        {
            if (bpOver != null)
                return true;
            return false;
        }
        public void InitItemInPart(BodyPart bpEnter)
        {
            data.SetContainer(bpEnter);
            data.SetCharacterPart(bpEnter.part);
            bpOver = bpEnter;
        }
        [SerializeField] BodyPart bpOver;
        void OnTriggerEnter2D(Collider2D collision)
        {
            BodyPart bpEnter = collision.gameObject.GetComponent<BodyPart>();
            if (bpEnter.part != data.part)  return;
            if (bpOver == null)
            {
                InitItemInPart( bpEnter);
            }
        }
        void OnTriggerExit2D(Collider2D collision)
        {
            if (timer+0.01f > Time.time) return;
            BodyPart bpExit = collision.GetComponent<BodyPart>();
            if (bpExit.part != data.part) return;
            bpOver = null;
        }
        public ItemInScene GetMirror() {  return mirror;  }
        public void SetMirror(ItemInScene mirrored)
        {
            mirror = mirrored;
        }

        Animation anim;
        BordersCreator borders;
        public void SetTools(bool isOn, AnimationClip clip = null)
        {
            SetBorders(isOn);
        }
        void SetBorders(bool isOn)
        {
            if (!UIManager.Instance.inputManager.groupOn)
            {
                print("SetBorders " + isOn + " go: " + gameObject.name);
                if (borders == null)
                {
                    borders = gameObject.AddComponent<BordersCreator>();
                    if (GetComponent<BoardItemManager>())
                        borders.Init(GetComponent<BoxCollider2D>());
                    else
                        borders.Init(sprite);
                }

                borders.Show(isOn);
            }
        }
    }
}
