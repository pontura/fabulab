using UnityEngine;
using Yaguar.Auth;

public class LoginManager : MonoBehaviour
{
    [SerializeField] SocialAuth socialAuth;
    [SerializeField] GameObject introContainer;
    [SerializeField] GameObject emailContainer;
    [SerializeField] GameObject registerPopup;
    [SerializeField] TMPro.TextMeshProUGUI title;
    [SerializeField] TMPro.TMP_InputField usernameField;
    [SerializeField] TMPro.TMP_InputField emailField;
    [SerializeField] TMPro.TMP_InputField passField;
    [SerializeField] TMPro.TextMeshProUGUI introFeedback;
    [SerializeField] TMPro.TextMeshProUGUI emailFeedback;
    [SerializeField] TMPro.TextMeshProUGUI popupFeedback;
    [SerializeField] GameObject loginLink, resetLink, signUpLink;
    [SerializeField] GameObject loginBtn, signUpBtn, resetBtn;
    public bool isNew;

    bool isToSyncUserToEmail;
    TMPro.TextMeshProUGUI feedback;

    //public override void OnEnabled()
    void Start() {

        FirebaseAuthManager.Instance.OnLogin += OnLogin;
        FirebaseAuthManager.Instance.OnTokenUpdated += OnTokenUpdated;
        FirebaseAuthManager.Instance.OnSignUp += OnSignUp;
        FirebaseAuthManager.Instance.OnSignedOut += OnSignedOut;
        FirebaseAuthManager.Instance.OnResetPassword += OnResetPassword;
        Events.ShowRegisterPopup += ShowRegisterPopup;

        //feedback = introFeedback;

        Invoke(nameof(CheckLogged), Time.deltaTime * 3);
    }

    void CheckLogged() {
        if (Data.Instance.userData.userDataInDatabase.username == "")
            isNew = true;

        ToRegister();

        if (!isNew) {
            feedback.text = "Bienvenide " + Data.Instance.userData.userDataInDatabase.username;
            Invoke(nameof(OnLogged), Time.deltaTime * 3);
        }
    }

    private void OnDestroy() {
        FirebaseAuthManager.Instance.OnLogin -= OnLogin;
        FirebaseAuthManager.Instance.OnSignUp -= OnSignUp;
        FirebaseAuthManager.Instance.OnSignedOut -= OnSignedOut;
        FirebaseAuthManager.Instance.OnResetPassword -= OnResetPassword;
        FirebaseAuthManager.Instance.OnTokenUpdated -= OnTokenUpdated;
        Events.ShowRegisterPopup -= ShowRegisterPopup;
    }

    private void OnSignedOut()
    {
        ResetRegisterFields();
        introContainer.SetActive(true);
    }

    void OnLogin(bool succes) {

        print("OnLogin " + succes);

        if (succes) {
            feedback.text = "Conectado...";
            Invoke(nameof(OnLogged), Time.deltaTime*3);
        } else
            feedback.text = "La dirección de correo electrónico o la contraseña son incorrectas";
    }
    void OnSignUp(bool succes) {
        if (succes) {
            feedback.text = "Usuario registrado correctamente";
            Invoke(nameof(OnLogged), 1);
        } else
            feedback.text = "Ocurrió un error al registrar el usuario, intentalo nuevamente más tarde";
    }

    void OnResetPassword(bool succes) {
        if (succes) {
            feedback.text = "Hemos envíado un link a " + emailField.text + " para que actualices tu contraseña";
        } else
            feedback.text = "La dirección de correo electrónico es incorrecta";
    }

    void OnTokenUpdated() {
        Debug.Log("#OnTokenUpdated");
        Invoke(nameof(OnLogged), Time.deltaTime * 3);
    }

    public void ShowEmailAuth() {
        registerPopup.SetActive(false);
        introContainer.SetActive(false);
        emailContainer.SetActive(true);
        feedback = emailFeedback;
    }

    public void SignUpWithPlayGames() {
        feedback = introFeedback;
        socialAuth.Init((authCode) => {
            Debug.Log("#socialAuth: " + authCode);
            if (authCode != "") {
                FirebaseAuthManager.Instance.SignInWithPlayGames(authCode, (success) => {
                    if (!success) {
                        feedback.text = "No fue posible ingresar con Play Games.";
                    } else {
                        introContainer.SetActive(false);
                    }
                });
            } else {
                feedback.text = "No fue posible ingresar con Play Games.";
            }
        });
    }

    public void SyncUserWithPlayGames() {
        feedback = popupFeedback;
        socialAuth.Init((authCode) => {
            Debug.Log("#socialAuth: " + authCode);
            if (authCode != "") {                
                FirebaseAuthManager.Instance.LinkWithPlayGames(socialAuth.GetLocalUser(), authCode, (success) => {
                    if (!success) {
                        feedback.text = "No fue posible linkear la cuenta con Play Games.";
                    } else {
                        registerPopup.SetActive(false);
                    }
                });
            } else {
                feedback.text = "No fue posible linkear la cuenta con Play Games.";
            }
        });
    }

    public void SignInAnonymously() {
        feedback = introFeedback;
        FirebaseAuthManager.Instance.SignInAnonymously();
    }

    void ResetRegisterFields() {
        //usernameField.text = "";
        emailField.text = "";
        passField.text = "";
        emailFeedback.text = "";
    }

    public void OnLogged() {
        CancelInvoke("OnLogged");
        print("logged");
        ResetRegisterFields();

        registerPopup.SetActive(false);
        introContainer.SetActive(false);
        emailContainer.SetActive(false);
    }
    

    public void Register() {
        CancelInvoke();
        if (usernameField.text == "")
            feedback.text = "Completá tu nombre de usuario";
        else if (!emailField.text.Contains("@") || !emailField.text.Contains("."))
            feedback.text = "El correo electrónico parece inválido";
        else if (passField.text.Length < 6)
            feedback.text = "La contraseña debe tener al menos 6 caracteres";
        else {
            feedback.text = "Enviando datos...";



            //Data.Instance.firebaseAuthManager.SignUpUserAnon();
            if(isToSyncUserToEmail)             
                FirebaseAuthManager.Instance.LinkWithEmail(usernameField.text, emailField.text, passField.text);
            else
                FirebaseAuthManager.Instance.SignUpUserEmail(usernameField.text, emailField.text, passField.text);

        }
    }

    public void Login() {
        CancelInvoke();
        feedback.text = "Enviando datos...";

        FirebaseAuthManager.Instance.LoginUserByEmail(emailField.text, passField.text);

    }

    public void Reset() {
        CancelInvoke();
        feedback.text = "Enviando datos...";

        FirebaseAuthManager.Instance.PasswordReset(emailField.text);

    }

    public void ToSyncUserToEmail() {                
        ToRegister();
        usernameField.text = Data.Instance.userData.userDataInDatabase.username;
        loginLink.SetActive(false);
        resetLink.SetActive(false);
        signUpBtn.SetActive(true);
        isToSyncUserToEmail = true;
        ShowEmailAuth();
    }

    public void ToRegister() {
        isToSyncUserToEmail = false;
        ResetRegisterFields();
        title.text = "Completá tus datos para registrarte";
        loginBtn.SetActive(false);
        resetBtn.SetActive(false);
        signUpBtn.SetActive(true);
        usernameField.gameObject.SetActive(true);
        emailField.gameObject.SetActive(true);
        passField.gameObject.SetActive(true);
        loginLink.SetActive(true);
        resetLink.SetActive(true);
        signUpLink.SetActive(false);
    }

    public void ToReset() {
        ResetRegisterFields();
        title.text = "Si no recordás tu contraseña completá tu correo electrónico";
        loginBtn.SetActive(false);
        signUpBtn.SetActive(false);
        resetBtn.SetActive(true);
        usernameField.gameObject.SetActive(false);
        emailField.gameObject.SetActive(true);
        passField.gameObject.SetActive(false);
        loginLink.SetActive(true);
        resetLink.SetActive(false);
        signUpLink.SetActive(true);
    }

    public void ToLogin() {
        ResetRegisterFields();
        title.text = "Si ya estás registrado completá tus datos para ingresar";
        signUpBtn.SetActive(false);
        resetBtn.SetActive(false);
        loginBtn.SetActive(true);
        usernameField.gameObject.SetActive(false);
        emailField.gameObject.SetActive(true);
        passField.gameObject.SetActive(true);
        loginLink.SetActive(false);
        resetLink.SetActive(true);
        signUpLink.SetActive(true);
    }

    void ShowRegisterPopup() {
        registerPopup.SetActive(true);
    }

    public void CloseRegisterPopup() {
        registerPopup.SetActive(false);
    }

}
