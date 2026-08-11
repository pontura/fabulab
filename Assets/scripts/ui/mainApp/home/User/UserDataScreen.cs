using UnityEngine;
using Yaguar.Auth;
using Yaguar.StoryMaker.DB;

namespace UI.MainApp.Home.User
{
    public class UserDataScreen : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_InputField TMP_InputField;

        public void Show(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn)
            {
                string username = Data.Instance.userData.userDataInDatabase.username;
                TMP_InputField.text = username;
            }
        }
        public void EditName()
        {
            string name = TMP_InputField.text;
            if (string.IsNullOrEmpty(name))
                return;
            ChangeName(name);
        }
        void ChangeName(string name)
        {
            FirebaseStoryMakerDBManager.Instance.UpdateUsername(name,(success) => {
                if (success) 
                    Events.ChangeName(name);
            });            
        }
        public void Logout()
        {
            FirebaseAuthManager.Instance.SignOut();
        }

        public void DeleteAccount() {
            AudioManager.Instance.uiSfxManager.PlayTransp("click", -5);
            Events.OnConfirm("¿Estás seguro de que querés borrar tu cuenta?", "SI", "NO", OnConfirm);
        }
        void OnConfirm(bool ok) {
            if (ok) {
                Events.OnConfirm("En el caso de continuar se borrará de manera irreversible tu usuario y todo lo que hayas guardado en la aplicación.", "CONTINUAR", "CANCELAR", OnReConfirm);
            }
        }
        void OnReConfirm(bool ok) {
            if (ok) {
                Events.OnConfirm("En el caso de continuar se borrará de manera irreversible tu usuario y todo lo que hayas guardado en la aplicación.", "CONTINUAR", "CANCELAR", OnConfirm);
                FirebaseStoryMakerDBManager.Instance.DeleteAccount();
            }
        }
        void OnDeleted(string filmId) {
            Data.Instance.scenesData.RemoveFD(filmId);
            Events.OnFilmMetadataRemoved(filmId);
            Destroy(gameObject);
        }
    }
}