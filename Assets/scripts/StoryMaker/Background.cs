using BoardItems.SceneObjects;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class Background : Objeto
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

        }        
    }        
}
