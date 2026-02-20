using UnityEngine;

public class CharacterAnims : MonoBehaviour
{
    Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public enum anims
    {
        edit,
        idle,
        walk_right,
        idle_2,
        idle_3,
        robotIdle,
        groovyWalk_right,
        robotWalk_right,
        waveHand,
        dance_1,
        dance_2,
        hurra,
        shake,
        spin
    }
    public void Play(anims animName)
    {
        anim.Play(animName.ToString());
    }
    public void Play(string animName)
    {
        anim.Play(animName);
    }
}
