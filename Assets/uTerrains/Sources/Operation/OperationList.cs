using System.Collections.Generic;

namespace UltimateTerrains
{
    public class OperationList
    {
        private readonly List<OperationHandler> operations = new List<OperationHandler>();
        private volatile int count;

        public int Count {
            get { return count; }
        }

        internal void Add(OperationHandler operation)
        {
            lock (operations) {
                if (count < operations.Count) {
                    operations[count] = operation;
                } else {
                    operations.Add(operation);
                }

                count++;
            }
        }

        internal void RemoveLastWhenEqual(OperationHandler operation)
        {
            lock (operations) {
                if (count > 0 && operations[count - 1].Operation == operation.Operation) {
                    count--;
                }
            }
        }

        internal void AddOrMerge(int currentMergeId, OperationHandler newOperation)
        {
            lock (operations) {
                var head = count - 1;
                if (head >= 0 && operations[head].IsAlreadyMergedWith(newOperation)) {
                    for (var i = head - 1; i >= 0; --i) {
                        if (!operations[i].IsAlreadyMergedWith(newOperation) && !operations[i].Merge(currentMergeId, newOperation))
                            return;
                        count--;
                    }

                    return;
                }

                if (head < 0 || !operations[head].Merge(currentMergeId, newOperation)) {
                    Add(newOperation);
                    return;
                }

                for (var i = head - 1; i >= 0; --i) {
                    if (!operations[i].IsAlreadyMergedWith(newOperation) && !operations[i].Merge(currentMergeId, newOperation))
                        return;
                    count--;
                }
            }
        }

        internal void ActAll(int maxCount, double x, double y, double z, ref Voxel voxel)
        {
            voxel.OverrideVoxelTypePriority = 0;
            for (var i = 0; i < maxCount; ++i) {
                operations[i].Act(x, y, z, ref voxel, i + 1);
            }
        }

        public OperationHandler this[int i] {
            get { return operations[i]; }
        }
    }
}