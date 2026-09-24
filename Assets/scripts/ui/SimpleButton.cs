using BoardItems;
using BoardItems.BoardData;
using UI.MainApp;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SimpleButton : MonoBehaviour
    {
        [SerializeField] protected Image thumb;
                
        public virtual void Init(Sprite sprite) {
            thumb.sprite = sprite;
        }        
    }

}