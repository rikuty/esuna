using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
#if NET_4_6
using System.Collections.Concurrent;
#else
using UltimateTerrains.Concurrent.NET3x;
#endif

namespace UltimateTerrains
{
    [TestFixture]
    class UltimateOperationsManagerTest
    {
        Param param;
        UnitConverter converter;
        VoxelTypeSet voxelTypeSet;
        VoxelType defaultVoxelType;
        UltimateTerrain ut;
        UltimateOperationsManager opManager;

        [SetUp]
        public void SetUp()
        {
            param = new Param();
            param.LevelCount = 4;
            param.ChunkLayer = 8;
            param.MaxLevelIndexCollider = 7;
            param.Init();

            converter = new UnitConverter(param);

            var voxelTypes = new Dictionary<string, VoxelType>();
            defaultVoxelType = new VoxelType();
            defaultVoxelType.Name = "Default";
            voxelTypes.Add("Default", defaultVoxelType);
            voxelTypeSet = new VoxelTypeSet();
            TestUtils.SetPrivateFieldValue(voxelTypeSet, "voxelTypes", voxelTypes);

            ut = new UltimateTerrain();
            TestUtils.SetPrivateFieldValue(ut, "param", param);
            ut.InitFastAccessToParams();
            TestUtils.SetPrivateFieldValue(ut, "converter", converter);
            TestUtils.SetPrivateFieldValue(ut, "voxelTypeSet", voxelTypeSet);

            opManager = TestUtils.CallPrivateConstructor<UltimateOperationsManager>(typeof(UltimateTerrain)).WithParams(ut);
            TestUtils.SetPrivateFieldValue(opManager, "operationDataPath", Application.temporaryCachePath);
        }

        [Test]
        public void GetFilenameFromRegionTest()
        {
            string result = TestUtils.CallPrivateMethod<string>(opManager, "GetFilenameFromRegion").WithParams(new Vector3i(8, 0, -9), true);
            Assert.AreEqual("ops_8_0_-9.utd.json", result);
        }

        [Test]
        public void GetRegionFromFilenameTest()
        {
            Vector3i result = TestUtils.CallPrivateMethod<Vector3i>(opManager, "GetRegionFromFilename").WithParams("ops_-3_1_0.utd.json");
            Assert.AreEqual(new Vector3i(-3, 1, 0), result);
        }

        [Test]
        public void GetRegionFromFilenameTest_no_ext()
        {
            Vector3i result = TestUtils.CallPrivateMethod<Vector3i>(opManager, "GetRegionFromFilename").WithParams("ops_-3_1_0");
            Assert.AreEqual(new Vector3i(-3, 1, 0), result);
        }

        [Test]
        public void GetAllRegionsTest_ok()
        {
            string path = "Assets/uTerrains/Tests/Test resources/uTdata01";
            List<Vector3i> result = TestUtils.CallPrivateMethod<List<Vector3i>>(opManager, "GetAllRegions").WithParams(path);
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
            Assert.Contains(new Vector3i(0, 0, 0), result);
            Assert.Contains(new Vector3i(0, -9, -2), result);
            Assert.Contains(new Vector3i(0, -40, -20), result);
            Assert.Contains(new Vector3i(-4, 1, 200), result);
        }

        [Test]
        public void GetAllRegionsTest_dir_dont_exist()
        {
            string path = "Assets/uTerrains/Tests/Test resources/dir-that-doesnt-exist";
            List<Vector3i> result = TestUtils.CallPrivateMethod<List<Vector3i>>(opManager, "GetAllRegions").WithParams(path);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void LoadAllTest()
        {
            Vector3i region = new Vector3i(0, 0, 0);
            string filename = "ops_" + region.x + "_" + region.y + "_" + region.z + ".utd.json";
            string json = "[" +
                          "{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"p\":\"2.0;2.0;2.0\",\"l\":\"4;0;0\",\"h\":\"0;4;0\",\"w\":\"0;0;4\",\"v\":\"Default\"}," +
                          "{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"p\":\"-2.0;0;2.0\",\"l\":\"8;0;0\",\"h\":\"0;8;0\",\"w\":\"0;0;8\",\"v\":\"Default\"}]";
            File.WriteAllText(Application.temporaryCachePath + "/" + filename, json);
            opManager.LoadAll();
            File.Delete(Application.temporaryCachePath + "/" + filename);
            var operations = TestUtils.GetPrivateFieldValue<ConcurrentDictionary<Vector3i, OperationList>[]>(opManager, "operations");
            Assert.AreEqual(7, operations.Length);
            Assert.AreEqual(1, operations[0].Count);
            OperationList ops;
            Assert.IsTrue(operations[0].TryGetValue(new Vector3i(0, 0, 0), out ops));
            Assert.AreEqual(2, ops.Count);
        }
        
        [Test]
        public void LoadAllTest_WrongRegion()
        {
            Vector3i region = new Vector3i(-64, 128, 64);
            string filename = "ops_" + region.x + "_" + region.y + "_" + region.z + ".utd.json";
            string json = "[" +
                          "{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"p\":\"2.0;2.0;2.0\",\"l\":\"4;0;0\",\"h\":\"0;4;0\",\"w\":\"0;0;4\",\"v\":\"Default\"}," +
                          "{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"p\":\"-2.0;0;2.0\",\"l\":\"8;0;0\",\"h\":\"0;8;0\",\"w\":\"0;0;8\",\"v\":\"Default\"}]";
            File.WriteAllText(Application.temporaryCachePath + "/" + filename, json);
            opManager.LoadAll();
            File.Delete(Application.temporaryCachePath + "/" + filename);
            var operations = TestUtils.GetPrivateFieldValue<ConcurrentDictionary<Vector3i, OperationList>[]>(opManager, "operations");
            Assert.AreEqual(7, operations.Length);
            Assert.AreEqual(0, operations[0].Count);
        }

        [Test]
        public void DeserializeOperationTest()
        {
            string json = "{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"p\":\"1.123457;2;3.2\",\"l\":\"4.1;0;0\",\"h\":\"0;4;0\",\"w\":\"0;0;4\",\"v\":\"footype\"}";
            IOperation iop = opManager.DeserializeOperation(json);
            Assert.AreEqual(typeof(AxisAlignedCube), iop.GetType());
            AxisAlignedCube op = (AxisAlignedCube) iop;
            Vector3d corner = TestUtils.GetPrivateFieldValue<Vector3d>(op, "corner");
            Vector3d vL = TestUtils.GetPrivateFieldValue<Vector3d>(op, "vL");
            Vector3d vH = TestUtils.GetPrivateFieldValue<Vector3d>(op, "vH");
            Vector3d vW = TestUtils.GetPrivateFieldValue<Vector3d>(op, "vW");
            string voxelTypeName = TestUtils.GetPrivateFieldValue<string>(op, "voxelTypeName");
            Assert.AreEqual(new Vector3d(1.123457, 2, 3.2), corner);
            Assert.AreEqual(new Vector3d(4.1, 0, 0), vL);
            Assert.AreEqual(new Vector3d(0, 4, 0), vH);
            Assert.AreEqual(new Vector3d(0, 0, 4), vW);
            Assert.AreEqual("footype", voxelTypeName);
        }

        [Test]
        public void SerializeOperationTest()
        {
            var op = new AxisAlignedCube(ut, true, new Vector3d(1.123456789123456789, 2, 3.2), new Vector3d(4.1, 4, 4));
            string json = opManager.SerializeOperation(op);
            Assert.AreEqual("{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"d\":true,\"p\":\"-0.926543210876543;0;1.2\",\"l\":\"4.1;0;0\",\"h\":\"0;4;0\",\"w\":\"0;0;4\",\"v\":\"\"}", json);
        }

        [Test]
        public void DeserializeOperationListTest()
        {
            string json = "[" +
                          "{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"d\":true,\"p\":\"1.1234578;2;3.2\",\"l\":\"4.1;0;0\",\"h\":\"0;4;0\",\"w\":\"0;0;4\",\"v\":\"footype\"}," +
                          "{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"d\":true,\"p\":\"1.1234578;3;3.2\",\"l\":\"5.1;0;0\",\"h\":\"0;5;0\",\"w\":\"0;0;5\",\"v\":\"footype\"}," +
                          "{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"d\":true,\"p\":\"1.1234578;4;3.2\",\"l\":\"6.1;0;0\",\"h\":\"0;6;0\",\"w\":\"0;0;6\",\"v\":\"footype\"}]";
            List<IOperation> iops = opManager.DeserializeOperations(json);

            Assert.AreEqual(typeof(AxisAlignedCube), iops[0].GetType());
            AxisAlignedCube op1 = (AxisAlignedCube) iops[0];
            Vector3d pos = TestUtils.GetPrivateFieldValue<Vector3d>(op1, "corner");
            Vector3d vL = TestUtils.GetPrivateFieldValue<Vector3d>(op1, "vL");
            Vector3d vH = TestUtils.GetPrivateFieldValue<Vector3d>(op1, "vH");
            Vector3d vW = TestUtils.GetPrivateFieldValue<Vector3d>(op1, "vW");
            string voxelTypeName = TestUtils.GetPrivateFieldValue<string>(op1, "voxelTypeName");
            Assert.AreEqual(new Vector3d(1.1234578, 2, 3.2), pos);
            Assert.AreEqual(new Vector3d(4.1, 0, 0), vL);
            Assert.AreEqual(new Vector3d(0, 4, 0), vH);
            Assert.AreEqual(new Vector3d(0, 0, 4), vW);
            Assert.AreEqual("footype", voxelTypeName);

            Assert.AreEqual(typeof(AxisAlignedCube), iops[1].GetType());
            AxisAlignedCube op2 = (AxisAlignedCube) iops[1];
            pos = TestUtils.GetPrivateFieldValue<Vector3d>(op2, "corner");
            vL = TestUtils.GetPrivateFieldValue<Vector3d>(op2, "vL");
            vH = TestUtils.GetPrivateFieldValue<Vector3d>(op2, "vH");
            vW = TestUtils.GetPrivateFieldValue<Vector3d>(op2, "vW");
            voxelTypeName = TestUtils.GetPrivateFieldValue<string>(op2, "voxelTypeName");
            Assert.AreEqual(new Vector3d(1.1234578, 3, 3.2), pos);
            Assert.AreEqual(new Vector3d(5.1, 0, 0), vL);
            Assert.AreEqual(new Vector3d(0, 5, 0), vH);
            Assert.AreEqual(new Vector3d(0, 0, 5), vW);
            Assert.AreEqual("footype", voxelTypeName);

            Assert.AreEqual(typeof(AxisAlignedCube), iops[2].GetType());
            AxisAlignedCube op3 = (AxisAlignedCube) iops[2];
            pos = TestUtils.GetPrivateFieldValue<Vector3d>(op3, "corner");
            vL = TestUtils.GetPrivateFieldValue<Vector3d>(op3, "vL");
            vH = TestUtils.GetPrivateFieldValue<Vector3d>(op3, "vH");
            vW = TestUtils.GetPrivateFieldValue<Vector3d>(op3, "vW");
            voxelTypeName = TestUtils.GetPrivateFieldValue<string>(op3, "voxelTypeName");
            Assert.AreEqual(new Vector3d(1.1234578, 4, 3.2), pos);
            Assert.AreEqual(new Vector3d(6.1, 0, 0), vL);
            Assert.AreEqual(new Vector3d(0, 6, 0), vH);
            Assert.AreEqual(new Vector3d(0, 0, 6), vW);
            Assert.AreEqual("footype", voxelTypeName);
        }

        [Test]
        public void SerializeListOfOperationsTest()
        {
            List<IOperation> ops = new List<IOperation>();
            ops.Add(new AxisAlignedCube(ut, false, new Vector3d(1.12345678, 2, 3.2), new Vector3d(4.1, 4, 4)));
            ops.Add(new AxisAlignedCube(ut, true, new Vector3d(1.12345678, 3, 3.2), new Vector3d(5.1, 5, 5)));
            ops.Add(new AxisAlignedCube(ut, false, new Vector3d(1.12345678, 4, 3.2), new Vector3d(6.1, 6, 6), defaultVoxelType));
            string json = opManager.SerializeOperations(ops);
            Assert.AreEqual("[" +
                            "{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"d\":false,\"p\":\"-0.92654322;0;1.2\",\"l\":\"4.1;0;0\",\"h\":\"0;4;0\",\"w\":\"0;0;4\",\"v\":\"\"}," +
                            "{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"d\":true,\"p\":\"-1.42654322;0.5;0.7\",\"l\":\"5.1;0;0\",\"h\":\"0;5;0\",\"w\":\"0;0;5\",\"v\":\"\"}," +
                            "{\"$type\":\"AxisAlignedCube, Assembly-CSharp\",\"d\":false,\"p\":\"-1.92654322;1;0.2\",\"l\":\"6.1;0;0\",\"h\":\"0;6;0\",\"w\":\"0;0;6\",\"v\":\"Default\"}" +
                            "]", json);
        }
    }
}