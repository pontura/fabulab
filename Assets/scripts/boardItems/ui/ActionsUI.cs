using System;
using UnityEditorInternal;
using UnityEngine;
namespace BoardItems.UI
{
    public class ActionsUI : MonoBehaviour
    {
        [SerializeField] int characterEditorID = 0;
        public void Clicked(int id)
        {
            CharacterAnims.anims anim = (CharacterAnims.anims)id;
            Events.EditMode(anim == CharacterAnims.anims.edit);
            Events.OnCharacterAnim(characterEditorID, anim);
        }
    }
}
