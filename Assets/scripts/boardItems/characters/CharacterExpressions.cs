using UnityEngine;

public class CharacterExpressions : MonoBehaviour
{
    [SerializeField] Animator anim;

    public enum expressions
    {
        Anger,
        Crazy,
        Desperate,
        Displeasure,
        evilSmile,
        Holy,
        Insulted,
        knockOut,
        Lol,
        Malfunction,
        shySmile,
        Smile1,
        Smile2,
        Thunderstruck,
        Wow
    }
    public void Play(expressions animName)
    {
        anim.Play(animName.ToString());
    }
}
