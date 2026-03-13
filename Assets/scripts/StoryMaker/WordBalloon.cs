using BoardItems.SceneObjects;
using System;
using System.Runtime.Serialization;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;

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
        [SerializeField] SpriteRenderer spriteRenderer;

        [field:SerializeField] public Sprite[] balloonSprites { get; private set; }
                
        public override void OnInit() {
            SOData soData = GetData();
            if (soData == null)
                return;
            if (soData.customization != null && soData.customization.Length > 0)
                SetField(GetData().customization);        

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            collider.size = new Vector2(10, 10);

            balloonType = (balloonTypes)Enum.Parse(typeof(balloonTypes), soData.id);

            spriteRenderer.sprite = balloonSprites[(int)balloonType];

        }
    }
}
