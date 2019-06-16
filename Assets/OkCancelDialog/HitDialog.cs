using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace OKCANCELDIALOG
{
	public class HitDialog : MonoBehaviour
	{
		public Color ClickedColor;
		public Vector3 ClickedScale;

		public float HitEventWait = 1.0f;
		public UnityEngine.Events.UnityEvent ClickedEvents = new UnityEngine.Events.UnityEvent();

        public void Init()
        {
            GetComponent<Image>().color = Color.white;
            transform.localScale = Vector3.one;
        }

        void OnTriggerEnter(Collider collision)
		{
			float t = 0.1f;

			DOTween.Sequence()
				.Append(transform.DOScale(ClickedScale, t))
				.Join(GetComponent<Image>().DOColor(ClickedColor, t))
				//.Join(transform.Find("Button_arrow").GetComponent<Image>().DOFade(0, t))
				//.Join(transform.Find("Checkmark").GetComponent<Image>().DOFade(1, t))
				.OnComplete(() =>
				{
					DOVirtual.DelayedCall(HitEventWait, () => ClickedEvents.Invoke());
				});
		}
	}
}