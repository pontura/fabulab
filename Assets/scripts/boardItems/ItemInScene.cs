using BoardItems.Characters;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace BoardItems
{
    public class ItemInScene : MonoBehaviour
    {
        [SerializeField] ItemInScene mirror;
        public ItemData data;
        public Rigidbody2D rb;
        [SerializeField] Collider2D collider;
        [SerializeField] List<Collider2D> colliders;
        List<SpriteRenderer> sprites;
        AudioSource audioSource;
        ItemInSceneAnims anims;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody2D>();

            rb.constraints = RigidbodyConstraints2D.FreezeAll;

            if(collider == null)
                collider = GetComponent<Collider2D>();

            sprites = new List<SpriteRenderer>();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.outputAudioMixerGroup = AudioManager.Instance.sfx;
            AudioClip clip = Resources.Load<AudioClip>("hit");
            audioSource.clip = clip;
            audioSource.volume = 0.1f;
            colliders = new List<Collider2D>();
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            {
                Component c = CopyComponent(collider, sr.gameObject);
                Collider2D c2d = c.GetComponent<Collider2D>();
                c2d.enabled = true;
                colliders.Add(c2d);
                sprites.Add(sr);
            }

            if (data != null)
                SetColor(data.colorName);
        }
        public void Appear()
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
        Component CopyComponent(Component original, GameObject destination)
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy;
        }
        public void RotateSetValue(float v)
        {
            float newAngle = transform.localEulerAngles.z + v;
            data.rotation = new Vector3(0, 0, newAngle);
            transform.localEulerAngles = data.rotation;
            AudioManager.Instance.sfxManager.SetPitch((newAngle % 360) * 0.0056f);
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
            AudioManager.Instance.sfxManager.SetPitch(Data.Instance.settings.maxScale - Mathf.Abs(nenewValue));
        }
        float timer;
        public void StartBeingDrag()
        {
            timer = Time.time;
            print("StartBeingDrag");
            rb = gameObject.GetComponent<Rigidbody2D>();

            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody2D>();

                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.isKinematic = true;

            if(colliders != null)
                foreach (Collider2D c in colliders)
                    c.enabled = false;
            SetCollider(true);

            if(bpOver != null)
                Events.OnNewBodyPartSelected(bpOver);
        }
        public void StopBeingDrag()
        {
            rb.isKinematic = false;
            foreach (Collider2D c in colliders)
                c.enabled = true;
            Events.OnNewBodyPartSelected(null);
        }
        public void StartFalling()
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.AddTorque(Random.Range(-5, 5) * 500);
            rb.isKinematic = false;
            foreach (Collider2D c in colliders)
                c.enabled = true;
            SetCollider(true);
        }
        public void SetPos(Vector2 pos)
        {
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        }
        public void SetPosByData()
        {
            transform.localPosition = data.position;
        }
        public void SetColor(PalettesManager.colorNames name)
        {
            data.colorName = name;
            List<Color> allColors = Data.Instance.palettesManager.GetColorsByName(name);
            int id = 0;
            foreach (SpriteRenderer sr in sprites)
            {
                if (id <= allColors.Count - 1)
                    sr.color = allColors[id];
                id++;
            }
        }
        public bool IsBeingUse()
        {
            return data.part != CharacterPartsHelper.parts.none;
        }
        public bool IsInActiveBodyPart(BodyPart bodyPart)
        {
            if (bodyPart.part == UIManager.Instance.zoomManager.part)
                return true;
            return false;
        }
        public bool IsOverBodyPart()
        {
            if (bpOver != null)
                return true;
            return false;
        }
        [SerializeField] BodyPart bpOver;
       // [SerializeField] BodyPart bpEnter;
        void OnTriggerEnter2D(Collider2D collision)
        {
            BodyPart bpEnter = collision.gameObject.GetComponent<BodyPart>();
            if (bpEnter.part != data.part)  return;
            if (bpOver == null)
            {
                data.SetCharacterPart(bpEnter.part);
                Events.OnNewBodyPartSelected(bpEnter);
                bpOver = bpEnter;
                ArrengeZPos(bpOver);
            }
        }
        void OnTriggerExit2D(Collider2D collision)
        {
           // print("coll OnTriggerExit2D: " + (timer + 0.1f) + "     Timer " + Time.time);
            if (timer+0.01f > Time.time) return;
            BodyPart bpExit = collision.GetComponent<BodyPart>();
            if (bpExit.part != data.part) return;

            Events.OnNewBodyPartSelected(null);
            bpOver = null;
        }
        public ItemInScene GetMirror() {  return mirror;  }
        public void SetMirror(ItemInScene mirrored)
        {
            mirror = mirrored;
        }
        void ArrengeZPos(BodyPart bp)
        {
            if (rb.bodyType != RigidbodyType2D.Kinematic) return;
            float z = bp.GetLastZ();
            if (z == 0) return; //no objects in bodypart;
            print("Arrenge Z" + z);
            Vector3 pos = transform.position;
            pos.z = z;
            transform.position = pos;
        }
        Animation anim;
        public void SetTools(bool isOn, AnimationClip clip = null)
        {
            if (isOn)
            {
                if (anim == null)
                {
                    anim = colliders[0].gameObject.AddComponent<Animation>();
                    anim.AddClip(clip, clip.name);
                    anim.clip = clip;
                    anim.Play();
                }
            }
            else if(anim != null)
            {
                colliders[0].gameObject.transform.localScale = Vector3.one;
                Destroy(anim);
            }
        }
    }
}
