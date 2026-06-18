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

        public override void Show(bool isOn) {
            Init();
        }

        protected override void Init() {
           // Events.OnLoadingParent(transform, LoadNext);
            foreach (Transform child in worksContainer) {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }
            LoadNext();
        }

        protected override void LoadNext()
        {
            foreach (WordBalloon.balloonTypes balloonType in Enum.GetValues(typeof(WordBalloon.balloonTypes)))
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                print(balloonType + " go " + go);
                int a = (int)balloonType;
                Sprite s = wballon_prefab.balloonIcons[a];
                go.Init(balloonType.ToString(), s);
                go.AddOnClick(OpenWordBalloon);
            }
            Events.OnLoading(false);
        }        

        void OpenWordBalloon(string id)
        {
            SOWordBalloonData data = new SOWordBalloonData();
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