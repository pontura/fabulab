using UnityEngine;

namespace Common.UI
{
    public class TabButton : MonoBehaviour
    {
        TabController tabController;
        [SerializeField] Animator anim;
        public int id;

        public void Init(TabController tabController, int id)
        {
            this.id = id;
            this.tabController = tabController;
            Reset();
        }
        public void Clicked()
        {
            print("clicked");
            if (tabController != null)
                tabController.Clicked(this);
        }
        public void Reset()
        {
            anim.SetBool("active", false);
        }
        public void SetActive()
        {
            anim.SetBool("active", true);
        }
        public virtual void OnActive() { }
    }
}
