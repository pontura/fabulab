using UnityEngine;

public class CharacterAnims : MonoBehaviour
{
    Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void Play(string animName)
    {
        anim.speed = 1;
        if(anim.HasState(0, Animator.StringToHash(animName)))
            anim.Play(animName);
        else if(int.TryParse(animName, out int index)) {
            string aName = Data.Instance.characterAnimsManager.all[index - 1].name;
            Debug.Log("& anim name: " + aName);
            anim.Play(aName);
        }

    }
    public void Idle()
    {
        Play("idle");
    }
    public void Stop() {
        anim.speed = 0;
        Debug.Log("& Speed: " + anim.speed);
    }
}
