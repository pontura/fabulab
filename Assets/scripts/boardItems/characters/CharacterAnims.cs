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
        anim.Play(animName);
    }
    public void Idle()
    {
        Play("idle");
    }
    public void Stop() {
        anim.StopPlayback();
    }
}
