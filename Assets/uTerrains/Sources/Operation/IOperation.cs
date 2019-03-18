using UnityEngine;

namespace UltimateTerrains
{
    public interface IOperation
    {
        /// <summary>
        ///     Gets the min (down-back-left) vector of the effect area.
        /// </summary>
        /// <returns>The min (down-back-left) vector of the effect area.</returns>
        Vector3i GetAreaOfEffectMin();

        /// <summary>
        ///     Gets the max (up-front-right) vector of the effect area.
        /// </summary>
        /// <returns>The max (up-front-right) vector of the effect area.</returns>
        Vector3i GetAreaOfEffectMax();

        /// <summary>
        ///     Tells Ultimate Terrains wether or not the chunk defined by its min and max corners has a chance to be
        ///     affected by this operation. If this method returns false for a given chunk, Ultimate Terrains won't
        ///     recompute it which will save processing time. This is nothing more than an optimization.
        /// </summary>
        /// <param name="chunkDownBackLeft">Min corner of the chunk</param>
        /// <param name="chunkUpFrontRight">Max corner of the chunk</param>
        /// <returns>True if the chunkhas a chance to be affected. False otherwise.</returns>
        /// <remarks>If you are not sure how to implement this method, just make it return true.</remarks>
        bool WillAffectChunk(Vector3i chunkDownBackLeft, Vector3i chunkUpFrontRight);

        /// <summary>
        ///     Perform this operation on the given voxel.
        /// </summary>
        /// <param name="x">The x coordinate of the voxel world position.</param>
        /// <param name="y">The y coordinate of the voxel world position.</param>
        /// <param name="z">The z coordinate of the voxel world position.</param>
        /// <param name="voxel">Voxel to be affected by this operation.</param>
        /// <returns>True if the voxel type is overriden by this operation. False otherwise.</returns>
        bool Act(double x, double y, double z, ref Voxel voxel);

        /// <summary>
        ///     Tells Ultimate Terrains if this operation can be merged with the other one.
        ///     Ultimate Terrains will try to merge operations when one of them is completely included into
        ///     the other one (as per GetAreaOfEffectMin and GetAreaOfEffectMax).
        /// </summary>
        /// <param name="other">The operation to be merged with this one.</param>
        /// <returns>True if this operation can be merged. False otherwise.</returns>
        /// <remarks>If you are not sure how to implement this method, just make it return false.</remarks>
        bool CanBeMergedWith(IOperation other);

        /// <summary>
        ///     Merges this operation with the other one.
        /// </summary>
        /// <param name="other">The operation to be merged with this one.</param>
        /// <returns>The operation created/modified by merge.</returns>
        IOperation Merge(IOperation other);

        /// <summary>
        ///     Fills the buffer with colliders that will receive OnAffectedByTerrainOperation event.
        /// </summary>
        /// <returns>The amount of colliders stored in collidersBuffer.</returns>
        int FindAffectedColliders(Collider[] collidersBuffer);

        /// <summary>
        ///     Called once the operation is done.
        /// </summary>
        void OnOperationDone();

        /// <summary>
        ///     Called once the operation is undone.
        /// </summary>
        void OnOperationUndone();

        /// <summary>
        ///     Called right after this operation has been deserialized. Some initialization may be done here.
        /// </summary>
        /// <param name="uTerrain">The Ultimate terrain.</param>
        void AfterDeserialize(UltimateTerrain uTerrain);
    }
}