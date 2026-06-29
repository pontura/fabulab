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
            foreach(OnBoardingMain go in onboardingScreens)
                go.Init();  
            Events.OnBoardingDone += OnBoardingDone;
            onboardingSequenceID = PlayerPrefs.GetInt("onboardingSequenceID", 0);
            
            if(Data.Instance.userData.onboardingSteps >0) return;
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
            if(s != steps.ready) 
            {
                Events.OnBoarding(s);
            }
            else
            {
                Data.Instance.userData.OnBoardingAllStepsDone();
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
