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
        public BodyPart GetMirror()
        {
            return mirror;
        }
    }
}
