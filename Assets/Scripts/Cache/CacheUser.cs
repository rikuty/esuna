using System.Collections.Generic;

public static partial class Cache
{
	public class User : CacheBase
	{
		public CalibrationData calibrationData;
		public UserData userData;

		public Dictionary<int, Dictionary<string, float>> goalCurrentRotDic;


		public override void Unload()
		{
			this.calibrationData = null;
			this.userData = null;
			this.goalCurrentRotDic = null;
		}
	}
}