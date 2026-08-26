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
            Events.OnConfirm("�Est�s seguro de que quer�s borrar tu cuenta?", "SI", "NO", OnConfirm);
        }
        void OnConfirm(bool ok) {
            if (ok) {
                Invoke(nameof(LastConfirmation), Time.deltaTime * 3);
            }
        }

        void LastConfirmation() {
            Events.OnConfirm("En el caso de continuar se borrar� de manera irreversible tu usuario y todo lo que hayas guardado en la aplicaci�n.", "CONTINUAR", "CANCELAR", OnReConfirm);
        }

        void OnReConfirm(bool ok) {
            if (ok) {
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