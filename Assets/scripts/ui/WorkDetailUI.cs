using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WorkDetailUI : MonoBehaviour
    {
        public GameObject savedSignal;
        public GameObject WorkDetail;
        public Image workImage;
        public List<Image> borders;
        public GameObject confirmation;
        public GameObject sendedSign;
        public GameObject sendingSign;
       // public SharePanel sharePanel;
        public Animation pkpkAnim;
        string id;

        public void ShowWorkDetail(string _id, Sprite sprite, bool isNew)
        {
            AudioManager.Instance.musicManager.Play("work");
            id = _id;
            workImage.sprite = sprite;
            confirmation.SetActive(false);
            WorkDetail.SetActive(true);
            savedSignal.SetActive(isNew);
         //   SetSendedSign(id, pkpkShared);
            //sendedSign.SetActive(pkpkShared);
           // sendingSign.SetActive(Data.Instance.shareManager.sendingMail);
            Events.OnLoading(false);
        }

        public void Back()
        {
            //UIManager.Instance.albumUI.ShowAlbum(true);
            WorkDetail.SetActive(false);
        }

        public void EnviarPkPk()
        {

        }

        void SharePkpkConfirmed(bool isOk)
        {
        }

        //void OnDataDone(SharePanel.Data data)
        //{
        //    pkpkAnim.Stop();
        //   // Data.Instance.shareManager.SharePkpk(id, data);
        //}
        public void Share()
        {

        }

        void ShareConfirmed(bool isOk)
        {
            //if (isOk)
             //   Data.Instance.shareManager.Share(id);
        }

        //public void OnDataDoneSimpleShare(SharePanel.Data data)
        //{
        //  //  Data.Instance.shareManager.Share(id, data);
        //}

        public void Edit()
        {
            Debug.Log("Open Work Nr:" + id);
            UIManager.Instance.boardUI.LoadWork(id);
            WorkDetail.SetActive(false);
        }

        public void Delete()
        {
            confirmation.SetActive(true);
        }

        public void DeleteConfirmation(bool enable)
        {
            if (enable)
            {
                Data.Instance.albumData.DeleteCharacter(id);
                Back();
                confirmation.SetActive(false);
            }
            else
            {
                confirmation.SetActive(false);
            }
        }

        public void SetSendedSign(string _id, bool enable)
        {
            if (id == _id)
            {
                sendedSign.SetActive(enable);
                if (enable)
                    pkpkAnim.Stop();
                else
                    pkpkAnim.Play("enviar_btn");
            }
        }

        public void SetSendingSign(bool enable)
        {
            sendingSign.SetActive(enable);
        }

    }
}