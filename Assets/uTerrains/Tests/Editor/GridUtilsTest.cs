using System.Collections.Generic;
using NUnit.Framework;

namespace UltimateTerrains
{
    [TestFixture]
    class GridUtilsTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void TraverseCellsIn3DGrid_X_Test()
        {
            List<Vector3i> traversedCells = new List<Vector3i>();
            GridUtils.TraverseCellsIn3DGrid(new Vector3i(0, 0, 0), new Vector3i(10, 0, 0), (x, y, z) => {
                traversedCells.Add(new Vector3i(x, y, z));
                return false;
            });

            Assert.AreEqual(11, traversedCells.Count);
            for (int i = 0; i < traversedCells.Count; ++i) {
                Assert.AreEqual(new Vector3i(i, 0, 0), traversedCells[i]);
            }
        }

        [Test]
        public void TraverseCellsIn3DGrid_Y_Test()
        {
            List<Vector3i> traversedCells = new List<Vector3i>();
            GridUtils.TraverseCellsIn3DGrid(new Vector3i(0, 0, 0), new Vector3i(0, 10, 0), (x, y, z) => {
                traversedCells.Add(new Vector3i(x, y, z));
                return false;
            });

            Assert.AreEqual(11, traversedCells.Count);
            for (int i = 0; i < traversedCells.Count; ++i) {
                Assert.AreEqual(new Vector3i(0, i, 0), traversedCells[i]);
            }
        }

        [Test]
        public void TraverseCellsIn3DGrid_Z_Test()
        {
            List<Vector3i> traversedCells = new List<Vector3i>();
            GridUtils.TraverseCellsIn3DGrid(new Vector3i(0, 0, 0), new Vector3i(0, 0, 10), (x, y, z) => {
                traversedCells.Add(new Vector3i(x, y, z));
                return false;
            });

            Assert.AreEqual(11, traversedCells.Count);
            for (int i = 0; i < traversedCells.Count; ++i) {
                Assert.AreEqual(new Vector3i(0, 0, i), traversedCells[i]);
            }
        }

        [Test]
        public void TraverseCellsIn3DGrid_mX_Test()
        {
            List<Vector3i> traversedCells = new List<Vector3i>();
            GridUtils.TraverseCellsIn3DGrid(new Vector3i(0, 0, 0), new Vector3i(-10, 0, 0), (x, y, z) => {
                traversedCells.Add(new Vector3i(x, y, z));
                return false;
            });

            Assert.AreEqual(11, traversedCells.Count);
            for (int i = 0; i < traversedCells.Count; ++i) {
                Assert.AreEqual(new Vector3i(-i, 0, 0), traversedCells[i]);
            }
        }

        [Test]
        public void TraverseCellsIn3DGrid_mY_Test()
        {
            List<Vector3i> traversedCells = new List<Vector3i>();
            GridUtils.TraverseCellsIn3DGrid(new Vector3i(0, 0, 0), new Vector3i(0, -10, 0), (x, y, z) => {
                traversedCells.Add(new Vector3i(x, y, z));
                return false;
            });

            Assert.AreEqual(11, traversedCells.Count);
            for (int i = 0; i < traversedCells.Count; ++i) {
                Assert.AreEqual(new Vector3i(0, -i, 0), traversedCells[i]);
            }
        }

        [Test]
        public void TraverseCellsIn3DGrid_mZ_Test()
        {
            List<Vector3i> traversedCells = new List<Vector3i>();
            GridUtils.TraverseCellsIn3DGrid(new Vector3i(0, 0, 0), new Vector3i(0, 0, -10), (x, y, z) => {
                traversedCells.Add(new Vector3i(x, y, z));
                return false;
            });

            Assert.AreEqual(11, traversedCells.Count);
            for (int i = 0; i < traversedCells.Count; ++i) {
                Assert.AreEqual(new Vector3i(0, 0, -i), traversedCells[i]);
            }
        }

        [Test]
        public void TraverseCellsIn3DGrid_XY_Test()
        {
            List<Vector3i> traversedCells = new List<Vector3i>();
            GridUtils.TraverseCellsIn3DGrid(new Vector3i(0, 0, 0), new Vector3i(8, 3, 0), (x, y, z) => {
                traversedCells.Add(new Vector3i(x, y, z));
                return false;
            });

            Assert.AreEqual(12, traversedCells.Count);
            int i = 0;
            Assert.AreEqual(new Vector3i(0, 0, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(1, 0, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(1, 1, 0), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(2, 1, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 1, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(4, 1, 0), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(4, 2, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(5, 2, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(6, 2, 0), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(6, 3, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(7, 3, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(8, 3, 0), traversedCells[i++]);
        }

        [Test]
        public void TraverseCellsIn3DGrid_XZ_Test()
        {
            List<Vector3i> traversedCells = new List<Vector3i>();
            GridUtils.TraverseCellsIn3DGrid(new Vector3i(0, 0, 0), new Vector3i(8, 0, 3), (x, y, z) => {
                traversedCells.Add(new Vector3i(x, y, z));
                return false;
            });

            Assert.AreEqual(12, traversedCells.Count);
            int i = 0;
            Assert.AreEqual(new Vector3i(0, 0, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(1, 0, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(1, 0, 1), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(2, 0, 1), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 0, 1), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(4, 0, 1), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(4, 0, 2), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(5, 0, 2), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(6, 0, 2), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(6, 0, 3), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(7, 0, 3), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(8, 0, 3), traversedCells[i++]);
        }

        [Test]
        public void TraverseCellsIn3DGrid_YZ_Test()
        {
            List<Vector3i> traversedCells = new List<Vector3i>();
            GridUtils.TraverseCellsIn3DGrid(new Vector3i(0, 0, 0), new Vector3i(0, 8, 3), (x, y, z) => {
                traversedCells.Add(new Vector3i(x, y, z));
                return false;
            });

            Assert.AreEqual(12, traversedCells.Count);
            int i = 0;
            Assert.AreEqual(new Vector3i(0, 0, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(0, 1, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(0, 1, 1), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(0, 2, 1), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(0, 3, 1), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(0, 4, 1), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(0, 4, 2), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(0, 5, 2), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(0, 6, 2), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(0, 6, 3), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(0, 7, 3), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(0, 8, 3), traversedCells[i++]);
        }

        [Test]
        public void TraverseCellsIn3DGrid_YZ_offset_Test()
        {
            List<Vector3i> traversedCells = new List<Vector3i>();
            GridUtils.TraverseCellsIn3DGrid(new Vector3i(3, -3, 5), new Vector3i(3, 8 - 3, 3 + 5), (x, y, z) => {
                traversedCells.Add(new Vector3i(x, y, z));
                return false;
            });

            Assert.AreEqual(12, traversedCells.Count);
            int i = 0;
            Assert.AreEqual(new Vector3i(3, 0 - 3, 0 + 5), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 1 - 3, 0 + 5), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 1 - 3, 1 + 5), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(3, 2 - 3, 1 + 5), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 3 - 3, 1 + 5), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 4 - 3, 1 + 5), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(3, 4 - 3, 2 + 5), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 5 - 3, 2 + 5), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 6 - 3, 2 + 5), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(3, 6 - 3, 3 + 5), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 7 - 3, 3 + 5), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 8 - 3, 3 + 5), traversedCells[i++]);
        }

        [Test]
        public void TraverseCellsIn3DGrid_XYZ_Test()
        {
            List<Vector3i> traversedCells = new List<Vector3i>();
            GridUtils.TraverseCellsIn3DGrid(new Vector3i(0, 0, 0), new Vector3i(6, 8, 3), (x, y, z) => {
                traversedCells.Add(new Vector3i(x, y, z));
                return false;
            });

            Assert.AreEqual(21, traversedCells.Count);
            int i = 0;
            Assert.AreEqual(new Vector3i(0, 0, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(1, 0, 0), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(1, 1, 0), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(1, 1, 1), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(1, 2, 1), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(2, 2, 1), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(2, 3, 1), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 3, 1), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 4, 1), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(4, 4, 1), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(3, 4, 2), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(4, 4, 2), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(4, 5, 2), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(4, 6, 2), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(5, 6, 2), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(4, 6, 3), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(5, 6, 3), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(5, 7, 3), traversedCells[i++]);

            Assert.AreEqual(new Vector3i(6, 7, 3), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(6, 8, 3), traversedCells[i++]);
            Assert.AreEqual(new Vector3i(7, 8, 3), traversedCells[i++]);
        }
    }
}