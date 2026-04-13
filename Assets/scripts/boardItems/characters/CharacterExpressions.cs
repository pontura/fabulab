using UnityEngine;

public class CharacterExpressions : MonoBehaviour
{
    [SerializeField] Animator anim;

    public void Play(string animName)
    {
        Debug.Log("Play Emoji: " + animName);

        if (animName == "" || animName ==null)
            animName = Data.Instance.characterAnimsManager.defaultEmoji.name;
        if (anim.HasState(0, Animator.StringToHash(animName)))
            anim.Play(animName);
        else if (int.TryParse(animName, out int index)) {
            string aName = Data.Instance.characterAnimsManager.all[index].clip.name;
            Debug.Log("& anim name: " + aName);
            anim.Play(aName);
        }
    }
    public void Stop() {
        anim.speed = 0;
    }
}
