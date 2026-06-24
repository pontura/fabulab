using System;
using System.Collections;
using UI;
using UI.MainApp;
using UI.MainApp.Home.User;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

namespace OnBoarding
{
    public class OnBoarding_Bg : OnBoarding_GenericText
    {
        public TMPro.TMP_Text field2;
        [SerializeField] GameObject part1;
        [SerializeField] GameObject part2;

        [SerializeField] StoryEditorScreen storyEditorScreen;
        [SerializeField] BackgroundSelectionScreen backgroundSelectionScreen;

        public override void OnShow()
        {     
            Events.OnLoadingParent(null, OnLoadingDone);
            Events.OnLoading(true);            
        }
        void OnLoadingDone()
        {
            field.text = "Empecemos eligiendo un fondo";
            field2.text = "Lindo, no? Ahora pongamos algún objeto";

            part1.gameObject.SetActive(true);
            part2.gameObject.SetActive(false);

            UIManager.Instance.NewStory();            

            StartCoroutine(AddCharacterToScene());
            Events.OnBoardingStepDone += OnBoardingStepDone;
        }
        private void OnBoardingStepDone(OnBoardingManager.steps steps)
        {
            if(active && steps != step) return;
            Step2();
            Events.OnBoardingStepDone -= OnBoardingStepDone;
        }

        void OnDestroy()
        {            
            Events.OnBoardingStepDone -= OnBoardingStepDone;
        }
        IEnumerator AddCharacterToScene()
        {
            StoryMakerEvents.SetEditing(true);
            yield return new WaitForSeconds(0.25f);
            storyEditorScreen.OnTabClicked(0);
            //TO-DO - agregar el character recien creado:
            string newCharacterID =  "-OsbnGzN2MMES--Y3_QJ";
            print("OpenWork " + newCharacterID);
            SOAvatarFabulabData data = new SOAvatarFabulabData();
            data.id = newCharacterID;
            data.itemName = Utils.GetUniqueDateTimeId();
            
            StoryMakerEvents.AddSceneObject(data); 

            Data.Instance.charactersData.ResetCurrentID();
            StoryMakerEvents.SetEditing(true);  

            yield return new WaitForSeconds(0.25f);
            
            backgroundSelectionScreen.Clicked(1);
        }
        void Step2()
        {            
            part1.gameObject.SetActive(false);
            part2.gameObject.SetActive(true);
        }
        public override void ShowPanelsBack() {}
        public override void OnHide()
        {
        }
    }
}
