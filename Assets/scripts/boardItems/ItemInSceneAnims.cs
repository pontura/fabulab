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
        Vector3 finalRot;
        Vector3 finalPos;
        Vector3 _goto;

        public void Appear(ItemData itemData)
        {
            Vector3 to = itemData.position;
            this.finalPos = to;
            this.finalRot = itemData.rotation;

            Vector3 from = itemData.position;
            from.y -= 10;
            transform.localPosition = from;

            Vector3 rotateFrom = itemData.position;

            if(Random.Range(0,10)<5)
                rotateFrom.z -= 30;
            else
                rotateFrom.z += 30;

            transform.localEulerAngles = rotateFrom;

            anim = anims.APPEAR;
        }
        void Update()
        {
            if (anim == anims.OFF)
                return;
            Transform t = transform;
            float r = Mathf.Lerp(t.localEulerAngles.z, finalRot.z, 0.1f);
            Vector3 g = Vector3.Lerp(t.localPosition, finalPos, 0.1f);

            t.localPosition = g;
            t.localEulerAngles = new Vector3(0,0,r);

            if (Vector3.Distance(t.localPosition, finalPos) <0.1)
            {
                t.localEulerAngles = finalRot;
                t.localPosition = finalPos;
                anim = anims.OFF;
            }
        }
    }
}
