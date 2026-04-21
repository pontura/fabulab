using BoardItems.BoardData;
using System;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace UI.MainApp.Home.User
{
    public class TextSelectionScreen : ItemSelectionScreen
    {
        [SerializeField] WordBalloon wballon_prefab;
        [SerializeField] WordBox wbox_prefab;
        protected override void LoadNext()
        {
            foreach (WordBalloon.balloonTypes balloonType in Enum.GetValues(typeof(WordBalloon.balloonTypes)))
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                go.Init(wballon_prefab.balloonIcons[(int)balloonType]);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWordBalloon(balloonType.ToString()));                
            }

            foreach (WordBox.boxTypes boxType in Enum.GetValues(typeof(WordBox.boxTypes))) {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                go.Init(wbox_prefab.boxIcons[(int)boxType]);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWordBox(boxType.ToString()));
            }
        }        

        void OpenWordBalloon(string id)
        {
            SOWordBalloonData data = new SOWordBalloonData();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
            StoryMakerEvents.AddSceneObject(data);
            Invoke(nameof(OpenTextInput), 2 * Time.deltaTime);
        }

        void OpenWordBox(string id) {
            SOWordBoxData data = new SOWordBoxData();
            data.id = id;
            data.itemName = Utils.GetUniqueDateTimeId();
            StoryMakerEvents.AddSceneObject(data);
            Invoke(nameof(OpenTextInput), 2 * Time.deltaTime);
        }

        void OpenTextInput() {
            StoryMakerEvents.OnInputField((ObjectSignal)Scenario.Instance.sceneObejctsManager.selected);
        }
    }
}