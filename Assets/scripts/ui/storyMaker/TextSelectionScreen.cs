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
            Events.OnLoadingParent(transform, LoadNext);
            foreach (Transform child in worksContainer) {
                if (child.tag != "Persistent")
                    Destroy(child.gameObject);
            }
        }

        protected override void LoadNext()
        {
            foreach (WordBalloon.balloonTypes balloonType in Enum.GetValues(typeof(WordBalloon.balloonTypes)))
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                go.Init(wballon_prefab.balloonIcons[(int)balloonType]);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWordBalloon(balloonType.ToString()));                
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