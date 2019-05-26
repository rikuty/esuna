using System.Collections;
using System.Collections.Generic;

public static partial class Cache
{
	public class User : CacheBase
	{
		public UserData userData;

		public override void Unload()
		{
			this.userData = null;
		}
	}
}