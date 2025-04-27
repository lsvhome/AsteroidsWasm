using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Asteroids.Standard.Components;
using Asteroids.Standard.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Asteroids.Standard.Tests
{
    [TestClass]
    public class GeometryHelperTests
    {

        public class IsInsidePolygon_Test_DataItem
        {
            public int Id { get; set; }
            public PointD Point { get; set; }
            public IList<PointD> PolygonPoints { get; set; }
            public bool ExpectedResult { get; set; }

            public object[] GetData()
            {
                return new object[] { Point, PolygonPoints, ExpectedResult, Id };
            }
            public void SetData(object[] data)
            {
                this.Point = (PointD)data[0];
                this.PolygonPoints = (IList<PointD>)data[1];
                this.ExpectedResult = (bool)data[2];
                this.Id = (int)data[3];
            }
        }

        public static IEnumerable<object[]> IsInsidePolygon_Test_Data
        {
            get
            {
                return new[]
                {
                    new IsInsidePolygon_Test_DataItem{ Id = 1, Point = new PointD(0,0), PolygonPoints= new List<PointD>{ new PointD(0,1), new PointD(1,-1) , new PointD(-1,-1) }, ExpectedResult = true }.GetData(),
                    new IsInsidePolygon_Test_DataItem{ Id = 2, 
                        Point = new PointD(4046,2238),
                        PolygonPoints= new List<PointD>
                        {
                            new PointD(9700, 6075),
                            new PointD(9850, 6150),
                            new PointD(9925, 6150),
                            new PointD(9925, 6225),
                            new PointD(10075, 6225),
                            new PointD(10075, 6150),
                            new PointD(10150, 6150),
                            new PointD(10300, 6075),
                            new PointD(10150, 6000),
                            new PointD(9850, 6000)
                        },
                        ExpectedResult = false }.GetData(),
                };
            }
        }

        public static string IsInsidePolygon_Test_DataItem_DisplayName(MethodInfo methodInfo, object[] data)
        {
            var t = new IsInsidePolygon_Test_DataItem();
            t.SetData(data);

            return string.Format("TData id=[{0}]", t.Id);
        }


        [DataTestMethod]
        [DynamicData(nameof(IsInsidePolygon_Test_Data), DynamicDataDisplayName = nameof(IsInsidePolygon_Test_DataItem_DisplayName))]
        public void IsInsidePolygon_Test(PointD point, IList<PointD> polygonPoints, bool expectedResult, int id)
        {
            bool actual = GeometryHelper.IsInsidePolygon(point, polygonPoints);
            Assert.AreEqual(expectedResult, actual);
        }
    }
}
