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
        FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
        FirebaseAuthManager.Instance.OnSignUp += OnSignUp;
        FirebaseAuthManager.Instance.OnResetPassword += OnResetPassword;

        Invoke(nameof(CheckLogged), Time.deltaTime * 3);
    }

    void CheckLogged() {
        if (Data.Instance.userData.userDataInDatabase.username == "")
            isNew = true;

        if (!isNew) {
            error.text = "Bienvenide " + Data.Instance.userData.userDataInDatabase.username;
            Invoke(nameof(OnLogged), Time.deltaTime * 3);
        }
    }

    private void OnDestroy() {
        FirebaseAuthManager.Instance.OnLogin -= OnLogin;
        FirebaseAuthManager.Instance.OnSignUp -= OnSignUp;
        FirebaseAuthManager.Instance.OnResetPassword -= OnResetPassword;
        FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
    }

    void OnLogin(bool succes) {

        print("OnLogin " + succes);

        if (succes) {
            error.text = "Conectado...";
            Invoke(nameof(OnLogged), Time.deltaTime*3);
        } else
            error.text = "La dirección de correo electrónico o la contraseña son incorrectas";
    }
    void OnSignUp(bool succes) {
        if (succes) {
            error.text = "Usuario registrado correctamente";
            Invoke(nameof(OnLogged), 1);
        } else
            error.text = "Ocurrió un error al registrar el usuario, intentalo nuevamente más tarde";
    }

    void OnResetPassword(bool succes) {
        if (succes) {
            error.text = "Hemos envíado un link a " + emailField.text + " para que actualices tu contraseña";
        } else
            error.text = "La dirección de correo electrónico es incorrecta";
    }

    void OnTokenUpdated() {
        Invoke(nameof(OnLogged), Time.deltaTime * 3);
    }

    void ResetRegisterFields() {
        usernameField.text = "";
        emailField.text = "";
        passField.text = "";
        error.text = "";
    }

    public void OnLogged() {
        CancelInvoke("OnLogged");
        print("logged");
        gameObject.SetActive(false);
    }
    

    public void Register() {
        CancelInvoke();
        if (usernameField.text == "")
            error.text = "Completá tu nombre de usuario";
        else if (!emailField.text.Contains("@") || !emailField.text.Contains("."))
            error.text = "El correo electrónico parece inválido";
        else if (passField.text.Length < 6)
            error.text = "La contraseña debe tener al menos 6 caracteres";
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
