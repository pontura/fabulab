
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainApp.Home
{
    public class GameButton : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_Text titleField;
        [SerializeField] TMPro.TMP_Text textField;
        [SerializeField] Image thumb;

        System.Action<GameData> OnPlay;
        GameData gameData;
        public void Init(GameData gameData, System.Action<GameData> OnPlay )
        {
            this.gameData = gameData;
            this.OnPlay = OnPlay;
            titleField.text = gameData.title;
            textField.text = gameData.description;
            GameThumbnail gd = Data.Instance.gamesManager.GetThumb(gameData.id);
            if(gd != null)
                thumb.sprite = gd.thumbnail;
        }
        public void OnClicked()
        {
            OnPlay(gameData);
        }
    }
}
