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
        }
    }
}
