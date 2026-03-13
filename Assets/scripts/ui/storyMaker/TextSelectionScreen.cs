using BoardItems.BoardData;
using System;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class TextSelectionScreen : ItemSelectionScreen
    {
        [SerializeField] WordBalloon wb_prefab;
        protected override void LoadNext()
        {
            foreach (WordBalloon.balloonTypes balloonType in Enum.GetValues(typeof(WordBalloon.balloonTypes)))
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                go.Init(wb_prefab.balloonSprites[(int)balloonType]);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(balloonType.ToString()));                
            }            
        }        

        public override void OpenWork(string id)
        {
            SOWordBalloonData data = new SOWordBalloonData();
            data.id = id;
            StoryMakerEvents.AddSceneObject(data);
        }        
    }
}