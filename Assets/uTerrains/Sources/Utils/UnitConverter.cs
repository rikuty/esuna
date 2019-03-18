using System;
using UnityEngine;

namespace UltimateTerrains
{
    /// <summary>
    ///     This class offers useful methods to convert 3D positions between Unity's world unit, voxel unit and chunk unit.
    /// </summary>
    public sealed class UnitConverter
    {
        // Fast access to params
        private readonly int lod2Distance;
        private readonly int lod4Distance;
        private readonly int lod8Distance;
        private readonly int lod16Distance;
        private readonly int lod32Distance;
        private readonly int lod64Distance;
        private readonly int maxLevel;
        private readonly int maxLevelBits;
        private readonly double sizeXVoxel;
        private readonly double sizeYVoxel;
        private readonly double sizeZVoxel;
        private readonly double sizeXTotal;
        private readonly double sizeYTotal;
        private readonly double sizeZTotal;
        private readonly double minSizeVoxel;

        public UnitConverter(Param terrainParams)
        {
            lod2Distance = terrainParams.Lod2Distance;
            lod4Distance = terrainParams.Lod4Distance;
            lod8Distance = terrainParams.Lod8Distance;
            lod16Distance = terrainParams.Lod16Distance;
            lod32Distance = terrainParams.Lod32Distance;
            lod64Distance = terrainParams.Lod64Distance;
            maxLevel = terrainParams.MaxLevel;
            maxLevelBits = terrainParams.MaxLevelBits;
            sizeXVoxel = terrainParams.SizeXVoxel;
            sizeYVoxel = terrainParams.SizeYVoxel;
            sizeZVoxel = terrainParams.SizeZVoxel;
            sizeXTotal = terrainParams.SizeXTotal;
            sizeYTotal = terrainParams.SizeYTotal;
            sizeZTotal = terrainParams.SizeZTotal;
            minSizeVoxel = UMath.Min(sizeXVoxel, sizeYVoxel, sizeZVoxel);
        }


        /// <summary>
        ///     Returns a chunk-tree world position from a chunk world position.
        /// </summary>
        /// <param name="chunkWorldPosition">Position in 'chunk unit'.</param>
        /// <returns>The chunk-tree world position corresponding to the chunk world position.</returns>
        public Vector3i ChunkToChunkTreePosition(Vector3i chunkWorldPosition)
        {
            chunkWorldPosition.x = chunkWorldPosition.x >> maxLevelBits;
            chunkWorldPosition.y = chunkWorldPosition.y >> maxLevelBits;
            chunkWorldPosition.z = chunkWorldPosition.z >> maxLevelBits;
            return chunkWorldPosition;
        }

        /// <summary>
        ///     Returns a chunk world position from a chunk-tree world position.
        /// </summary>
        /// <param name="chunkTreePosition">Position of chunk tree in 'chunk unit'.</param>
        /// <returns>The chunk world position corresponding to the chunk-tree world position.</returns>
        public Vector3i ChunkTreeToChunkPosition(Vector3i chunkTreePosition)
        {
            chunkTreePosition.x = chunkTreePosition.x << maxLevelBits;
            chunkTreePosition.y = chunkTreePosition.y << maxLevelBits;
            chunkTreePosition.z = chunkTreePosition.z << maxLevelBits;
            return chunkTreePosition;
        }

        /// <summary>
        ///     Returns a leveled chunk world position from a chunk world position and a given LOD.
        /// </summary>
        /// <param name="chunkWorldPosition">Position in 'chunk unit'.</param>
        /// <param name="level">Desired LOD.</param>
        /// <returns>The leveled chunk world position corresponding to the chunk world position.</returns>
        public static Vector3i ChunkToLeveledChunkPosition(Vector3i chunkWorldPosition, int level)
        {
            var levelBits = LevelToLevelIndex(level);
            chunkWorldPosition.x = chunkWorldPosition.x >> levelBits;
            chunkWorldPosition.y = chunkWorldPosition.y >> levelBits;
            chunkWorldPosition.z = chunkWorldPosition.z >> levelBits;
            return chunkWorldPosition * level;
        }

        /// <summary>
        ///     Returns a chunk world position from a voxel world position.
        /// </summary>
        /// <param name="voxelWorldPosition">Position in 'voxel unit'.</param>
        /// <returns>The chunk world position corresponding to the voxel world position.</returns>
        public static Vector3i VoxelToChunkPosition(Vector3i voxelWorldPosition)
        {
            voxelWorldPosition.x = voxelWorldPosition.x >> Param.SIZE_X_BITS;
            voxelWorldPosition.y = voxelWorldPosition.y >> Param.SIZE_Y_BITS;
            voxelWorldPosition.z = voxelWorldPosition.z >> Param.SIZE_Z_BITS;
            return voxelWorldPosition;
        }
        
        /// <summary>
        ///     Returns a chunk world position from a voxel world position.
        /// </summary>
        /// <param name="voxelWorldPosition">Position in 'voxel unit'.</param>
        /// <returns>The chunk world position corresponding to the voxel world position.</returns>
        public static Vector3i VoxelToChunkPosition(Vector3d voxelWorldPosition)
        {
            Vector3i pos;
            pos.x = (int)voxelWorldPosition.x >> Param.SIZE_X_BITS;
            pos.y = (int)voxelWorldPosition.y >> Param.SIZE_Y_BITS;
            pos.z = (int)voxelWorldPosition.z >> Param.SIZE_Z_BITS;
            return pos;
        }

        /// <summary>
        ///     Returns a chunk world position from a voxel world position.
        /// </summary>
        /// <returns>The chunk world position corresponding to the voxel world position.</returns>
        public static Vector3i VoxelToChunkPosition(double x, double y, double z)
        {
            Vector3i pos;
            pos.x = (int)x >> Param.SIZE_X_BITS;
            pos.y = (int)y >> Param.SIZE_Y_BITS;
            pos.z = (int)z >> Param.SIZE_Z_BITS;
            return pos;
        }
        
        /// <summary>
        ///     Returns a voxel world position from a chunk world position.
        /// </summary>
        /// <param name="chunkWorldPosition">Position in 'chunk unit'.</param>
        /// <returns>The voxel world position corresponding to the chunk world position.</returns>
        public static Vector3i ChunkToVoxelPosition(Vector3i chunkWorldPosition)
        {
            chunkWorldPosition.x = chunkWorldPosition.x << Param.SIZE_X_BITS;
            chunkWorldPosition.y = chunkWorldPosition.y << Param.SIZE_Y_BITS;
            chunkWorldPosition.z = chunkWorldPosition.z << Param.SIZE_Z_BITS;
            return chunkWorldPosition;
        }

        /// <summary>
        ///     Returns a chunk world position from a unity world position.
        /// </summary>
        /// <param name="unityWorldPosition">Position.</param>
        /// <returns>The chunk world position corresponding to the world position.</returns>
        public Vector3i UnityToChunkPosition(Vector3 unityWorldPosition)
        {
            return VoxelToChunkPosition(UnityToVoxelPositionFloor(unityWorldPosition));
        }

        /// <summary>
        ///     Returns the voxel position relative to its chunk from its world position.
        /// </summary>
        /// <param name="voxelWorldPosition">Position in 'voxel unit'.</param>
        /// <returns>The local voxel position corresponding to the voxel world position.</returns>
        public static Vector3i ToLocalPosition(Vector3i voxelWorldPosition)
        {
            Vector3i voxelLocalPosition;
            voxelLocalPosition.x = voxelWorldPosition.x & (Param.SIZE_X - 1);
            voxelLocalPosition.y = voxelWorldPosition.y & (Param.SIZE_Y - 1);
            voxelLocalPosition.z = voxelWorldPosition.z & (Param.SIZE_Z - 1);
            return voxelLocalPosition;
        }

        /// <summary>
        ///     Returns the voxel position relative to its chunk from its world position.
        /// </summary>
        /// <param name="voxelWorldPosition">Position in 'voxel unit'.</param>
        /// <returns>The local voxel position corresponding to the voxel world position.</returns>
        internal static Vector3i ToLocalPositionWithOffset(Vector3i voxelWorldPosition)
        {
            Vector3i voxelLocalPosition;
            voxelLocalPosition.x = voxelWorldPosition.x & (Param.SIZE_TOTAL - 1);
            voxelLocalPosition.y = voxelWorldPosition.y & (Param.SIZE_TOTAL - 1);
            voxelLocalPosition.z = voxelWorldPosition.z & (Param.SIZE_TOTAL - 1);
            return voxelLocalPosition;
        }

        /// <summary>
        ///     Returns a unity world position from a chunk world position and a local (relative to the chunk) voxel position.
        /// </summary>
        /// <param name="chunkWorldPosition">Position in 'chunk unit'.</param>
        /// <param name="voxelWorldPosition">Local (relative to the chunk) position in 'voxel unit'.</param>
        /// <returns>The world position corresponding to the chunk world position + local voxel position.</returns>
        public Vector3d ChunkVoxelToUnityPosition(Vector3i chunkWorldPosition, Vector3i voxelLocalPosition)
        {
            Vector3d p;
            p.x = chunkWorldPosition.x * sizeXTotal + voxelLocalPosition.x * sizeXVoxel;
            p.y = chunkWorldPosition.y * sizeYTotal + voxelLocalPosition.y * sizeYVoxel;
            p.z = chunkWorldPosition.z * sizeZTotal + voxelLocalPosition.z * sizeZVoxel;
            return p;
        }

        /// <summary>
        ///     Returns a unity world position from a chunk world position and a local (relative to the chunk) unity position.
        /// </summary>
        /// <param name="chunkWorldPosition">Position in 'chunk unit'.</param>
        /// <param name="unityLocalPosition">Local (relative to the chunk) position.</param>
        /// <returns>The world position corresponding to the chunk world position + local position.</returns>
        public Vector3d ChunkToUnityPosition(Vector3i chunkWorldPosition, Vector3 unityLocalPosition)
        {
            Vector3d p;
            p.x = chunkWorldPosition.x * sizeXTotal + unityLocalPosition.x;
            p.y = chunkWorldPosition.y * sizeYTotal + unityLocalPosition.y;
            p.z = chunkWorldPosition.z * sizeZTotal + unityLocalPosition.z;
            return p;
        }
        
        /// <summary>
        ///     Returns a unity vector from a voxel vector.
        /// </summary>
        /// <param name="voxelVector">Vector in 'voxel unit'.</param>
        /// <returns>The Unity vector corresponding to the voxel vector.</returns>
        public Vector3 VoxelToUnity(Vector3d voxelVector)
        {
            Vector3 p;
            p.x = (float)(voxelVector.x * sizeXVoxel);
            p.y = (float)(voxelVector.y * sizeYVoxel);
            p.z = (float)(voxelVector.z * sizeZVoxel);
            return p;
        }

        /// <summary>
        ///     Returns a unity world position from a voxel world position.
        /// </summary>
        /// <param name="voxelWorldPosition">Position in 'voxel unit'.</param>
        /// <returns>The world position corresponding to the voxel world position.</returns>
        public Vector3d VoxelToUnityPosition(Vector3i voxelWorldPosition)
        {
            Vector3d p;
            p.x = voxelWorldPosition.x * sizeXVoxel;
            p.y = voxelWorldPosition.y * sizeYVoxel;
            p.z = voxelWorldPosition.z * sizeZVoxel;
            return p;
        }

        /// <summary>
        ///     Returns a unity world position from a voxel world position.
        /// </summary>
        /// <param name="voxelWorldPosition">Position in 'voxel unit'.</param>
        /// <returns>The world position corresponding to the voxel world position.</returns>
        public Vector3 VoxelToUnityPosition(Vector3d voxelWorldPosition)
        {
            Vector3 p;
            p.x = (float)(voxelWorldPosition.x * sizeXVoxel);
            p.y = (float)(voxelWorldPosition.y * sizeYVoxel);
            p.z = (float)(voxelWorldPosition.z * sizeZVoxel);
            return p;
        }
        
        /// <summary>
        ///     Returns a unity world position from a voxel world position.
        /// </summary>
        /// <param name="voxelWorldPosition">Position in 'voxel unit'.</param>
        /// <returns>The world position corresponding to the voxel world position.</returns>
        public Vector3 VoxelToUnityPosition(Vector3 voxelWorldPosition)
        {
            Vector3 p;
            p.x = (float)(voxelWorldPosition.x * sizeXVoxel);
            p.y = (float)(voxelWorldPosition.y * sizeYVoxel);
            p.z = (float)(voxelWorldPosition.z * sizeZVoxel);
            return p;
        }

        /// <summary>
        ///     Returns a unity world position from a voxel world position.
        /// </summary>
        /// <param name="x">X coordinate in 'voxel unit'.</param>
        /// <param name="y">Y coordinate in 'voxel unit'.</param>
        /// <param name="z">Z coordinate in 'voxel unit'.</param>
        /// <returns>The world position corresponding to the voxel world position.</returns>
        public Vector3d VoxelToUnityPosition(int x, int y, int z)
        {
            Vector3d p;
            p.x = x * sizeXVoxel;
            p.y = y * sizeYVoxel;
            p.z = z * sizeZVoxel;
            return p;
        }

        /// <summary>
        ///     Returns a unity world position from a voxel world position.
        /// </summary>
        /// <param name="x">X coordinate in 'voxel unit'.</param>
        /// <param name="y">Y coordinate in 'voxel unit'.</param>
        /// <param name="z">Z coordinate in 'voxel unit'.</param>
        /// <returns>The world position corresponding to the voxel world position.</returns>
        public Vector3d VoxelToUnityPosition(double x, double y, double z)
        {
            Vector3d p;
            p.x = x * sizeXVoxel;
            p.y = y * sizeYVoxel;
            p.z = z * sizeZVoxel;
            return p;
        }

        /// <summary>
        ///     Returns a unity world position X coordinate from a voxel world position X coordinate.
        /// </summary>
        /// <param name="x">X coordinate in 'voxel unit'.</param>
        /// <returns>The world position corresponding to the voxel world position.</returns>
        public double VoxelToUnityPositionX(double x)
        {
            return x * sizeXVoxel;
        }

        /// <summary>
        ///     Returns a unity world position Y coordinate from a voxel world position Y coordinate.
        /// </summary>
        /// <param name="y">Y coordinate in 'voxel unit'.</param>
        /// <returns>The world position corresponding to the voxel world position.</returns>
        public double VoxelToUnityPositionY(double y)
        {
            return y * sizeYVoxel;
        }

        /// <summary>
        ///     Returns a unity world position Z coordinate from a voxel world position Z coordinate.
        /// </summary>
        /// <param name="z">Z coordinate in 'voxel unit'.</param>
        /// <returns>The world position corresponding to the voxel world position.</returns>
        public double VoxelToUnityPositionZ(double z)
        {
            return z * sizeZVoxel;
        }
        
        /// <summary>
        ///     Returns a voxel vector from a unity vector.
        /// </summary>
        /// <param name="unityVector">The vector</param>
        /// <returns>The voxel vector corresponding to the Unity vector.</returns>
        public Vector3d UnityToVoxel(Vector3 unityVector)
        {
            Vector3d p;
            p.x = unityVector.x / sizeXVoxel;
            p.y = unityVector.y / sizeYVoxel;
            p.z = unityVector.z / sizeZVoxel;
            return p;
        }
        
        /// <summary>
        ///     Returns a voxel world position from a unity world position.
        /// </summary>
        /// <param name="unityWorldPosition">Position.</param>
        /// <returns>The voxel world position corresponding to the world position.</returns>
        public Vector3d UnityToVoxelPosition(Vector3 unityWorldPosition)
        {
            Vector3d p;
            p.x = unityWorldPosition.x / sizeXVoxel;
            p.y = unityWorldPosition.y / sizeYVoxel;
            p.z = unityWorldPosition.z / sizeZVoxel;
            return p;
        }

        /// <summary>
        ///     Returns a voxel world position from a unity world position by rounding its value.
        /// </summary>
        /// <param name="unityWorldPosition">Position.</param>
        /// <returns>The voxel world position corresponding to the world position.</returns>
        public Vector3i UnityToVoxelPositionRound(Vector3 unityWorldPosition)
        {
            Vector3i p;
            p.x = Convert.ToInt32(unityWorldPosition.x / sizeXVoxel);
            p.y = Convert.ToInt32(unityWorldPosition.y / sizeYVoxel);
            p.z = Convert.ToInt32(unityWorldPosition.z / sizeZVoxel);
            return p;
        }

        /// <summary>
        ///     Returns a voxel world position from a unity world position by rounding its value.
        /// </summary>
        /// <param name="unityWorldPosition">Position.</param>
        /// <returns>The voxel world position corresponding to the world position.</returns>
        public Vector3i UnityToVoxelPositionRound(Vector3d unityWorldPosition)
        {
            Vector3i p;
            p.x = Convert.ToInt32(unityWorldPosition.x / sizeXVoxel);
            p.y = Convert.ToInt32(unityWorldPosition.y / sizeYVoxel);
            p.z = Convert.ToInt32(unityWorldPosition.z / sizeZVoxel);
            return p;
        }

        /// <summary>
        ///     Returns a voxel world position from a unity world position by truncating its value.
        /// </summary>
        /// <param name="unityWorldPosition">Position.</param>
        /// <returns>The voxel world position corresponding to the world position.</returns>
        public Vector3i UnityToVoxelPositionFloor(Vector3 unityWorldPosition)
        {
            Vector3i p;
            p.x = (int) Math.Floor(unityWorldPosition.x / sizeXVoxel);
            p.y = (int) Math.Floor(unityWorldPosition.y / sizeYVoxel);
            p.z = (int) Math.Floor(unityWorldPosition.z / sizeZVoxel);
            return p;
        }

        /// <summary>
        ///     Returns a voxel world position from a unity world position by truncating its value.
        /// </summary>
        /// <param name="unityWorldPosition">Position.</param>
        /// <returns>The voxel world position corresponding to the world position.</returns>
        public Vector3i UnityToVoxelPositionFloor(Vector3d unityWorldPosition)
        {
            Vector3i p;
            p.x = (int) Math.Floor(unityWorldPosition.x / sizeXVoxel);
            p.y = (int) Math.Floor(unityWorldPosition.y / sizeYVoxel);
            p.z = (int) Math.Floor(unityWorldPosition.z / sizeZVoxel);
            return p;
        }
        
        /// <summary>
        ///     Returns a voxel distance from a unity distance.
        /// </summary>
        /// <param name="distance">Distance.</param>
        /// <returns>The voxel distance corresponding to the unity distance.</returns>
        public double UnityToVoxelDisance(double distance)
        {
            return distance / minSizeVoxel;
        }

        
        /// <summary>
        ///     Returns a voxel distance from a unity distance by rounding its value.
        /// </summary>
        /// <param name="distance">Distance.</param>
        /// <returns>The voxel distance corresponding to the unity distance.</returns>
        public int UnityToVoxelDisanceRound(double distance)
        {
            var vd = distance / minSizeVoxel;
            var ivd = (int) vd;
            return vd - ivd > Param.PRECISION ? ivd + 1 : ivd;
        }
        
        /// <summary>
        ///     Returns a unity distance from a voxel distance.
        /// </summary>
        /// <param name="distance">Distance.</param>
        /// <returns>The unity distance corresponding to the voxel distance.</returns>
        public double VoxelToUnityDisance(int distance)
        {
            return distance * minSizeVoxel;
        }
        
        /// <summary>
        ///     Returns a unity distance from a voxel distance.
        /// </summary>
        /// <param name="distance">Distance.</param>
        /// <returns>The unity distance corresponding to the voxel distance.</returns>
        public double VoxelToUnityDisance(double distance)
        {
            return distance * minSizeVoxel;
        }

        /// <summary>
        ///     Returns the level in power of 2 corresponding to a level-index.
        /// </summary>
        public static int LevelIndexToLevel(int levelIndex)
        {
            return 1 << levelIndex;
        }

        /// <summary>
        ///     Returns level-index corresponding to the level.
        /// </summary>
        public static int LevelToLevelIndex(int level)
        {
            switch (level) {
                case 1:
                    return 0;
                case 2:
                    return 1;
                case 4:
                    return 2;
                case 8:
                    return 3;
                case 16:
                    return 4;
                case 32:
                    return 5;
                case 64:
                    return 6;
                case 128:
                    return 7;
                case 256:
                    return 8;
                case 512:
                    return 9;
                case 1024:
                    return 10;
            }

            // ReSharper disable once HeapView.ObjectAllocation.Evident
            throw new ArgumentOutOfRangeException(string.Format("There is no value for level {0}.", level));
        }

        public int LevelToLodDistance(int level)
        {
            switch (level) {
                case 1:
                    return 0;
                case 2:
                    return lod2Distance;
                case 4:
                    return lod4Distance;
                case 8:
                    return lod8Distance;
                case 16:
                    return lod16Distance;
                case 32:
                    return lod32Distance;
                default:
                    return lod64Distance;
            }
        }

        public int LevelToLowerLodDistance(int level)
        {
            switch (level) {
                case 1:
                    return lod2Distance;
                case 2:
                    return lod4Distance;
                case 4:
                    return lod8Distance;
                case 8:
                    return lod16Distance;
                case 16:
                    return lod32Distance;
                case 32:
                    return lod64Distance;
                case 64:
                    return lod64Distance;
                case 128:
                    return lod64Distance;
                case 256:
                    return lod64Distance;
                case 512:
                    return lod64Distance;
                case 1024:
                    return 1024; // int.MaxValue
            }

            // ReSharper disable once HeapView.ObjectAllocation.Evident
            throw new ArgumentOutOfRangeException(string.Format("There is no value for level {0}.", level));
        }

        /// <summary>
        ///     Returns inverse of level.
        /// </summary>
        public static double LevelInverse(int level)
        {
            switch (level) {
                case 1:
                    return 1.0;
                case 2:
                    return 1.0 / 2.0;
                case 4:
                    return 1.0 / 4.0;
                case 8:
                    return 1.0 / 8.0;
                case 16:
                    return 1.0 / 16.0;
                case 32:
                    return 1.0 / 32.0;
                case 64:
                    return 1.0 / 64.0;
                case 128:
                    return 1.0 / 128.0;
                case 256:
                    return 1.0 / 256.0;
                case 512:
                    return 1.0 / 512.0;
                case 1024:
                    return 1.0 / 1024.0;
            }

            // ReSharper disable once HeapView.ObjectAllocation.Evident
            throw new ArgumentOutOfRangeException(string.Format("There is no value for level {0}.", level));
        }

        public int LevelAtPosition(Vector3i origin, Vector3i position)
        {
            for (var level = 1; level <= maxLevel; level *= 2) {
                if (position.IsInCubeArea(origin, LevelToLodDistance(level * 2) * level * 2)) {
                    return level;
                }
            }

            return maxLevel;
        }

        public Vector3d ToLeveledPosition(Vector3d pos, int level)
        {
            pos.x = pos.x * sizeXVoxel * level;
            pos.y = pos.y * sizeYVoxel * level;
            pos.z = pos.z * sizeZVoxel * level;
            return pos;
        }
    }
}