using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace OKCANCELDIALOG
{
	public class OkCancelDialog : MonoBehaviour
	{
		public Vector3 StartScale;
		public Vector3 ShowScale;
		public float ShowDuration = 1f;

		void Start()
		{
			transform.localScale = StartScale;
		}

		public void ShowDialog(string text)
		{
			transform.Find("Canvas/Panel/Text").GetComponent<Text>().text = text;
			transform.DOScale(ShowScale, ShowDuration);
		}
	}
}
