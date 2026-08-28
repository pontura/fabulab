using UnityEngine;

public class GameSelector : MonoBehaviour
{
    public void Show(bool isOn)
    {
        gameObject.SetActive(isOn);
        if(isOn)
        {
           // GameData gd = Data.Instance.gamesManager.ActiveGame;
        }
    }
}
