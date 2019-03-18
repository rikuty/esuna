using System.Globalization;

namespace UltimateTerrains
{
    public struct Voxel // 23 bytes
    {
        private double voxelValue; // 8
        public Vector3s Position; // 2*3
        public ushort VoxelTypeIndex { get; private set; } // 2
        public ushort MaterialIndex { get; private set; } // 2
        public bool IsInside { get; private set; } // 1
        public int OverrideVoxelTypePriority; // 4

        public double Value {
            get { return voxelValue; }
            set {
                if (value <= -Param.MAX_LEVEL) {
                    voxelValue = -Param.MAX_LEVEL;
                    IsInside = true;
                } else if (value >= Param.MAX_LEVEL) {
                    voxelValue = Param.MAX_LEVEL;
                    IsInside = false;
                } else {
                    voxelValue = value;
                    IsInside = value < 0;
                }
            }
        }

        public VoxelType VoxelType {
            set {
                VoxelTypeIndex = value.Index;
                MaterialIndex = value.MaterialIndex;
            }
        }

        public double GetClampedValue(int chunkLevel)
        {
            if (voxelValue <= -chunkLevel) {
                return -chunkLevel;
            }

            if (voxelValue >= chunkLevel) {
                return chunkLevel;
            }

            return voxelValue;
        }

        public Vector3d RealPosition {
            get { return new Vector3d(Position.x * Param.SIZE_OFFSET_INVERSE, Position.y * Param.SIZE_OFFSET_INVERSE, Position.z * Param.SIZE_OFFSET_INVERSE); }
        }


        public bool IsOnSurface(double distToBeNear)
        {
            return voxelValue < distToBeNear && voxelValue > -distToBeNear;
        }

        public override string ToString()
        {
            return string.Format("[Voxel: Value={0}, Position={1}, VoxelTypeIndex={2}]", Value.ToString(CultureInfo.InvariantCulture), Position.ToShortString(), VoxelTypeIndex);
        }
    }
}