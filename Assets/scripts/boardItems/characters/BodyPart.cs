using BoardItems.Characters;
using UnityEngine;

namespace BoardItems
{
    public class BodyPart : MonoBehaviour
    {
        Collider2D col2D;
        public CharacterData.parts part;
        [SerializeField] BodyPart mirror;

        void Start()
        {
            col2D = GetComponentInChildren<Collider2D>();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            GameObject go = collision.gameObject;
            ItemInScene iis = go.GetComponent<ItemInScene>();
            if (iis != null)
            {
                print("enter: " + go.name);
                iis.data.SetCharacterPart(part);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            GameObject go = collision.gameObject;
            ItemInScene iis = go.GetComponent<ItemInScene>();
            if (iis != null)
            {
                iis.data.OutOfBody();
                if (iis.data.part == part)
                    iis.data.SetCharacterPart(CharacterData.parts.none);
            }
        }
    }
}
