using BoardItems.Characters;
using BoardItems.UI;
using System.Collections.Generic;
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
        [SerializeField] bool used;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody2D>();

            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            used = false;

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
            used = true;
            rb = gameObject.GetComponent<Rigidbody2D>();

            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody2D>();

                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.isKinematic = true;

            if(colliders != null)
                foreach (Collider2D c in colliders)
                    c.enabled = false;
            SetCollider(true);

            if(bp != null)
                Events.OnNewBodyPartSelected(bp);
        }
        public void StopBeingDrag()
        {
            used = true;
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
            return used;
            if (rb != null && rb.constraints == RigidbodyConstraints2D.FreezeAll)
                return true;
            return false;
        }
        public bool IsInActiveBodyPart(BodyPart bodyPart)
        {
            if (bodyPart.part == UIManager.Instance.zoomManager.part)
                return true;
            return false;
        }
        [SerializeField] BodyPart bp;
       // [SerializeField] BodyPart bpEnter;
        void OnTriggerEnter2D(Collider2D collision)
        {
            BodyPart bpEnter = collision.gameObject.GetComponent<BodyPart>();
            if (!IsInActiveBodyPart(bpEnter))
            {
                return;
            }
            if (bp == null)
            {
                data.SetCharacterPart(bpEnter.part);
                Events.OnNewBodyPartSelected(bpEnter);
                bp = bpEnter;
                ArrengeZPos(bp);
            }
            //if(bp == null && bpEnter != null)
            //    bp = bpEnter;
        }
        void OnTriggerExit2D(Collider2D collision)
        {
           // print("coll OnTriggerExit2D: " + (timer + 0.1f) + "     Timer " + Time.time);
            if (timer+0.01f > Time.time) return;
            BodyPart bpExit = collision.GetComponent<BodyPart>();

            if (!IsInActiveBodyPart(bpExit))
            {
                bpExit = null; return;
            }
            if (bpExit != null)
            {
                if(bp == bpExit)
                {    
                    data.OutOfBody();
                    //if (bp == bpEnter)
                    //{

                        data.SetCharacterPart(CharacterData.parts.none);
                        Events.OnNewBodyPartSelected(null);
                        bp = null;
                       // bpEnter = null;
                    //}
                    //else
                    //{
                    //    bp = bpEnter;
                    //    data.SetCharacterPart(bp.part);
                    //    Events.OnNewBodyPartSelected(bp);
                    //}
                }
                //else if (bpEnter != null)
                //{
                //    bpEnter = bp;
                //    data.OutOfBody();
                //    print("OnTriggerExit2D and set last bp: " + bp.part);
                //    data.SetCharacterPart(bp.part);
                //    Events.OnNewBodyPartSelected(bp);
                //}
            }
        }
        //void OnCollisionEnter2D(Collision2D collision)
        //{
        //   // print("coll: " + collision.gameObject.name);
        //    if (collision.relativeVelocity.magnitude > 2)
        //    {
        //        if (!audioSource.isPlaying && AudioManager.Instance.IsVoiceRoom())
        //        {
        //            audioSource.volume = collision.relativeVelocity.magnitude * 0.25f * (0.15f + Random.Range(-0.05f, 0.05f));
        //            audioSource.pitch = 0.75f + Random.Range(-0.25f, 0.25f);
        //            audioSource.Play();
        //            AudioManager.Instance.VoiceCount(audioSource.clip.length);
        //        }
        //    }
        //}
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
    }
}
