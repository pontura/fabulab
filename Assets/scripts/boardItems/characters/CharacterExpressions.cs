using UnityEngine;

public class CharacterExpressions : MonoBehaviour
{
    [SerializeField] Animator anim;

    public void Play(string animName)
    {
        anim.Play(animName);
    }
}
