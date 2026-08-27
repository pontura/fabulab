using UnityEngine;

namespace UI.MainApp.Home
{
    public class GameButton : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_Text titleField;
        [SerializeField] TMPro.TMP_Text textField;
        System.Action<GameData> OnPlay;
        GameData gameData;
        public void Init(GameData gameData, System.Action<GameData> OnPlay )
        {
            this.gameData = gameData;
            this.OnPlay = OnPlay;
            titleField.text = gameData.title;
            textField.text = gameData.description;
        }
        public void OnClicked()
        {
            OnPlay(gameData);
        }
    }
}
