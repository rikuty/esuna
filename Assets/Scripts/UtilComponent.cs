//#define USE_NGUI
#define USE_SPLITE_ATLASE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UtilComponent : MonoBehaviour {

	#region Destroy
	//オブジェクト破棄用退避
	private static Transform _destroyTr;
	private static Transform destroyTr {
		get {
			if(_destroyTr != null) return _destroyTr;

			_destroyTr = new GameObject("_destroyTr").transform;
			_destroyTr.localScale = Vector3.zero;

			//メインカメラの下にオブジェクトがくるよう調整
			Transform parent = (Camera.main != null) ? Camera.main.transform : null;
			if (parent != null){
				_destroyTr.SetParent( parent );
			}
			//return null; 
			return _destroyTr; 
		}
	}
	
	/// <summary>子供のgameObjectを破棄</summary>
	public static void DestroyChildren( GameObject parentGameObject ) {
		if( parentGameObject == null ) return;
		DestroyChildren( parentGameObject.transform );
	}

	/// <summary>子供のgameObjectを破棄</summary>
	public static void DestroyChildren( Transform parentTransform ) {
		if( parentTransform == null ) return;
		List<GameObject> children = new List<GameObject>();
		foreach ( Transform child in parentTransform ) {
			//child.localScale = Vector3.zero;
			children.Add(child.gameObject);
		}
		foreach (GameObject child in children) {
			DestroyObj (child);
		}
	}

	/// <summary>gameObjectを破棄</summary>
	public static void DestroyObj ( MonoBehaviour target ) {
		if( target == null ) return;
		DestroyObj( target.gameObject );
	}

	/// <summary>gameObjectを破棄</summary>
	public static void DestroyObj ( GameObject targetGameObject ) {
		if( targetGameObject == null ) return;
		ForceActive( targetGameObject.transform );
		GameObject.Destroy( targetGameObject );
	}
	#endregion


	public static Transform MakeTransform( string name, Transform parent ){
		Transform tr = new GameObject(name).transform;
		tr.SetParent(parent);
		tr.localScale = Vector3.zero;
		return tr;
	}

	public static RectTransform MakeRectTransform( string name, Transform parent ){
		RectTransform rt = new GameObject(name).AddComponent<RectTransform>();
		rt.SetParent(parent);
		rt.localScale = Vector3.zero;
		return rt;
	}

	public static bool CheckDiffSize( Vector2 diff, Vector2 checkSize ) {
		if( diff.x < 0 ) diff.x = -diff.x;
		if( diff.y < 0 ) diff.y = -diff.y;
		return (diff.x > checkSize.x) || (diff.y > checkSize.y);
	}

	public static IEnumerator WaitForFPS(float frameRate = 0f, int maxCount = 5){
		if( frameRate.Equals(0f) ) {
			frameRate = ((float)Application.targetFrameRate)/2f;
		}
		float currentFps = 0f;
		int count = 0;
		do {
			yield return null;
			count++;
			currentFps = (1f/UnityEngine.Time.deltaTime);
			if( count >= maxCount ) break;
		}while( currentFps < frameRate );
//		Debug.LogError(count);
	}

	/// <summary>キャッシュにあるゲームオブジェクトを取得する</summary>
//	public static GameObject GetPreLoadPrefab(string prefabPath){
//		if(!Cache.ContainsUnityObject(prefabPath)){
//			Cache.AddUnityObject(prefabPath, Resources.Load(prefabPath) as GameObject);
//		}
//		return Cache.GetUnityObject<GameObject>(prefabPath);
//	}
	
	#region SetActive
	/// <summary>子供のgameObjecetを全てactiveにする (※ OnDestroyを呼ぶため)</summary>
	private static void ForceActive( Transform targetTransform ) {
		targetTransform.SetParent( destroyTr);
		//targetTransform.localScale = Vector3.zero;
		targetTransform.gameObject.SetActive(true);
		Transform[] children = targetTransform.GetComponentsInChildren<Transform>(true);
		SetActive( children, true );
	}

	public static void SetActive( GameObject target, bool isShow ) {
		if( target == null ) return;
		target.SetActive( isShow );
	}
	
	public static void SetActive( MonoBehaviour target, bool isShow ) {
		if( target == null ) return;
		SetActive( target.gameObject, isShow );
	}

	public static void SetActive( Transform target, bool isShow ) {
		if( target == null ) return;
		target.gameObject.SetActive( isShow );
	}

	public static void SetActive( GameObject[] targets, bool isShow ) {
		for(int i=0; i < targets.Length; i++) {
			SetActive( targets[i], isShow );
		}
	}

	public static void SetActive( MonoBehaviour[] targets, bool isShow ) {
		for(int i=0; i < targets.Length; i++) {
			SetActive( targets[i], isShow );
		}
	}

	public static void SetActive( Transform[] targets, bool isShow ) {
		for(int i=0; i < targets.Length; i++) {
			SetActive( targets[i], isShow );
		}
	}
	#endregion

#if USE_NGUI
	public static void SetLabel( UILabel lbl, string val ) {
		if( lbl == null ) return;
		lbl.text = val;
	}
	
	public static void SetLabel( UILabel lbl, int val, bool isSeparete = false) {
		if( lbl == null ) return;
		if( isSeparete ) {
			AppDebug.Log(" ", AppDebug.LogType.Todo);
//			lbl.text = CommonUtil.GetThansandSeparatedStr( val );
		}else{
			lbl.text = val.ToString();
		}
	}

	public static void SetInput( UIInput ipt, string val ) {
		if( ipt == null ) return;
		SetLabel (ipt.label, val);
	}

	public static void SetTexture( UITexture texture, Texture2D texture2D ){
		if( texture == null ) return;
		texture.mainTexture = texture2D;
	}
	
	public static void SetTexture( UITexture texture, Color color ){
		if( texture == null ) return;
		texture.color = color;
	}
	
	public static void SetSprite( UISprite sprite, Color color ) {
		if( sprite == null ) return;
		sprite.color = color;
	}
	
	public static void SetSprite( UISprite sprite, string spriteName ) {
		if( sprite == null ) return;
		sprite.spriteName = spriteName;
	}

	public static void SetFillAmount(UISprite sprite, float fillAmount){
		if (sprite == null) return;
		sprite.fillAmount = fillAmount;
	}
	
	public static void SetSlider( UISlider slider, float val ) {
		if( slider == null ) return;
		slider.sliderValue = val;
	}

	public static void SetWidgetColor(UIWidget widget, Color color){
		if (widget == null) return;
		widget.color = color;
	}

	public static void SetWidgetPixelPerfect(UIWidget widget){
		if (widget == null) return;
		widget.MakePixelPerfect ();
	}

#else
    public static void SetLabel(Text txt, string val)
    {
        if (txt == null) return;
        txt.text = val;
    }

    public static void SetLabel(Text txt, int val, bool isSeparete = false)
    {
        if (txt == null) return;
        if (isSeparete)
        {
            //          AppDebug.Log(" ", AppDebug.LogType.Todo);
            //          lbl.text = CommonUtil.GetThansandSeparatedStr( val );
        }
        else
        {
            txt.text = val.ToString();
        }
    }

	public static void SetLabel( Text[] txt, string val ) {
		for(int i=0; i<txt.Length; i++){
			SetLabel(txt[i], val);
		}
	}



	public static void SetLabelColor( Text txt, Color color){
		if(txt == null)	return;
		txt.color = color;
	}

	public static void SetInput( InputField ipt, string val ) {
		if( ipt == null ) return;
		ipt.text = val;
//		SetLabel (ipt.textComponent, val);
	}

	public static void SetImage( Image image, Texture2D texture2D ){
		SetImage(image, texture2D, 100.0F);
	}

	public static void SetImage( Image image, Texture2D texture2D, float pixelPerUnit ){
		if( image == null ) return;
		Rect rect = new Rect(0.0F, 0.0F, (float)texture2D.width, (float)texture2D.height);
		Sprite sprite = Sprite.Create(texture2D, rect, new Vector2(0.5F, 0.5F), pixelPerUnit);
		image.sprite = sprite;
	}

	public static void SetRawImage( RawImage rawImage, Texture2D texture2D, bool isSetNativeSize = true ){
		if (rawImage == null) return;
		rawImage.texture = texture2D;
		if (isSetNativeSize) {
			rawImage.SetNativeSize ();
		}
	}

	public static void SetImageColor( Image image, Color color ){
		if( image == null ) return;
		image.color = color;
	}

	public static void SetImageFillAmount(Image image, float fillAmount){
		if(image == null)	return;
		image.fillAmount = fillAmount;
	}

	public static void SetSlider( Slider slider, float val ) {
		if( slider == null ) return;
		slider.value = val;
	}

	public static void SetGraphicColor(Graphic[] graphics, Color color){
		if(graphics == null)
			return;
		for (int i = 0; i < graphics.Length; i++) {
			SetGraphicColor (graphics [i], color);
		}
	}

	/// <summary> color設定(アルファ値のみ現状保持)</summary>
	public static void SetGraphicColorKeepAlpha(Transform transform, Color color){
		SetGraphicColorKeepAlpha (transform.GetComponentsInChildren<Graphic> (true), color);
//		SetGraphicColorKeepAlpha (transform.GetComponents<Graphic>(), color);
	}

	/// <summary> color設定(アルファ値のみ現状保持)</summary>
	public static void SetGraphicColorKeepAlpha(Graphic[] graphics, Color color){
		for (int i = 0; i < graphics.Length; i++) {
			SetGraphicColorKeepAlpha (graphics [i], color);
		}
	}

	/// <summary> color設定(アルファ値のみ現状保持)</summary>
	public static void SetGraphicColorKeepAlpha(Graphic graphic, Color color){
		if (graphic == null) {
			return;
		}
		// アルファ値のみ保持
		color.a = graphic.color.a;
		graphic.color = color;
	}

	public static void SetGraphicColor(Graphic graphic, Color color){
		if(graphic == null)
			return;
		graphic.color = color;
	}

	/// <summary> color設定(アルファ値のみ)</summary>
	public static void SetGraphicAlpha(Transform transform, float alpha){
		SetGraphicAlpha (transform.GetComponentsInChildren<Graphic> (true), alpha);
//		SetGraphicAlpha (transform.GetComponents<Graphic>(), alpha);
	}

	/// <summary> color設定(アルファ値のみ)</summary>
	public static void SetGraphicAlpha(Graphic[] graphics, float alpha){
		for (int i = 0; i < graphics.Length; i++) {
			SetGraphicAlpha (graphics [i], alpha);
		}
	}
	public static void SetGraphicAlpha(Graphic graphic, float alpha){
		if(graphic == null)
			return;
		Color toColor = graphic.color;
		toColor.a = alpha;
		graphic.color = toColor;
	}

	public static void SetGraphicPixelPerfect(Graphic graphic){
		if(graphic == null)
			return;
		graphic.SetNativeSize();
	}


	public static void SetAnchor(RectTransform rtr, DEFINE_APP.UGUI.ANCHOR anchor, bool isSetAnchoredPosition = true){
		SetAnchor (rtr, DEFINE_APP.UGUI.ANCHOR_VECTOR2 [anchor], isSetAnchoredPosition);
	}

	public static void SetAnchor(RectTransform tr, Vector2 vec, bool isSetAnchoredPosition = true){
		if (tr == null) {
			return;
		}
		tr.anchorMax = vec;
		tr.anchorMin = vec;
		if (isSetAnchoredPosition) {
			tr.pivot = vec;
		}
	}
#endif

	#region renderer
	/// <summary> color設定(アルファ値のみ)</summary>
	public static void SetRendererAlpha(Transform transform, float alpha){
		SetRendererAlpha (transform.GetComponentsInChildren<Renderer> (true), alpha);
	}

	/// <summary> color設定(アルファ値のみ)</summary>
	public static void SetRendererAlpha(Renderer[] renderers, float alpha){
		for (int i = 0; i < renderers.Length; i++) {
			if (renderers[i] is SpriteRenderer) {
				SetSpriteRendererAlpha ((renderers[i] as SpriteRenderer), alpha);
			}
			else {
				SetGraphicAlpha (renderers[i], alpha);
			}
		}
	}
	public static void SetGraphicAlpha(Renderer renderer, float alpha){
		if(renderer == null || renderer.material == null)
			return;
		Color toColor = renderer.material.color;
		toColor.a = alpha;
		renderer.material.color = toColor;
	}
	public static void SetSpriteRendererAlpha(SpriteRenderer spriteRenderer, float alpha){
		if (spriteRenderer == null) {
			return;
		}
		Color toColor = spriteRenderer.color;
		toColor.a = alpha;
		spriteRenderer.color = toColor;
	}
	#endregion

	#if USE_SPLITE_ATLASE
//	public static void SetSpriteAtlasUVColor( SpriteAtlasUV spriteAtlasUV, Color color ) {
//		if( spriteAtlasUV == null ) return;
//		spriteAtlasUV.SetImageColor(color);
//	}
//
//	public static void SetSpriteAtlasUV( SpriteAtlasUV spriteAtlasUV, string spriteName ) {
//		if( spriteAtlasUV == null ) return;
//		spriteAtlasUV.spriteName = spriteName;
//	}
//
//	public static void SetSpriteAtlasUV( SpriteAtlasUV[] spriteAtlasUV, string spriteName ) {
//		if( spriteAtlasUV == null ) return;
//		for(int i=0; i<spriteAtlasUV.Length; i++){
//			spriteAtlasUV[i].spriteName = spriteName;
//		}
//	}
//
//	public static void SetSpriteAtlas( SpriteAtlasUV spriteAtlasUV, int index ) {
//		if( spriteAtlasUV == null ) return;
//		if(spriteAtlasUV.spriteAtlas.sprites.Length <= index) return;
//		spriteAtlasUV.SetSprite(spriteAtlasUV.spriteAtlas.sprites[index]);
//	}
//
//	public static void SetSpriteAtlasUVFillAmount(SpriteAtlasUV spriteAtlasUV, float fillAmount){
//		if (spriteAtlasUV == null) return;
//		spriteAtlasUV.SetImageFillAmount(fillAmount);
//	}
	#endif

	#region Layer
//	/// <summary>引数のGameObjectを含む全子オブジェクトのレイヤーを変更する。DEFINE_APP.LAYERSクラス参照</summary>
//	public static void SetLayer(GameObject obj, DEFINE_APP.LAYERS layer){
//		SetLayer (obj, (int)layer);
//	}
//
//	public static void SetLayer(GameObject obj, int layerId){
//		obj.layer = layerId;
//		for (int index = 0; index < obj.transform.childCount; index++) {
//			SetLayer (obj.transform.GetChild (index).gameObject, layerId);
//		}
//	}
	#endregion Layer

	#region Layer
//	public static void SetSortingLayer(Transform target, DEFINE_APP.SORTING_LAYERS sortingLayer)
//	{
//		Renderer renderer = target.GetComponent<Renderer>();
//		if (renderer != null) {
//			renderer.sortingLayerName = sortingLayer.ToString();
//		}
//		for (int i = 0; i < target.childCount; i++) {
//			SetSortingLayer(target.GetChild(i), sortingLayer);
//		}
//	}
	#endregion


	#region Transform
	/// <summary>
	/// ヒエラルキーでの順番通りのindexで子オブジェクトを取得
	/// </summary>
	public static Transform GetHierarchyChild(Transform parent, int index)
	{
		for (int i = 0; i < parent.childCount; i++) {
			Transform child = parent.GetChild(i);
			if (child.GetSiblingIndex() == index) {
				return child;
			}
		}
		return null;
	}
	#endregion
}