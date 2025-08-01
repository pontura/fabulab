using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
namespace BoardItems
{
    public class ItemInScene : MonoBehaviour
    {
        float initialScale;
        public ItemData data;
        public Rigidbody2D rb;
        public Collider2D collider;
        public Collider2D[] colliders;
        List<SpriteRenderer> sprites;
        AudioSource audioSource;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
            if (collider)
            {
                collider.enabled = false;
            }
            sprites = new List<SpriteRenderer>();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.outputAudioMixerGroup = AudioManager.Instance.sfx;
            AudioClip clip = Resources.Load<AudioClip>("hit");
            audioSource.clip = clip;
            audioSource.volume = 0.1f;
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            {
                CopyComponent(collider, sr.gameObject);
                collider.enabled = true;
                sprites.Add(sr);
            }
            colliders = GetComponentsInChildren<Collider2D>();

            if (data != null)
                SetColor(data.colorName);


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
        public void StartBeingDrag()
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.isKinematic = true;
            foreach (Collider2D c in colliders)
                c.enabled = false;
        }
        public void StopBeingDrag()
        {
            rb.isKinematic = false;
            foreach (Collider2D c in colliders)
                c.enabled = true;
        }
        public void StartFalling()
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.AddTorque(Random.Range(-5, 5) * 500);
            rb.isKinematic = false;
            foreach (Collider2D c in colliders)
                c.enabled = true;
        }
        public void SetPos(Vector2 pos)
        {
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);
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
            if (rb != null && rb.constraints == RigidbodyConstraints2D.FreezeAll)
                return true;
            return false;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {

            if (collision.relativeVelocity.magnitude > 2)
            {
                if (!audioSource.isPlaying && AudioManager.Instance.IsVoiceRoom())
                {
                    audioSource.volume = collision.relativeVelocity.magnitude * 0.25f * (0.15f + Random.Range(-0.05f, 0.05f));
                    audioSource.pitch = 0.75f + Random.Range(-0.25f, 0.25f);
                    audioSource.Play();
                    AudioManager.Instance.VoiceCount(audioSource.clip.length);
                }
            }

            //Debug.Log(gameObject.name+" Play Sound2");

        }
    }
}
