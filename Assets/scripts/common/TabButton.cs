using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public class TabButton : MonoBehaviour
    {
        TabController tabController;
        [SerializeField] TMPro.TMP_Text field;
        [SerializeField] Animator anim;
        [SerializeField] Image image;
        public int id;

        public void Init(TabController tabController, int id)
        {
            this.id = id;
            this.tabController = tabController;
            Reset();
        }
        public void SetThumb(Sprite s)
        {
            image.sprite = s;
        }
        public void SetName(string name)
        {
            field.text = name;
        }
        public void Clicked()
        {
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
