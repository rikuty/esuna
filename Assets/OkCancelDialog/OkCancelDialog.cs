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


        public void Init(Action callbackOK, Action callbackNG=null)
        {
            this.callbackNG = callbackNG;
            this.callbackOK = callbackOK;
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
            callbackOK();
            transform.DOScale(Vector3.zero, ShowDuration);
            SetActive(this, false);
        }


        public void NG()
        {
            callbackNG();
            transform.DOScale(Vector3.zero, ShowDuration);
            SetActive(this, false);


        }
    }
}
