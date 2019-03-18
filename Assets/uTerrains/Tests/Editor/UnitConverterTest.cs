using NUnit.Framework;

namespace UltimateTerrains
{
    [TestFixture]
    class UnitConverterTest
    {
        Param param;
        UnitConverter unitConverter;

        [SetUp]
        public void SetUp()
        {
            param = new Param();
            param.LevelCount = 4;
            param.ChunkLayer = 8;
            param.Lod2Distance = 2;
            param.MaxLevelIndexCollider = 7;
            param.Init();
            unitConverter = new UnitConverter(param);
        }

        [Test]
        public void ChunkToLeveledChunkPositionTest()
        {
            Vector3i p, r;

            p = new Vector3i(8, -1, 1);
            r = UnitConverter.ChunkToLeveledChunkPosition(p, 1);
            Assert.AreEqual(p, r);

            r = UnitConverter.ChunkToLeveledChunkPosition(p, 2);
            Assert.AreEqual(new Vector3i(8, -2, 0), r);

            r = UnitConverter.ChunkToLeveledChunkPosition(p, 4);
            Assert.AreEqual(new Vector3i(8, -4, 0), r);

            r = UnitConverter.ChunkToLeveledChunkPosition(p, 8);
            Assert.AreEqual(new Vector3i(8, -8, 0), r);

            r = UnitConverter.ChunkToLeveledChunkPosition(p, 16);
            Assert.AreEqual(new Vector3i(0, -16, 0), r);
        }

        [Test]
        public void LevelAtPositionTest()
        {
            Vector3i o = new Vector3i(0, 0, 0);
            Assert.AreEqual(1, unitConverter.LevelAtPosition(o, o));
            Assert.AreEqual(1, unitConverter.LevelAtPosition(o, new Vector3i(3, 0, 0)));
            Assert.AreEqual(1, unitConverter.LevelAtPosition(o, new Vector3i(4, 0, 0)));
            Assert.AreEqual(1, unitConverter.LevelAtPosition(o, new Vector3i(-4, 0, 0)));

            Assert.AreEqual(2, unitConverter.LevelAtPosition(o, new Vector3i(5, 0, 0)));
            Assert.AreEqual(2, unitConverter.LevelAtPosition(o, new Vector3i(6, 0, 0)));
        }
    }
}