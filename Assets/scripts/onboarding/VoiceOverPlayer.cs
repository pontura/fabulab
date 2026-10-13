using UnityEngine;
namespace OnBoarding
{
    public class VoiceOverPlayer : MonoBehaviour
    {
        [SerializeField] AudioClip audio;

        void Start()
        {
            if (audio == null) return;
            AudioManager.Instance.PlayVoice(audio);
        }
    }
}
