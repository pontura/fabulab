using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ParentalGate : MonoBehaviour
    {
        public GameObject panel;
        System.Action<bool> OnDone;
        public InputField inputField;
        public Text texto;
        public Text respuesta;
        public GameObject okBtn;
        int result;
        bool isCorrect;

        void Start()
        {
            Events.OnParentalGate += OnParentalGate;
            SetOff();
        }
        private void OnDestroy()
        {
            Events.OnParentalGate -= OnParentalGate;
        }
        void OnParentalGate(System.Action<bool> OnDone)
        {
            isCorrect = false;
            inputField.text = "";
            int a = 3 + (int)Mathf.Floor(Random.value * 5);
            int b = 6 + (int)Mathf.Floor(Random.value * 4);
            result = a * b;
            texto.text = "¿Cuánto es " + a + "x" + b + "?";
            this.OnDone = OnDone;
            respuesta.text = "";
            panel.SetActive(true);
            okBtn.SetActive(true);
        }
        public void OnClick()
        {
            if (inputField.text != "")
            {
                if (int.Parse(inputField.text) == result)
                {
                    respuesta.text = "Muy bien, el cálculo es correcto";
                    isCorrect = true;
                    //OnDone(true);
                    //Events.OnSimplePopup("Muy Bien!", "El cálculo es correcto");
                }
                else
                {
                    respuesta.text = "Incorrecto, preguntale a adultos";
                    isCorrect = false;
                    //OnDone(false);
                    //Events.OnSimplePopup("Incorrecto", "Preguntale a tus padres");
                }
                okBtn.SetActive(false);
                Invoke("ParentalDone", 2);
            }
        }

        void ParentalDone()
        {
            OnDone(isCorrect);
            SetOff();
        }

        public void SetOff()
        {
            CancelInvoke();
            panel.SetActive(false);
        }
    }
}
