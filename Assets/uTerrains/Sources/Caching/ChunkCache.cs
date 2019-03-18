//#define CHECK_CACHE_BOUNDS

namespace UltimateTerrains
{
    internal sealed class ChunkCache
    {
        private const int MinX = 0, MaxX = Param.CACHE_SIZE_X - MinX;
        private const int MinY = 0, MaxY = Param.CACHE_SIZE_Y - MinY;
        private const int MinZ = 0, MaxZ = Param.CACHE_SIZE_Z - MinZ;

        private readonly CachedMultipleValues[] values2DCache;
        private readonly CachedMultipleValues[] biomeSelectorValues2DCache;

        public ChunkCache(UltimateTerrain terrain)
        {
            var biomes = terrain.GeneratorModules.Biomes;
            values2DCache = new CachedMultipleValues[Param.CACHE_2D_SIZE * biomes.Count];

            for (var i = 0; i < values2DCache.Length; ++i) {
                values2DCache[i] = new CachedMultipleValues();
            }

            biomeSelectorValues2DCache = new CachedMultipleValues[Param.CACHE_2D_SIZE];

            for (var i = 0; i < biomeSelectorValues2DCache.Length; ++i) {
                biomeSelectorValues2DCache[i] = new CachedMultipleValues();
            }
        }

        internal bool TryGetValue2D(int x, int z, int biomeId, out double[] values)
        {
#if CHECK_CACHE_BOUNDS
            if (x < MinX || x >= MaxX || z < MinZ || z >= MaxZ) {
                UDebug.LogError("Tried to cache a voxel out of cache zone");
                values = null;
                return false;
            }
#endif
            var v = values2DCache[x - MinX + Param.CACHE_SIZE_X * (z - MinZ) + Param.CACHE_SIZE_X * Param.CACHE_SIZE_Z * biomeId];
            values = v.Values;
            return v.IsCached;
        }

        internal void CacheValue2D(int x, int z, int biomeId, double[] values)
        {
#if CHECK_CACHE_BOUNDS
            if (x < MinX || x >= MaxX || z < MinZ || z >= MaxZ) {
                UDebug.LogError("Tried to cache a set of 2D values out of cache zone [" + MinX + ";" + MaxX + "] at (" + x + ", " + z + ")");
            }
#endif
            values2DCache[x - MinX + Param.CACHE_SIZE_X * (z - MinZ) + Param.CACHE_SIZE_X * Param.CACHE_SIZE_Z * biomeId].Cache(values);
        }

        internal bool TryGetBiomeSelectorValue2D(int x, int z, out double[] values)
        {
#if CHECK_CACHE_BOUNDS
            if (x < MinX || x >= MaxX || z < MinZ || z >= MaxZ) {
                UDebug.LogError("Tried to cache a voxel out of cache zone");
                values = null;
                return false;
            }
#endif
            var v = biomeSelectorValues2DCache[x - MinX + Param.CACHE_SIZE_X * (z - MinZ)];
            values = v.Values;
            return v.IsCached;
        }

        internal void CacheBiomeSelectorValue2D(int x, int z, double[] values)
        {
#if CHECK_CACHE_BOUNDS
            if (x < MinX || x >= MaxX || z < MinZ || z >= MaxZ) {
                UDebug.LogError("Tried to cache a set of 2D values out of cache zone [" + MinX + ";" + MaxX + "] at (" + x + ", " + z + ")");
            }
#endif
            biomeSelectorValues2DCache[x - MinX + Param.CACHE_SIZE_X * (z - MinZ)].Cache(values);
        }

        internal void ResetCache(UnsafeArrayPool<double> float2DPool)
        {
            for (var i = 0; i < values2DCache.Length; ++i) {
                values2DCache[i].Reset(float2DPool);
            }

            for (var i = 0; i < biomeSelectorValues2DCache.Length; ++i) {
                biomeSelectorValues2DCache[i].Reset(float2DPool);
            }
        }

        internal void CopyTo(ChunkCache other, UnsafeArrayPool<double> float2DPool)
        {
            for (var i = 0; i < values2DCache.Length; ++i) {
                values2DCache[i].CopyTo(other.values2DCache[i], float2DPool);
            }

            for (var i = 0; i < biomeSelectorValues2DCache.Length; ++i) {
                biomeSelectorValues2DCache[i].CopyTo(other.biomeSelectorValues2DCache[i], float2DPool);
            }
        }
    }
}