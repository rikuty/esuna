namespace UltimateTerrains
{
    internal sealed class CachedMultipleValues
    {
        private double[] values;
        private bool cached;

        public double[] Values {
            get { return values; }
        }

        public bool IsCached {
            get { return cached; }
        }

        public void Cache(double[] v)
        {
            values = v;
            cached = true;
        }

        public void Reset(UnsafeArrayPool<double> pool)
        {
            if (values != null)
                pool.Free(values);
            values = null;
            cached = false;
        }

        public void CopyTo(CachedMultipleValues other, UnsafeArrayPool<double> pool)
        {
            if (other.values != null) {
                UDebug.Fatal("Tried to copy CachedMultipleValues that was not reset");
            }

            if (values != null) {
                other.values = pool.Get();
                if (other.values.Length != values.Length) {
                    UDebug.Fatal("Tried to copy values in an array of different size");
                }

                for (var i = 0; i < values.Length; i++) {
                    other.values[i] = values[i];
                }

                other.cached = cached;
            }
        }
    }
}