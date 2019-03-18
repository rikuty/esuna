namespace UltimateTerrains
{
    internal static class OctreePopulator
    {
        internal static void Populate(OctreeNode node, VoxelGenerator generator, int index, OctreeNode parent,
                                      OctreeNode n0, OctreeNode n1, OctreeNode n2, OctreeNode n3, OctreeNode n4, OctreeNode n5, OctreeNode n6)
        {
            //UnityEngine.Profiler.BeginSample ("Populate");
            generator.PrepareNode(node.Level >= Param.SIZE_OFFSET);

            // Generate center
            if (parent == null) {
                if (node.Level > Param.SIZE_OFFSET) {
                    // Generate bottom voxels
                    node.CenterBackBottomVoxel = generator.GenerateNoGradient(node.CenterBackBottom);
                    node.CenterLeftBottomVoxel = generator.GenerateNoGradient(node.CenterLeftBottom);
                    node.CenterBottomVoxel = generator.GenerateNoGradient(node.CenterBottom);
                    node.CenterRightBottomVoxel = generator.GenerateNoGradient(node.CenterRightBottom);
                    node.CenterFrontBottomVoxel = generator.GenerateNoGradient(node.CenterFrontBottom);

                    // Generate side voxels
                    node.CenterBackLeftVoxel = generator.GenerateNoGradient(node.CenterBackLeft);
                    node.CenterBackVoxel = generator.GenerateNoGradient(node.CenterBack);
                    node.CenterBackRightVoxel = generator.GenerateNoGradient(node.CenterBackRight);
                    node.CenterRightVoxel = generator.GenerateNoGradient(node.CenterRight);
                    node.CenterFrontRightVoxel = generator.GenerateNoGradient(node.CenterFrontRight);
                    node.CenterFrontVoxel = generator.GenerateNoGradient(node.CenterFront);
                    node.CenterFrontLeftVoxel = generator.GenerateNoGradient(node.CenterFrontLeft);
                    node.CenterLeftVoxel = generator.GenerateNoGradient(node.CenterLeft);

                    // Generate top voxels
                    node.CenterBackTopVoxel = generator.GenerateNoGradient(node.CenterBackTop);
                    node.CenterLeftTopVoxel = generator.GenerateNoGradient(node.CenterLeftTop);
                    node.CenterTopVoxel = generator.GenerateNoGradient(node.CenterTop);
                    node.CenterRightTopVoxel = generator.GenerateNoGradient(node.CenterRightTop);
                    node.CenterFrontTopVoxel = generator.GenerateNoGradient(node.CenterFrontTop);
                }

                // Generate corners
                node.Corner0Voxel = generator.GenerateNoGradient(node.Corner0);
                node.Corner1Voxel = generator.GenerateNoGradient(node.Corner1);
                node.Corner2Voxel = generator.GenerateNoGradient(node.Corner2);
                node.Corner3Voxel = generator.GenerateNoGradient(node.Corner3);
                node.Corner4Voxel = generator.GenerateNoGradient(node.Corner4);
                node.Corner5Voxel = generator.GenerateNoGradient(node.Corner5);
                node.Corner6Voxel = generator.GenerateNoGradient(node.Corner6);
                node.Corner7Voxel = generator.GenerateNoGradient(node.Corner7);
            } else {
                switch (index) {
                    case 0:
                        if (node.Level > Param.SIZE_OFFSET) {
                            // Generate bottom voxels
                            node.CenterBackBottomVoxel = generator.GenerateNoGradient(node.CenterBackBottom);
                            node.CenterLeftBottomVoxel = generator.GenerateNoGradient(node.CenterLeftBottom);
                            node.CenterBottomVoxel = generator.GenerateNoGradient(node.CenterBottom);
                            node.CenterRightBottomVoxel = generator.GenerateNoGradient(node.CenterRightBottom);
                            node.CenterFrontBottomVoxel = generator.GenerateNoGradient(node.CenterFrontBottom);

                            // Generate side voxels
                            node.CenterBackLeftVoxel = generator.GenerateNoGradient(node.CenterBackLeft);
                            node.CenterBackVoxel = generator.GenerateNoGradient(node.CenterBack);
                            node.CenterBackRightVoxel = generator.GenerateNoGradient(node.CenterBackRight);
                            node.CenterRightVoxel = generator.GenerateNoGradient(node.CenterRight);
                            node.CenterFrontRightVoxel = generator.GenerateNoGradient(node.CenterFrontRight);
                            node.CenterFrontVoxel = generator.GenerateNoGradient(node.CenterFront);
                            node.CenterFrontLeftVoxel = generator.GenerateNoGradient(node.CenterFrontLeft);
                            node.CenterLeftVoxel = generator.GenerateNoGradient(node.CenterLeft);

                            // Generate top voxels
                            node.CenterBackTopVoxel = generator.GenerateNoGradient(node.CenterBackTop);
                            node.CenterLeftTopVoxel = generator.GenerateNoGradient(node.CenterLeftTop);
                            node.CenterTopVoxel = generator.GenerateNoGradient(node.CenterTop);
                            node.CenterRightTopVoxel = generator.GenerateNoGradient(node.CenterRightTop);
                            node.CenterFrontTopVoxel = generator.GenerateNoGradient(node.CenterFrontTop);
                        }

                        // Get existing voxels from parent corners
                        node.Corner0Voxel = parent.Corner0Voxel;
                        node.Corner1Voxel = parent.CenterBackBottomVoxel;
                        node.Corner2Voxel = parent.CenterBottomVoxel;
                        node.Corner3Voxel = parent.CenterLeftBottomVoxel;
                        node.Corner4Voxel = parent.CenterBackLeftVoxel;
                        node.Corner5Voxel = parent.CenterBackVoxel;
                        node.Corner6Voxel = parent.CenterVoxel;
                        node.Corner7Voxel = parent.CenterLeftVoxel;
                        break;
                    case 1:
                        if (node.Level > Param.SIZE_OFFSET) {
                            // Generate bottom voxels
                            node.CenterBackBottomVoxel = generator.GenerateNoGradient(node.CenterBackBottom);
                            node.CenterBottomVoxel = generator.GenerateNoGradient(node.CenterBottom);
                            node.CenterRightBottomVoxel = generator.GenerateNoGradient(node.CenterRightBottom);
                            node.CenterFrontBottomVoxel = generator.GenerateNoGradient(node.CenterFrontBottom);

                            // Generate side voxels
                            node.CenterBackVoxel = generator.GenerateNoGradient(node.CenterBack);
                            node.CenterBackRightVoxel = generator.GenerateNoGradient(node.CenterBackRight);
                            node.CenterRightVoxel = generator.GenerateNoGradient(node.CenterRight);
                            node.CenterFrontRightVoxel = generator.GenerateNoGradient(node.CenterFrontRight);
                            node.CenterFrontVoxel = generator.GenerateNoGradient(node.CenterFront);

                            // Generate top voxels
                            node.CenterBackTopVoxel = generator.GenerateNoGradient(node.CenterBackTop);
                            node.CenterTopVoxel = generator.GenerateNoGradient(node.CenterTop);
                            node.CenterRightTopVoxel = generator.GenerateNoGradient(node.CenterRightTop);
                            node.CenterFrontTopVoxel = generator.GenerateNoGradient(node.CenterFrontTop);

                            // Get existing voxels from left
                            node.CenterLeftVoxel = n0.CenterRightVoxel;
                            node.CenterLeftBottomVoxel = n0.CenterRightBottomVoxel;
                            node.CenterLeftTopVoxel = n0.CenterRightTopVoxel;
                            node.CenterBackLeftVoxel = n0.CenterBackRightVoxel;
                            node.CenterFrontLeftVoxel = n0.CenterFrontRightVoxel;
                        }

                        // Get existing voxels from parent corners
                        node.Corner0Voxel = parent.CenterBackBottomVoxel;
                        node.Corner1Voxel = parent.Corner1Voxel;
                        node.Corner2Voxel = parent.CenterRightBottomVoxel;
                        node.Corner3Voxel = parent.CenterBottomVoxel;
                        node.Corner4Voxel = parent.CenterBackVoxel;
                        node.Corner5Voxel = parent.CenterBackRightVoxel;
                        node.Corner6Voxel = parent.CenterRightVoxel;
                        node.Corner7Voxel = parent.CenterVoxel;

                        break;
                    case 2:
                        if (node.Level > Param.SIZE_OFFSET) {
                            // Generate bottom voxels
                            node.CenterLeftBottomVoxel = generator.GenerateNoGradient(node.CenterLeftBottom);
                            node.CenterBottomVoxel = generator.GenerateNoGradient(node.CenterBottom);
                            node.CenterRightBottomVoxel = generator.GenerateNoGradient(node.CenterRightBottom);
                            node.CenterFrontBottomVoxel = generator.GenerateNoGradient(node.CenterFrontBottom);

                            // Generate side voxels
                            node.CenterRightVoxel = generator.GenerateNoGradient(node.CenterRight);
                            node.CenterFrontRightVoxel = generator.GenerateNoGradient(node.CenterFrontRight);
                            node.CenterFrontVoxel = generator.GenerateNoGradient(node.CenterFront);
                            node.CenterFrontLeftVoxel = generator.GenerateNoGradient(node.CenterFrontLeft);
                            node.CenterLeftVoxel = generator.GenerateNoGradient(node.CenterLeft);

                            // Generate top voxels
                            node.CenterLeftTopVoxel = generator.GenerateNoGradient(node.CenterLeftTop);
                            node.CenterTopVoxel = generator.GenerateNoGradient(node.CenterTop);
                            node.CenterRightTopVoxel = generator.GenerateNoGradient(node.CenterRightTop);
                            node.CenterFrontTopVoxel = generator.GenerateNoGradient(node.CenterFrontTop);

                            // Get existing voxels from back
                            node.CenterBackVoxel = n1.CenterFrontVoxel;
                            node.CenterBackLeftVoxel = n1.CenterFrontLeftVoxel;
                            node.CenterBackRightVoxel = n1.CenterFrontRightVoxel;
                            node.CenterBackBottomVoxel = n1.CenterFrontBottomVoxel;
                            node.CenterBackTopVoxel = n1.CenterFrontTopVoxel;
                        }

                        // Get existing voxels from parent corners
                        node.Corner0Voxel = parent.CenterBottomVoxel;
                        node.Corner1Voxel = parent.CenterRightBottomVoxel;
                        node.Corner2Voxel = parent.Corner2Voxel;
                        node.Corner3Voxel = parent.CenterFrontBottomVoxel;
                        node.Corner4Voxel = parent.CenterVoxel;
                        node.Corner5Voxel = parent.CenterRightVoxel;
                        node.Corner6Voxel = parent.CenterFrontRightVoxel;
                        node.Corner7Voxel = parent.CenterFrontVoxel;

                        break;
                    case 3:
                        if (node.Level > Param.SIZE_OFFSET) {
                            // Generate bottom voxels
                            node.CenterLeftBottomVoxel = generator.GenerateNoGradient(node.CenterLeftBottom);
                            node.CenterBottomVoxel = generator.GenerateNoGradient(node.CenterBottom);
                            node.CenterFrontBottomVoxel = generator.GenerateNoGradient(node.CenterFrontBottom);

                            // Generate side voxels
                            node.CenterFrontVoxel = generator.GenerateNoGradient(node.CenterFront);
                            node.CenterFrontLeftVoxel = generator.GenerateNoGradient(node.CenterFrontLeft);
                            node.CenterLeftVoxel = generator.GenerateNoGradient(node.CenterLeft);

                            // Generate top voxels
                            node.CenterLeftTopVoxel = generator.GenerateNoGradient(node.CenterLeftTop);
                            node.CenterTopVoxel = generator.GenerateNoGradient(node.CenterTop);
                            node.CenterFrontTopVoxel = generator.GenerateNoGradient(node.CenterFrontTop);

                            // Get existing voxels from right
                            node.CenterRightVoxel = n2.CenterLeftVoxel;
                            node.CenterRightBottomVoxel = n2.CenterLeftBottomVoxel;
                            node.CenterRightTopVoxel = n2.CenterLeftTopVoxel;
                            node.CenterBackRightVoxel = n2.CenterBackLeftVoxel;
                            node.CenterFrontRightVoxel = n2.CenterFrontLeftVoxel;

                            // Get existing voxels from back
                            node.CenterBackVoxel = n0.CenterFrontVoxel;
                            node.CenterBackLeftVoxel = n0.CenterFrontLeftVoxel;
                            //--datavoxels.centerBackRightVoxel = n0.centerFrontRightVoxel;
                            node.CenterBackBottomVoxel = n0.CenterFrontBottomVoxel;
                            node.CenterBackTopVoxel = n0.CenterFrontTopVoxel;
                        }

                        // Get existing voxels from parent corners
                        node.Corner0Voxel = parent.CenterLeftBottomVoxel;
                        node.Corner1Voxel = parent.CenterBottomVoxel;
                        node.Corner2Voxel = parent.CenterFrontBottomVoxel;
                        node.Corner3Voxel = parent.Corner3Voxel;
                        node.Corner4Voxel = parent.CenterLeftVoxel;
                        node.Corner5Voxel = parent.CenterVoxel;
                        node.Corner6Voxel = parent.CenterFrontVoxel;
                        node.Corner7Voxel = parent.CenterFrontLeftVoxel;

                        break;
                    case 4:
                        if (node.Level > Param.SIZE_OFFSET) {
                            // Generate bottom voxels
                            // nothing to do

                            // Generate side voxels
                            node.CenterBackLeftVoxel = generator.GenerateNoGradient(node.CenterBackLeft);
                            node.CenterBackVoxel = generator.GenerateNoGradient(node.CenterBack);
                            node.CenterBackRightVoxel = generator.GenerateNoGradient(node.CenterBackRight);
                            node.CenterRightVoxel = generator.GenerateNoGradient(node.CenterRight);
                            node.CenterFrontRightVoxel = generator.GenerateNoGradient(node.CenterFrontRight);
                            node.CenterFrontVoxel = generator.GenerateNoGradient(node.CenterFront);
                            node.CenterFrontLeftVoxel = generator.GenerateNoGradient(node.CenterFrontLeft);
                            node.CenterLeftVoxel = generator.GenerateNoGradient(node.CenterLeft);

                            // Generate top voxels
                            node.CenterBackTopVoxel = generator.GenerateNoGradient(node.CenterBackTop);
                            node.CenterLeftTopVoxel = generator.GenerateNoGradient(node.CenterLeftTop);
                            node.CenterTopVoxel = generator.GenerateNoGradient(node.CenterTop);
                            node.CenterRightTopVoxel = generator.GenerateNoGradient(node.CenterRightTop);
                            node.CenterFrontTopVoxel = generator.GenerateNoGradient(node.CenterFrontTop);

                            // Get existing voxels from bottom
                            node.CenterBottomVoxel = n0.CenterTopVoxel;
                            node.CenterLeftBottomVoxel = n0.CenterLeftTopVoxel;
                            node.CenterRightBottomVoxel = n0.CenterRightTopVoxel;
                            node.CenterBackBottomVoxel = n0.CenterBackTopVoxel;
                            node.CenterFrontBottomVoxel = n0.CenterFrontTopVoxel;
                        }

                        // Get existing voxels from parent corners
                        node.Corner0Voxel = parent.CenterBackLeftVoxel;
                        node.Corner1Voxel = parent.CenterBackVoxel;
                        node.Corner2Voxel = parent.CenterVoxel;
                        node.Corner3Voxel = parent.CenterLeftVoxel;
                        node.Corner4Voxel = parent.Corner4Voxel;
                        node.Corner5Voxel = parent.CenterBackTopVoxel;
                        node.Corner6Voxel = parent.CenterTopVoxel;
                        node.Corner7Voxel = parent.CenterLeftTopVoxel;

                        break;
                    case 5:
                        if (node.Level > Param.SIZE_OFFSET) {
                            // Generate bottom voxels
                            // nothing to do

                            // Generate side voxels
                            node.CenterBackVoxel = generator.GenerateNoGradient(node.CenterBack);
                            node.CenterBackRightVoxel = generator.GenerateNoGradient(node.CenterBackRight);
                            node.CenterRightVoxel = generator.GenerateNoGradient(node.CenterRight);
                            node.CenterFrontRightVoxel = generator.GenerateNoGradient(node.CenterFrontRight);
                            node.CenterFrontVoxel = generator.GenerateNoGradient(node.CenterFront);

                            // Generate top voxels
                            node.CenterBackTopVoxel = generator.GenerateNoGradient(node.CenterBackTop);
                            node.CenterTopVoxel = generator.GenerateNoGradient(node.CenterTop);
                            node.CenterRightTopVoxel = generator.GenerateNoGradient(node.CenterRightTop);
                            node.CenterFrontTopVoxel = generator.GenerateNoGradient(node.CenterFrontTop);

                            // Get existing voxels from left
                            node.CenterLeftVoxel = n4.CenterRightVoxel;
                            node.CenterLeftBottomVoxel = n4.CenterRightBottomVoxel;
                            node.CenterLeftTopVoxel = n4.CenterRightTopVoxel;
                            node.CenterBackLeftVoxel = n4.CenterBackRightVoxel;
                            node.CenterFrontLeftVoxel = n4.CenterFrontRightVoxel;

                            // Get existing voxels from bottom
                            node.CenterBottomVoxel = n1.CenterTopVoxel;
                            //--datavoxels.centerLeftBottomVoxel = n1.centerLeftTopVoxel;
                            node.CenterRightBottomVoxel = n1.CenterRightTopVoxel;
                            node.CenterBackBottomVoxel = n1.CenterBackTopVoxel;
                            node.CenterFrontBottomVoxel = n1.CenterFrontTopVoxel;
                        }

                        // Get existing voxels from parent corners
                        node.Corner0Voxel = parent.CenterBackVoxel;
                        node.Corner1Voxel = parent.CenterBackRightVoxel;
                        node.Corner2Voxel = parent.CenterRightVoxel;
                        node.Corner3Voxel = parent.CenterVoxel;
                        node.Corner4Voxel = parent.CenterBackTopVoxel;
                        node.Corner5Voxel = parent.Corner5Voxel;
                        node.Corner6Voxel = parent.CenterRightTopVoxel;
                        node.Corner7Voxel = parent.CenterTopVoxel;

                        break;
                    case 6:
                        if (node.Level > Param.SIZE_OFFSET) {
                            // Generate bottom voxels
                            // nothing to do

                            // Generate side voxels
                            node.CenterRightVoxel = generator.GenerateNoGradient(node.CenterRight);
                            node.CenterFrontRightVoxel = generator.GenerateNoGradient(node.CenterFrontRight);
                            node.CenterFrontVoxel = generator.GenerateNoGradient(node.CenterFront);
                            node.CenterFrontLeftVoxel = generator.GenerateNoGradient(node.CenterFrontLeft);
                            node.CenterLeftVoxel = generator.GenerateNoGradient(node.CenterLeft);

                            // Generate top voxels
                            node.CenterLeftTopVoxel = generator.GenerateNoGradient(node.CenterLeftTop);
                            node.CenterTopVoxel = generator.GenerateNoGradient(node.CenterTop);
                            node.CenterRightTopVoxel = generator.GenerateNoGradient(node.CenterRightTop);
                            node.CenterFrontTopVoxel = generator.GenerateNoGradient(node.CenterFrontTop);

                            // Get existing voxels from back
                            node.CenterBackVoxel = n5.CenterFrontVoxel;
                            node.CenterBackLeftVoxel = n5.CenterFrontLeftVoxel;
                            node.CenterBackRightVoxel = n5.CenterFrontRightVoxel;
                            node.CenterBackBottomVoxel = n5.CenterFrontBottomVoxel;
                            node.CenterBackTopVoxel = n5.CenterFrontTopVoxel;

                            // Get existing voxels from bottom
                            node.CenterBottomVoxel = n2.CenterTopVoxel;
                            node.CenterLeftBottomVoxel = n2.CenterLeftTopVoxel;
                            node.CenterRightBottomVoxel = n2.CenterRightTopVoxel;
                            //--datavoxels.centerBackBottomVoxel = n2.centerBackTopVoxel;
                            node.CenterFrontBottomVoxel = n2.CenterFrontTopVoxel;
                        }

                        // Get existing voxels from parent corners
                        node.Corner0Voxel = parent.CenterVoxel;
                        node.Corner1Voxel = parent.CenterRightVoxel;
                        node.Corner2Voxel = parent.CenterFrontRightVoxel;
                        node.Corner3Voxel = parent.CenterFrontVoxel;
                        node.Corner4Voxel = parent.CenterTopVoxel;
                        node.Corner5Voxel = parent.CenterRightTopVoxel;
                        node.Corner6Voxel = parent.Corner6Voxel;
                        node.Corner7Voxel = parent.CenterFrontTopVoxel;

                        break;
                    case 7:
                        if (node.Level > Param.SIZE_OFFSET) {
                            // Generate bottom voxels
                            // nothing to do

                            // Generate side voxels
                            node.CenterFrontVoxel = generator.GenerateNoGradient(node.CenterFront);
                            node.CenterFrontLeftVoxel = generator.GenerateNoGradient(node.CenterFrontLeft);
                            node.CenterLeftVoxel = generator.GenerateNoGradient(node.CenterLeft);

                            // Generate top voxels
                            node.CenterLeftTopVoxel = generator.GenerateNoGradient(node.CenterLeftTop);
                            node.CenterTopVoxel = generator.GenerateNoGradient(node.CenterTop);
                            node.CenterFrontTopVoxel = generator.GenerateNoGradient(node.CenterFrontTop);

                            // Get existing voxels from right
                            node.CenterRightVoxel = n6.CenterLeftVoxel;
                            node.CenterRightBottomVoxel = n6.CenterLeftBottomVoxel;
                            node.CenterRightTopVoxel = n6.CenterLeftTopVoxel;
                            node.CenterBackRightVoxel = n6.CenterBackLeftVoxel;
                            node.CenterFrontRightVoxel = n6.CenterFrontLeftVoxel;

                            // Get existing voxels from back
                            node.CenterBackVoxel = n4.CenterFrontVoxel;
                            node.CenterBackLeftVoxel = n4.CenterFrontLeftVoxel;
                            //--datavoxels.centerBackRightVoxel = n4.centerFrontRightVoxel;
                            node.CenterBackBottomVoxel = n4.CenterFrontBottomVoxel;
                            node.CenterBackTopVoxel = n4.CenterFrontTopVoxel;

                            // Get existing voxels from bottom
                            node.CenterBottomVoxel = n3.CenterTopVoxel;
                            node.CenterLeftBottomVoxel = n3.CenterLeftTopVoxel;
                            //--datavoxels.centerRightBottomVoxel = n3.centerRightTopVoxel;
                            //--datavoxels.centerBackBottomVoxel = n3.centerBackTopVoxel;
                            node.CenterFrontBottomVoxel = n3.CenterFrontTopVoxel;
                        }

                        // Get existing voxels from parent corners
                        node.Corner0Voxel = parent.CenterLeftVoxel;
                        node.Corner1Voxel = parent.CenterVoxel;
                        node.Corner2Voxel = parent.CenterFrontVoxel;
                        node.Corner3Voxel = parent.CenterFrontLeftVoxel;
                        node.Corner4Voxel = parent.CenterLeftTopVoxel;
                        node.Corner5Voxel = parent.CenterTopVoxel;
                        node.Corner6Voxel = parent.CenterFrontTopVoxel;
                        node.Corner7Voxel = parent.Corner7Voxel;

                        break;
                }
            }

            //UnityEngine.Profiler.EndSample ();
        }
    }
}