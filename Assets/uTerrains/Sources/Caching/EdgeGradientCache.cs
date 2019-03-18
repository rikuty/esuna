namespace UltimateTerrains
{
    internal sealed class EdgeGradientCache
    {
        private const int MIN_X = 0, MAX_X = Param.CACHE_SIZE_X - MIN_X;
        private const int MIN_Y = 0, MAX_Y = Param.CACHE_SIZE_Y - MIN_Y;
        private const int MIN_Z = 0, MAX_Z = Param.CACHE_SIZE_Z - MIN_Z;
        
        private struct CachedVector3d
        {
            public Vector3d Position;
            public Vector3d Gradient;
            public bool Cached;
        }

        private readonly CachedVector3d[] cache;

        public EdgeGradientCache()
        {
            cache = new CachedVector3d[Param.CACHE_3D_SIZE];
        }


        internal bool TryGet(Vector3s key, out Vector3d position, out Vector3d gradient)
        {
            if (key.x < MIN_X || key.x >= MAX_X || key.y < MIN_Y || key.y >= MAX_Y || key.z < MIN_Z || key.z >= MAX_Z) {
                gradient = Vector3d.zero;
                position= Vector3d.zero;
                return false;
            }

            var c = cache[key.x - MIN_X + Param.CACHE_SIZE_X * (key.y - MIN_Y) + Param.CACHE_SIZE_X * Param.CACHE_SIZE_Y * (key.z - MIN_Z)];
            position = c.Position;
            gradient = c.Gradient;
            return c.Cached;
        }

        internal void Cache(Vector3s key, Vector3d position, Vector3d gradient)
        {
            if (key.x >= MIN_X && key.x < MAX_X && key.y >= MIN_Y && key.y < MAX_Y && key.z >= MIN_Z && key.z < MAX_Z) {
                var c = new CachedVector3d
                {
                    Position = position,
                    Gradient = gradient,
                    Cached = true
                };
                cache[key.x - MIN_X + Param.CACHE_SIZE_X * (key.y - MIN_Y) + Param.CACHE_SIZE_X * Param.CACHE_SIZE_Y * (key.z - MIN_Z)] = c;
            } else {
                UDebug.LogWarning("Tried to cache an edge gradient out of cache zone [" + MIN_X + ";" + MAX_X + "] at (" + key.x + ", " + key.z + ")");
            }
        }
        
        internal void ResetCache()
        {
            for (var i = 0; i < Param.CACHE_3D_SIZE; ++i) {
                cache[i].Cached = false;
            }
        }
    }
}