using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Yaguar.StoryMaker.Editor
{
    public class ObjectSignal : Objeto
    {
        public TextMeshPro field;
        public void SetField(string text)
        {
            field.text = text;
            GetData().customizationSerialized = text;
        }
        public override void OnInit()
        {
            SOData soData = GetData();
            if (soData == null)
                return;
            if (soData.customizationSerialized != null && soData.customizationSerialized.Length > 0)
                SetField(GetData().customizationSerialized);
        }
    }
}
