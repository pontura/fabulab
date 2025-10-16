using BoardItems.Characters;
using UnityEngine;

namespace BoardItems
{
    public class BodyPart : MonoBehaviour
    {
        Collider2D col2D;
        public CharacterData.parts part;
        [SerializeField] GameObject selectedBodySignal;

        void Start()
        {
            Events.OnNewBodyPartSelected += OnNewBodyPartSelected;
            col2D = GetComponentInChildren<Collider2D>();
            SetSelection(false);
        }
        void OnDestroy()
        {
            Events.OnNewBodyPartSelected -= OnNewBodyPartSelected;
            col2D = GetComponentInChildren<Collider2D>();
        }
        void OnNewBodyPartSelected(BodyPart bp)
        {
            SetSelection(bp == this);
        }
        public void SetSelection(bool isOn)
        {
            selectedBodySignal.gameObject.SetActive(isOn);
        }
    }
}
