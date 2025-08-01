using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BoardItems
{
    public class Board : MonoBehaviour
    {
        public GameObject boardWallLimit;
        public GameObject inventaryPanel;
        public GameObject itemsContainer;

        private void Update() // todo: Despues sacarlo del Update
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
            pos.y = pos.z = 0;
            boardWallLimit.transform.position = pos;

            pos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.6f, 0, 0));
            pos.y = pos.z = 0;
            inventaryPanel.transform.position = pos;

            pos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.8f, 0, 0));
            pos.y = itemsContainer.transform.position.y;
            pos.z = itemsContainer.transform.position.z;
            itemsContainer.transform.position = pos;
        }
    }

}