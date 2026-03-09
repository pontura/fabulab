using BoardItems.SceneObjects;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class Prop : Objeto
    {
        protected GameObject asset;
        [SerializeField] GameObject prop_to_instantiate;
        [field: SerializeField] public SceneObjectManager objectManager { private set; get; }
        
        public override void OnInit() {


            if (asset != null)
                Destroy(asset.gameObject);

            asset = Instantiate(prop_to_instantiate, transform);
            //asset.transform.SetParent(transform);
            asset.transform.localEulerAngles = Vector3.zero;
            asset.transform.localPosition = Vector3.zero;
            asset.SetActive(true);
            objectManager = asset.GetComponent<SceneObjectManager>();
            objectManager.Init();

            Invoke("Delayed", 0.1f);
        }
        void Delayed() {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>()) {
                bounds.Encapsulate(sr.bounds);
            }

            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
            Vector3 localSize = transform.InverseTransformVector(bounds.size);

            collider.offset = localCenter;
            collider.size = localSize;
        }
    }        
}
