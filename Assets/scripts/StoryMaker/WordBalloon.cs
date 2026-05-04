using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class WordBalloon : ObjectSignal
    {
        int standardSize = 22;
        int genericSize = 60;

        public enum balloonTypes {
            speech,
            scream,
            thought,
            generic,
            title
        }

        public balloonTypes balloonType;

        [SerializeField] Sprite arrowSimple;
        [SerializeField] Sprite arrowThink;

        [SerializeField] Image image;
        [SerializeField] Image[] arrows;
        [SerializeField] Canvas canvas;

        [field:SerializeField] public Sprite[] balloonSprites { get; private set; }
        [field: SerializeField] public Sprite[] balloonIcons { get; private set; }

        public override void SetField(string text)
        {
            print("SetField Direction text: " + text);
            field.text = text;
            (GetData() as SOWordBalloonData).inputValue = text;
        }
        public override void SetDirection(int direction)
        {
            print("SetDirection " + direction);
            (GetData() as SOWordBalloonData).direction = direction;
            SetDirectionArrow(direction);
        }
        public void SetFont(int id)
        {
            Fonts font =  (Scenario.Instance.sceneObejctsManager as SceneObjectsManagerFabulab).FontAssets.GetFont(id);
            field.font = font.fontAsset;
        }

        public override void OnInit() {

            SOData soData = GetData();
            if (soData == null)
                return;
            /*if (soData.customization != null && soData.customization.Length > 0)
                SetField(GetData().customization);*/

            string text = (soData as SOWordBalloonData).inputValue;
            Debug.Log("&& WordBalloon " + text);

            SetField((soData as SOWordBalloonData).inputValue);
           

            //BoxCollider2D collider = GetComponent<BoxCollider2D>();
            //collider.size = new Vector2(10, 10);

            if (Enum.IsDefined(typeof(balloonTypes), soData.id))
                balloonType = (balloonTypes)Enum.Parse(typeof(balloonTypes), soData.id);
            else
                balloonType = balloonTypes.generic;
            SetArrow(); 
            int direction = (soData as SOWordBalloonData).direction;
            SetDirection(direction);
            SetDirectionArrow(direction);

            image.sprite = balloonSprites[(int)balloonType];
            data.pos.z = -50;
            gameObject.transform.localPosition = data.pos.ToVector3();

            if (balloonType == balloonTypes.generic || balloonType == balloonTypes.title)
            {
                canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(genericSize, canvas.GetComponent<RectTransform>().sizeDelta.y);
                foreach (Image i in arrows)
                    i.enabled = false;
            }
            else
            {
                canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(standardSize, canvas.GetComponent<RectTransform>().sizeDelta.y);
                foreach (Image i in arrows)
                    i.enabled =true;
            }
        }
        void SetArrow()
        {
            Sprite arrow = arrowSimple;
            if (balloonType == balloonTypes.thought) arrow = arrowThink;

            if(balloonType != balloonTypes.generic)
                foreach (Image i in arrows)
                    i.sprite = arrow;
        }
        public override void UpdatePos()
        {
            Vector3 scale = image.transform.localScale;
            V3 lastPos = data.pos;
            base.UpdatePos();
            V3 pos = data.pos;
            Vector2 v2 = new Vector2(pos.x - lastPos.x, pos.y - lastPos.y);
            int p = Quantize8Stable(v2);
            if (p>= 0)
                SetDirection(p);
        }
        void SetDirectionArrow(int dir)
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
            if(balloonType == balloonTypes.generic || balloonType == balloonTypes.title) return 0; // sin flecha
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
