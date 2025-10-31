using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yaguar.Auth;

public class LoginManager : MonoBehaviour
{
    [SerializeField] TMPro.TMP_InputField usernameField;
    [SerializeField] TMPro.TMP_InputField emailField;
    [SerializeField] TMPro.TMP_InputField passField;
    [SerializeField] TMPro.TextMeshProUGUI error;
    public bool isNew;

    //public override void OnEnabled()
    void Start() {

        FirebaseAuthManager.Instance.OnLogin += OnLogin;
        FirebaseAuthManager.Instance.OnSignUp += OnSignUp;
        FirebaseAuthManager.Instance.OnResetPassword += OnResetPassword;

        Invoke(nameof(CheckLogged), Time.deltaTime * 3);
    }

    void CheckLogged() {
        if (Data.Instance.userData.userDataInDatabase.username == "")
            isNew = true;

        if (!isNew) {
            error.text = "Bienvenide " + Data.Instance.userData.userDataInDatabase.username;
            Invoke(nameof(OnLogged), 3);
        }
    }

    private void OnDestroy() {
        FirebaseAuthManager.Instance.OnLogin -= OnLogin;
        FirebaseAuthManager.Instance.OnSignUp -= OnSignUp;
        FirebaseAuthManager.Instance.OnResetPassword -= OnResetPassword;
    }

    void OnLogin(bool succes) {
        if (succes) {
            error.text = "Conectado...";
            Invoke(nameof(OnLogged), 3);
        } else
            error.text = "La direcci�n de correo electr�nico o la contrase�a son incorrectas";
    }
    void OnSignUp(bool succes) {
        if (succes) {
            error.text = "Usuario registrado correctamente";
            Invoke(nameof(OnLogged), 3);
        } else
            error.text = "Ocurri� un error al registrar el usuario, intentalo nuevamente m�s tarde";
    }

    void OnResetPassword(bool succes) {
        if (succes) {
            error.text = "Hemos env�ado un link a " + emailField.text + " para que actualices tu contrase�a";
        } else
            error.text = "La direcci�n de correo electr�nico es incorrecta";
    }

    void ResetRegisterFields() {
        usernameField.text = "";
        emailField.text = "";
        passField.text = "";
        error.text = "";
    }

    public void OnLogged() {
        gameObject.SetActive(false);
    }
    

    public void Register() {
        CancelInvoke();
        if (usernameField.text == "")
            error.text = "Complet� tu nombre de usuario";
        else if (!emailField.text.Contains("@") || !emailField.text.Contains("."))
            error.text = "El correo electr�nico parece inv�lido";
        else if (passField.text.Length < 6)
            error.text = "La contrase�a debe tener al menos 6 caracteres";
        else {
            error.text = "Enviando datos...";



            //Data.Instance.firebaseAuthManager.SignUpUserAnon();
            FirebaseAuthManager.Instance.SignUpUserEmail(usernameField.text, emailField.text, passField.text);

        }
    }

    public void Login() {
        CancelInvoke();
        error.text = "Enviando datos...";

        FirebaseAuthManager.Instance.LoginUserByEmail(emailField.text, passField.text);

    }

    public void Reset() {
        CancelInvoke();
        error.text = "Enviando datos...";

        FirebaseAuthManager.Instance.PasswordReset(emailField.text);

    }

}
