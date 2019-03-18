namespace UltimateTerrains
{
    internal sealed class ChunkState
    {
        public long buildId;
        public int level;
        private Vector3i position;
        
        public Vector3i WorldPosition { get; private set; }

        public Vector3i Position {
            get { return position; }
            set {
                position = value;
                WorldPosition = new Vector3i(value.x * Param.SIZE_X, value.y * Param.SIZE_Y, value.z * Param.SIZE_Z);
            }
        }

        public ChunkLODBorderState lodBorderState;
        public bool IsRoot;

        public bool ShouldKeepCache(Param param)
        {
            return level <= param.MaxLevelWithCache;
        }

        internal void Reset()
        {
            buildId = -1;
            level = -1;
            IsRoot = false;
        }

        public int GetLevelIndex()
        {
            return UnitConverter.LevelToLevelIndex(level);
        }
    }
}