using UnityEngine;

namespace UI.MainApp
{
    public class BGColorBtn : MonoBehaviour
    {
        public void OnClicked()
        {
            Events.OnBGColorizerOpen(true);
        }
    }
}
