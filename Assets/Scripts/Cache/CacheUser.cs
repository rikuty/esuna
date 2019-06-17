using System.Collections.Generic;

public static partial class Cache
{
	public class User : CacheBase
	{
		// フロントでのみ使用
		private BodyScaleData bodyScaleData = null;
		public BodyScaleData BodyScaleData {
			get {
				if (this.bodyScaleData == null) {
					this.bodyScaleData = new BodyScaleData();
				}
				return this.bodyScaleData;
			}
			set {
				this.bodyScaleData = value;
			}
		}


		private UserData userData = null;
		public UserData UserData {
			get {
				if (this.userData == null) {
					this.userData = new UserData();
				}
				return this.userData;
			}
			set {
				this.userData = value;
			}
		}

		private MeasureData measureData = null;
		public MeasureData MeasureData {
			get {
				if (this.measureData == null) {
					this.measureData = new MeasureData();
				}
				return this.measureData;
			}
			set {
				this.measureData = value;
			}
		}


		public override void Unload()
		{
			this.bodyScaleData = null;
			this.userData = null;
			this.measureData = null;
		}
	}
}