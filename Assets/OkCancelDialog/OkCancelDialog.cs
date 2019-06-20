using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace OKCANCELDIALOG
{
	public class OkCancelDialog : UtilComponent
	{
        [SerializeField] Text txtExplain;
        [SerializeField] GameObject objNG;
        [SerializeField] HitDialog hitNG;
        [SerializeField] HitDialog hitOK;

        float ShowDuration = 0.3f;

        Action callbackNG;
        Action callbackOK;

        bool isWaitingAnswer = false;


        public void Init(Action callbackOK, Action callbackNG=null)
        {
            this.callbackNG = callbackNG;
            this.callbackOK = callbackOK;
            isWaitingAnswer = true;
            SetActive(this, true);
            hitNG.Init();
            hitOK.Init();
            SetActive(objNG, callbackNG != null);
        }

        public void ShowDialog(string text)
		{
			txtExplain.text = text;
			transform.DOScale(Vector3.one, ShowDuration);
		}


        public void OK()
        {
            if (!isWaitingAnswer) return;
            isWaitingAnswer = false;

            callbackOK();
            transform.DOScale(Vector3.zero, ShowDuration).OnComplete(() => SetActive(this, false));
        }


        public void NG()
        {
            if (!isWaitingAnswer) return;
            isWaitingAnswer = false;

            callbackNG();
            transform.DOScale(Vector3.zero, ShowDuration).OnComplete(()=> SetActive(this, false));


        }
    }
}
