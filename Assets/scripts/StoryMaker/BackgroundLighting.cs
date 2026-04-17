using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Yaguar.StoryMaker.Editor
{
    [System.Serializable]
    public class BgLigthtingPalette {

        [field: SerializeField] public string Id { get; private set; }
        [field:SerializeField] public Sprite Icon {  get; private set; }
        [field: SerializeField] public List<Color> StepColors { get; private set; }
    }

    [CreateAssetMenu(fileName = "BackgroundLighting", menuName = "YaguarLib/Scriptable Objects/BackgroundLighting")]
    public class BackgroundLighting : ScriptableObject
    {
        [field: SerializeField] public List<BgLigthtingPalette> LightingPalettes { get; private set; }


        BgLigthtingPalette GetCurrentPalette() {
            string id = ScenesManagerFabulab.Instance.GetActiveScene().lightingId;
            BgLigthtingPalette bglp = LightingPalettes.Find(x => x.Id == id);
            if (bglp == null) {
                Debug.Log("Cant find BgLigthtingPallete with id " + id);
                bglp = LightingPalettes.Find(x => x.Id == "default");
            }
            return bglp;
        }        

        public int GetMaxValue() {
            BgLigthtingPalette bglp = GetCurrentPalette();
            return bglp.StepColors.Count - 1;
        }

        public Color GetStepColor(int step) {
            BgLigthtingPalette bglp = GetCurrentPalette();
            return bglp.StepColors[step];
        }
    }
}