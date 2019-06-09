using System.Collections.Generic;

public static partial class Cache
{
	public class User : CacheBase
	{
		// フロントでのみ使用
		public BodyScaleData bodyScaleData;

		public UserData userData;

		public override void Unload()
		{
			this.bodyScaleData = null;
			this.userData = null;
		}
	}
}