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

            field.text = "Empecemos eligiendo un fondo.";
            field2.text = "Lindo, ¿no? Ahora pongamos algún objeto.";

            part1.gameObject.SetActive(true);
            part2.gameObject.SetActive(false);
            
            Events.ShowScreen(UIManager.screenType.StoryMaker);

            Events.OnLoadingParent(null, OnLoadingDone);
            Events.OnLoading(true);            
        }
        
        void OnLoadingDone()
        {
            Events.OnLoading(true);            
            LoadStoryDefault();
        }
         void LoadStoryDefault()
        {
            string storyID = "-Oxq-0rsW82KJKF_1ZKe"; //Story por defecto para onboarding
            Data.Instance.scenesData.LoadFilm(storyID);
            UIManager.Instance.boardUI.SetEditingType(BoardUI.editingTypes.NONE);
            StoryMakerEvents.OnLoadFilm += OnLoadFilm;
        }
         void OnLoadFilm() {
            StoryMakerEvents.OnLoadFilm -= OnLoadFilm;
            StoryMakerEvents.EnableStoryEdition(true);
            StoryMakerEvents.EnableInputManager(true);
            InitBg();
        }   
         void InitBg()
        {
            Events.OnLoading(true);            
            StartCoroutine(AddCharacterToScene());
            Events.OnBoardingStepDone += OnBoardingStepDone;
        }
        private void OnBoardingStepDone(OnBoardingManager.steps steps)
        {
            StoryMakerEvents.EnableStoryEdition(true);
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
            
            string originalCharacterID = "-On3wQ6Vy9jnpMtTTgWb"; //Character por defecto en story del onboarding:
            string newCharacterID =  Data.Instance.charactersData.userCharacters[Data.Instance.charactersData.userCharacters.Count - 1].id;

            StoryMakerEvents.ReplaceSceneObjectFromTo(originalCharacterID, newCharacterID);

            // print("AddCharacterToScene lastID:" + newCharacterID);

            // SOAvatarFabulabData data = new SOAvatarFabulabData();
            // data.id = newCharacterID;
            // data.itemName = Utils.GetUniqueDateTimeId();
            
            // StoryMakerEvents.AddSceneObject(data); 

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
