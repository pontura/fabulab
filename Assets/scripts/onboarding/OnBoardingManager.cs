using Firebase.Analytics;
using System;
using UI;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoardingManager : MonoBehaviour
    {
        [SerializeField] OnBoardingMain[] onboardingScreens;
        public enum steps
        {
            name,
            title_character,
            character_intro,
            choose_head,    
            modify_head,
            move,
            allBody,
            characterDone,
            firstStoryIntro,
            background,
            objects,
            storyPresentation,
            storyPresentationDone,
            ready
        }
        [SerializeField] int id;
        [SerializeField] int onboardingSequenceID; 
        public Sequences[] sequences;
        [Serializable] public class Sequences
        {
            public steps[] steps;
        }
        void Start()
        {
            //PlayerPrefs.DeleteKey("onboardingSteps");
            //PlayerPrefs.DeleteKey("onboardingSequenceID");            

            foreach (OnBoardingMain go in onboardingScreens)
                go.Init();  
            Events.OnBoardingDone += OnBoardingDone;
            if(Data.Instance.userData.onboardingSteps >0) return;
            Reset();
        }
        public void Reset()
        {
            print("Reset");
            onboardingSequenceID = 0;
            id = 0;
            onboardingSequenceID = PlayerPrefs.GetInt("onboardingSequenceID", 0);

            FirebaseAnalytics.LogEvent("onboarding_start");

            Next();
        }
        
        void Oestroy()
        {
            Events.OnBoardingDone -= OnBoardingDone;            
        }

        private void OnBoardingDone(steps step)
        {
            if(step == steps.title_character)
                UIManager.Instance.NewCharacter();
            Next();
        }

        void Next()
        {            
            steps s = GetStep();
            print(s);
            FirebaseAnalytics.LogEvent(
                "onboarding_step",
                new Parameter("step_id", (int)s),
                new Parameter("step_name", s.ToString())
            );
            if (s != steps.ready) 
            {
                Events.OnBoarding(s);
            }
            else
            {
                Data.Instance.userData.OnBoardingAllStepsDone();
                FirebaseAnalytics.LogEvent("onboarding_complete");
            }

            
        }
        steps GetStep()
        {
             if(onboardingSequenceID>=sequences.Length) return steps.ready;
            Sequences s = sequences[onboardingSequenceID];            
            if(id>=s.steps.Length) 
            {
                id = 0;
                onboardingSequenceID++;
                return GetStep();
            }
            else{
                steps step  = s.steps[id];                
                id++;
                return step;
            }
        }
    }
}
