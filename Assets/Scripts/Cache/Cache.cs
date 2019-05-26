using System.Collections.Generic;

public static partial class Cache
{
	public enum CacheGroup : int {
		User = 0
	}

	private static Dictionary<CacheGroup, CacheBase> cacheHash;

    private static bool isInitialized = false;

    public static User user;

	public static void Initialize()
	{
        if (isInitialized) {
            return;
        }
        isInitialized = true;

		cacheHash = new Dictionary<CacheGroup, CacheBase> {
			{ CacheGroup.User, user = new User() }
		};
	}

	public static void Clear(CacheGroup cacheGroup)
	{
		switch (cacheGroup) {
		case CacheGroup.User:
			cacheHash[cacheGroup] = user = new User();
			break;
		}
	}

	public static void Unload()
	{
		foreach (CacheBase target in cacheHash.Values) {
			target.Unload();
		}
	}
}