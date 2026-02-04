using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

namespace UI
{
    public class SharePanel : MonoBehaviour
    {
        [Serializable]
        public class Data
        {
            public string username;
            public string emailTo;
        }
        public Text titleField;
        public GameObject panel;
        public InputField nameField;
        public InputField otherEmailField;
        public Text responseField;
        System.Action<Data> OnDone;
        bool sending;

        void Start()
        {
            panel.SetActive(false);
            titleField.text = "Compartir";
            responseField.text = "";
        }
        bool toPakaPaka;
        public void Init(System.Action<Data> OnDone, bool toPakaPaka)
        {
            sending = false;
            panel.SetActive(true);
            this.toPakaPaka = toPakaPaka;
            this.OnDone = OnDone;
            nameField.text = PlayerPrefs.GetString("username", "");
            otherEmailField.text = PlayerPrefs.GetString("usermail", "");
            responseField.text = "";
            otherEmailField.gameObject.SetActive(!toPakaPaka);
        }
        public void Close()
        {
            panel.SetActive(false);
        }
        public void Send()
        {
            if (sending) return;
            Data data = new Data();
            data.username = nameField.text;
            data.emailTo = otherEmailField.text;

            if (nameField.text == "")
                nameField.GetComponent<Animation>().Play();
            else if (!toPakaPaka && (otherEmailField.text == "" || !IsValidEmailAddress(otherEmailField.text)))
                otherEmailField.GetComponent<Animation>().Play();
            else
            {
                PlayerPrefs.SetString("username", data.username);
                PlayerPrefs.SetString("usermail", data.emailTo);
                sending = true;
                responseField.text = "Enviando...";
                OnDone(data);
                Invoke("Close", 3);
            }
        }
        public bool IsValidEmailAddress(string s)
        {
            var regex = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
            return regex.IsMatch(s);
        }
    }

}