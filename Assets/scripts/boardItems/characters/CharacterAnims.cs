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
        waveHand
    }
    public void Play(anims animName)
    {
        anim.Play(animName.ToString());
    }
}
