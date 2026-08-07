using UnityEngine;

public class CharacterAnims : MonoBehaviour
{
    Animator anim;
    string currentAnimName;
    string lastAnim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void Play(string animName)
    {
        if (isWaitingForCallback) {
            lastAnim = animName;
            return;
        }

        anim.speed = 1;

        //  Debug.Log("$Play Anim 1: "+animName);

        if (animName == "" || animName == null)
            animName = Data.Instance.characterAnimsManager.defaultIdle.name;

      //  Debug.Log("$Play Anim 2: " + animName);

        if (anim.HasState(0, Animator.StringToHash(animName))) {
           // Debug.Log("$HasState true");
            anim.Play(animName);
        } else if (int.TryParse(animName, out int index)) {
            string aName = Data.Instance.characterAnimsManager.all[index - 1].clip.name;
          //  Debug.Log("& anim name: " + aName);
            anim.Play(aName);
        }

        currentAnimName = animName;
    }
    public void Idle()
    {
        Play("idle");
    }
    public void Stop() {
        anim.speed = 0;
        //Debug.Log("& Speed: " + anim.speed);
    }

    bool isWaitingForCallback;
    public System.Action SetTemporalDefaultAnim() {
        lastAnim = currentAnimName;
        Play("");
        isWaitingForCallback = true;
        Invoke(nameof(ResetWaitForCallback), 2f);
        return ()=> {
            isWaitingForCallback = false;
            Play(lastAnim);
        };        
    }

    void ResetWaitForCallback() {
        isWaitingForCallback = false;
    }
}
