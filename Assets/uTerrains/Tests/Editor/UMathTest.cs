using System;
using NUnit.Framework;

namespace UltimateTerrains
{
    [TestFixture]
    class UMathTest
    {
        private const float Delta = 0.0001f;

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void DistanceToPlaneTest()
        {
            var point = new Vector3d(0, 0, 0);
            var norm = new Vector3d(0, 1, 0);
            var d = UMath.DistanceToPlane(new Vector3d(0, 3, 0), point, norm);
            Assert.AreEqual(3, d, Delta);

            d = UMath.DistanceToPlane(new Vector3d(10, 3, 40), point, norm);
            Assert.AreEqual(3, d, Delta);

            point = new Vector3d(10, 40, 10);
            norm = new Vector3d(1, 0, 0);
            d = UMath.DistanceToPlane(new Vector3d(18, 3, 40), point, norm);
            Assert.AreEqual(8, d, Delta);

            point = new Vector3d(10, 10, 10);
            norm = (new Vector3d(1, 0, 1)).Normalized;
            d = UMath.DistanceToPlane(new Vector3d(20, 0, 20), point, norm);
            Assert.AreEqual(Math.Sqrt(10 * 10 + 10 * 10), d, Delta);
        }

        [Test]
        public void TrilinearInterpolateTest()
        {
            var point = new Vector3d(0.5, 0.5, 0.5);
            var result = UMath.TrilinearInterpolate(1, 1, 1, 1, 1, 1, 1, 1, point);
            Assert.AreEqual(1, result, Delta);

            point = new Vector3d(0.1, 0.9, 0.3);
            result = UMath.TrilinearInterpolate(10, 10, 10, 10, 10, 10, 10, 10, point);
            Assert.AreEqual(10, result, Delta);

            point = new Vector3d(0.5, 0.5, 0.5);
            result = UMath.TrilinearInterpolate(0, 0, 0, 0, 10, 10, 10, 10, point);
            Assert.AreEqual(5, result, Delta);

            point = new Vector3d(0, 0.5, 0.5);
            result = UMath.TrilinearInterpolate(0, 0, 0, 0, 10, 10, 10, 10, point);
            Assert.AreEqual(0, result, Delta);

            point = new Vector3d(1, 0.5, 0.5);
            result = UMath.TrilinearInterpolate(0, 0, 0, 0, 10, 10, 10, 10, point);
            Assert.AreEqual(10, result, Delta);
        }

        [Test]
        public void BilinearInterpolateTest()
        {
            var result = UMath.BilinearInterpolate(1, 1, 1, 1, 0.5, 0.5);
            Assert.AreEqual(1, result, Delta);
            
            result = UMath.BilinearInterpolate(10, 10, 10, 10, 0.8, 0.1);
            Assert.AreEqual(10, result, Delta);
            
            result = UMath.BilinearInterpolate(0, 0, 10, 10, 0.5, 0.5);
            Assert.AreEqual(5, result, Delta);
        }
    }
}