using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ImageLoader : MonoBehaviour {

	[SerializeField] private RectTransform rectParent;
	[SerializeField] private Image targetImage;

	private enum FitPaturn {
		NONE,
		WIDTH,
		HEIGHT,
	}
	[SerializeField] private FitPaturn fitPaturn;

	private Texture2D orgTexture = null;

	IEnumerator Start(){

		// wwwクラスのコンストラクタに画像URLを指定
		string url = "http://****************.png";
		WWW www = new WWW(url);

		// 画像ダウンロード完了を待機
		yield return www;

		// webサーバから取得した画像をRaw Imagで表示する
		this.targetImage.sprite = this.SpriteFromTexture2D(www.textureNonReadable);

		switch(this.fitPaturn){
		case FitPaturn.WIDTH: this.FitWidth (); break;
		case FitPaturn.HEIGHT: this.FitHeight (); break;
		}
	}

	private Sprite SpriteFromTexture2D(Texture2D texture)
	{
		this.orgTexture = texture;

		Sprite sprite = null;
		if (texture)
		{
			//Texture2DからSprite作成
			sprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), Vector2.zero);
		}
		return sprite;
	}

	private void FitWidth(){
		float changeRate = this.rectParent.rect.width / this.orgTexture.width;
		float changeHeightSize = this.orgTexture.height * changeRate;

		this.targetImage.rectTransform.sizeDelta = new Vector2 (this.rectParent.rect.width, changeHeightSize);
	}

	private void FitHeight(){
		float changeRate = this.rectParent.rect.height / this.orgTexture.height;
		float changeWidthSize = this.orgTexture.width * changeRate;

		this.targetImage.rectTransform.sizeDelta = new Vector2 (changeWidthSize, this.rectParent.rect.height);
	}
}