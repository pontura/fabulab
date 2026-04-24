using BoardItems.SceneObjects;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class Prop : Objeto
    {
        protected GameObject asset;
        [SerializeField] GameObject prop_to_instantiate;
        [field: SerializeField] public SceneObjectManager objectManager { private set; get; }

        public BordersCreator Borders { private set; get; }

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
            Bounds worldBounds = new Bounds(transform.position, Vector3.zero);
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>()) {
                worldBounds.Encapsulate(sr.bounds);
            }
                        
            Vector3[] corners = new Vector3[8];
            Vector3 ext = worldBounds.extents;
            Vector3 cen = worldBounds.center;
            int i = 0;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                        corners[i++] = cen + Vector3.Scale(ext, new Vector3(x, y, z));
                        
            Bounds localBounds = new Bounds(transform.InverseTransformPoint(corners[0]), Vector3.zero);
            for (int j = 1; j < corners.Length; j++) {
                localBounds.Encapsulate(transform.InverseTransformPoint(corners[j]));
            }

            collider.offset = localBounds.center;
            collider.size = localBounds.size;
        }

        public override void BeginDrag() {
            base.BeginDrag();
            if (Borders == null) {
                Borders = gameObject.AddComponent<BordersCreator>();
                Borders.Init(GetComponent<BoxCollider2D>());
            }

            Borders.Show(true);
        }
    }        
}
