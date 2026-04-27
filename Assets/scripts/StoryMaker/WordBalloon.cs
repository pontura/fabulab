using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class WordBalloon : ObjectSignal
    {
        public enum balloonTypes {
            speech,
            scream,
            thought
        }

        public balloonTypes balloonType;

        [SerializeField] Image image;
        [SerializeField] Image[] arrows;
        [SerializeField] Canvas canvas;

        [field:SerializeField] public Sprite[] balloonSprites { get; private set; }
        [field: SerializeField] public Sprite[] balloonIcons { get; private set; }

        public override void SetField(string text) {
            field.text = text;
            (GetData() as SOWordBalloonData).inputValue = text;
        }

        public override void OnInit() {
            SOData soData = GetData();
            if (soData == null)
                return;
            /*if (soData.customization != null && soData.customization.Length > 0)
                SetField(GetData().customization);*/

            string text = (soData as SOWordBalloonData).inputValue;
            Debug.Log("&& " + text);

            SetField((soData as SOWordBalloonData).inputValue);

            //BoxCollider2D collider = GetComponent<BoxCollider2D>();
            //collider.size = new Vector2(10, 10);

            balloonType = (balloonTypes)Enum.Parse(typeof(balloonTypes), soData.id);
            image.sprite = balloonSprites[(int)balloonType];
            data.pos.z = -50;
            gameObject.transform.localPosition = data.pos.ToVector3();
            SetDirection(0);
        }
        public override void UpdatePos()
        {
            Vector3 scale = image.transform.localScale;
            V3 lastPos = data.pos;
            base.UpdatePos();
            V3 pos = data.pos;
            Vector2 v2 = new Vector2(pos.x - lastPos.x, pos.y - lastPos.y);
            int p = Quantize8Stable(v2);
            if(p>= 0)
                SetDirection(p);
        }
        void SetDirection(int dir)
        {
            foreach (Image i in arrows)
                i.gameObject.SetActive(false);
            arrows[dir].gameObject.SetActive(true);
        }

        int lastDir = -1;
        Vector2 smooth = Vector2.zero;

        float smoothFactor = 0.18f;
        float angleThreshold = 25f;
        float deadzone = 0.2f;

        int Quantize8Stable(Vector2 input)
        {
            if (input.magnitude < deadzone)
                return lastDir;

            // 1. suavizado
            smooth = Vector2.Lerp(smooth, input.normalized, smoothFactor);

            // 2. ángulo
            float angle = Mathf.Atan2(smooth.y, smooth.x) * Mathf.Rad2Deg;
            angle -= 180;
            if (angle < 0) angle += 360;

            int candidate = Mathf.RoundToInt(angle / 45f) % 8;

            if (lastDir == -1)
            {
                lastDir = candidate;
                return lastDir;
            }

            // 3. ángulo entre direcciones (en grados)
            float currentAngle = lastDir * 45f;
            float delta = Mathf.DeltaAngle(currentAngle, (candidate * 45f));

            if (Mathf.Abs(delta) > angleThreshold)
            {
                lastDir = candidate;
            }

            return lastDir;
        }

    }
}
