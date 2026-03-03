using System.Collections.Generic;
using UnityEngine;

namespace UI.MainApp
{
    public class CreatorsList : MonoBehaviour
    {
        [SerializeField] Transform container;
        [SerializeField] ProfilePicture profilePicture;

        public void Init(List<string> uids)
        {
            if (uids == null) return;
            print("uids count: " + uids.Count);
            foreach(string uid in uids)
            {
                print("uid: " + uid);
                ProfilePicture p = Instantiate(profilePicture, container);
                p.Init(uid);
            }
        }
    }

}