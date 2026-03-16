using BoardItems.SceneObjects;
using System;
using System.Runtime.Serialization;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using static Yaguar.StoryMaker.Editor.WordBalloon;

namespace Yaguar.StoryMaker.Editor
{
    public class WordBox : ObjectSignal
    {
        public enum boxTypes {
            top,
            bottom            
        }

        public boxTypes boxType;

        [field: SerializeField] public Sprite[] boxIcons { get; private set; }

        public override void SetField(string text) {
            Debug.Log("&& SetField: " + text);
            field.text = text;
            (GetData() as SOWordBoxData).inputValue = text;
        }

        public override void OnInit() {
            SOData soData = GetData();
            if (soData == null)
                return;
            /*if (soData.customization != null && soData.customization.Length > 0)
                SetField(GetData().customization);*/

            SetField((soData as SOWordBoxData).inputValue);

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            collider.size = new Vector2(10, 10);

            boxType = (boxTypes)Enum.Parse(typeof(boxTypes), soData.id);

        }
        
    }
}
