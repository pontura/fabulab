using UnityEngine;

namespace BoardItems
{
    public class ItemInSceneAnims : MonoBehaviour
    {
        anims anim;
        public enum anims
        {
            OFF,
            APPEAR,
            DISAPPEAR
        }
        Vector3 _goto;

        public void Appear(Vector3 _from, Vector3 _goto)
        {
            transform.localPosition = _from;
            this._goto = _goto;
            anim = anims.APPEAR;
        }
        void Update()
        {
            if (anim == anims.OFF)
                return;
            Transform t = transform;
            Vector3 g = Vector3.Lerp(t.localPosition, _goto, 0.1f);
            t.localPosition = g;
            if(Vector3.Distance(t.localPosition, _goto)<0.1)
            {
                t.localPosition = _goto;
                anim = anims.OFF;
            }
        }
    }
}
