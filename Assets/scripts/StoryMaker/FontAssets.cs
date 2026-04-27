using UnityEngine;
using System.Collections.Generic;

namespace Yaguar.StoryMaker.Editor
{
    [System.Serializable]
    public class Fonts
    {
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public Sprite sprite { get; private set; }
        [field:SerializeField] public TMPro.TMP_FontAsset fontAsset{  get; private set; }
    }

    [CreateAssetMenu(fileName = "FontAssets", menuName = "YaguarLib/Scriptable Objects/FontAssets")]
    public class FontAssets : ScriptableObject
    {
        [field: SerializeField] public List<Fonts> fonts { get; private set; }


        public Fonts GetFont(int id) {
            Fonts bglp = fonts.Find(x => x.Id == id);
            if (bglp == null) {
                Debug.Log("Cant find BgLigthtingPallete with id " + id);
                bglp = fonts.Find(x => x.Id == 0);
            }
            return bglp;
        }        

    }
}