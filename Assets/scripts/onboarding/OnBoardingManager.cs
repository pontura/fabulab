using System;
using System.Linq;
using UnityEngine;

namespace OnBoarding
{
    public class OnBoardingManager : MonoBehaviour
    {
        public enum steps
        {
            name,
            title_character,
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
            Events.OnBoardingDone += OnBoardingDone;
            onboardingSequenceID = PlayerPrefs.GetInt("onboardingSequenceID", 0);
            Next();
        }
        void Oestroy()
        {
            Events.OnBoardingDone -= OnBoardingDone;            
        }

        private void OnBoardingDone(steps steps)
        {
            Next();
        }

        void Next()
        {            
            steps s = GetStep();
            if(s != steps.ready) 
            {
                Events.OnBoarding(s);
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
