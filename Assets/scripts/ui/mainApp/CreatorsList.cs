using UnityEngine;

namespace UI.MainApp
{
    // Shows the creator of an item
    public class CreatorsList : MonoBehaviour
    {
        [SerializeField] Transform container;
        [SerializeField] ProfilePicture profilePicture;

        public void Init(string uid)
        {
            profilePicture.Init(uid);
        }
    }

}
