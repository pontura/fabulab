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
        idle,
        edit,
        walk_right,
        waveHand,
        dance_1,
        dance_2
    }
    public void Play(anims animName)
    {
        anim.Play(animName.ToString());
    }
}
