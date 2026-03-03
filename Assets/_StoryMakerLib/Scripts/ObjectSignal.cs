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
            GetData().customization = text;
        }
        public override void OnInit()
        {
            SOData soData = GetData();
            if (soData == null)
                return;
            if (soData.customization != null && soData.customization.Length > 0)
                SetField(GetData().customization);
        }
    }
}
