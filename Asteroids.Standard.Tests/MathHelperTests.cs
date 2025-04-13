using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Asteroids.Standard.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Asteroids.Standard.Tests
{
    [TestClass]
    public class MathHelperTests
    {

        public class Triangle_Test_DataItem
        {
            public int Id { get; set; }
            public PointD A { get; set; }
            public PointD B { get; set; }
            public double angleBetaDegrees { get; set; }
            public double Va { get; set; }
            public double Vb { get; set; }
            public PointD ExpectedC { get; set; }

            public object[] GetData()
            {
                return new object[] { A, B, angleBetaDegrees, Va, Vb, ExpectedC, Id };
            }
            public void SetData(object[] data)
            {
                this.A = (PointD)data[0];
                this.B = (PointD)data[1];
                this.angleBetaDegrees = (double)data[2];
                this.Va = (double)data[3];
                this.Vb = (double)data[4];
                ExpectedC = (PointD)data[5];
                this.Id = (int)data[6];
            }
        }

        public static IEnumerable<object[]> Triangle_Test_Data
        {
            get
            {
                return new[]
                {
                    // https://www.calculator.net/triangle-calculator.html?vc=&vx=4&vy=3&va=&vz=5&vb=&angleunits=d&x=Calculate
                    new Triangle_Test_DataItem{ Id = 1, A = new PointD(0,0), B=new PointD(5,0), angleBetaDegrees= -53.13, Va = 3, Vb= 4, ExpectedC = new PointD(3.2,-2.4) }.GetData(),
                    new Triangle_Test_DataItem{ Id = 2, A = new PointD(0,0), B=new PointD(5,0), angleBetaDegrees= 53.13, Va = 3, Vb= 4, ExpectedC = new PointD(3.2,2.4) }.GetData(),

                    new Triangle_Test_DataItem{ Id = 3, A = new PointD(0,0), B=new PointD(-5,0), angleBetaDegrees= -53.13, Va = 3, Vb= 4, ExpectedC = new PointD(-3.2,2.4) }.GetData(),
                    new Triangle_Test_DataItem{ Id = 4, A = new PointD(0,0), B=new PointD(-5,0), angleBetaDegrees= 53.13, Va = 3, Vb= 4, ExpectedC = new PointD(-3.2, -2.4) }.GetData(),


                    new Triangle_Test_DataItem{ Id = 5, A = new PointD(0,0), B=new PointD(0,5), angleBetaDegrees= -53.13, Va = 3, Vb= 4, ExpectedC = new PointD(2.4, 3.2) }.GetData(),
                    new Triangle_Test_DataItem{ Id = 6, A = new PointD(0,0), B=new PointD(0,5), angleBetaDegrees= +53.13, Va = 3, Vb= 4, ExpectedC = new PointD(-2.4, 3.2) }.GetData(),

                    new Triangle_Test_DataItem{ Id = 7, A = new PointD(0,0), B=new PointD(0,-5), angleBetaDegrees= -53.13, Va = 3, Vb= 4, ExpectedC = new PointD(-2.4,-3.2) }.GetData(),
                    new Triangle_Test_DataItem{ Id = 8, A = new PointD(0,0), B=new PointD(0,-5), angleBetaDegrees= +53.13, Va = 3, Vb= 4, ExpectedC = new PointD(2.4, -3.2) }.GetData(),


                    new Triangle_Test_DataItem{ Id = 9, A = new PointD(0,0), B=new PointD(0,3), angleBetaDegrees= 90, Va = 4, Vb= 5, ExpectedC = new PointD(-4,3) }.GetData(),
                    new Triangle_Test_DataItem{ Id = 10, A = new PointD(1,1), B=new PointD(1,4), angleBetaDegrees= 90, Va = 4, Vb= 5, ExpectedC = new PointD(-3,4) }.GetData(),


                    // https://www.calculator.net/triangle-calculator.html?vc=60&vx=4&vy=4&va=&vz=&vb=&angleunits=d&x=Calculate
                    new Triangle_Test_DataItem{ Id = 21, A = new PointD(0,0), B=new PointD(4,0), angleBetaDegrees= -60, Va = 4, Vb= 4, ExpectedC = new PointD(2,-3.4641) }.GetData(),
                    new Triangle_Test_DataItem{ Id = 21, A = new PointD(0,0), B=new PointD(4,0), angleBetaDegrees= +60, Va = 4, Vb= 4, ExpectedC = new PointD(2,3.4641) }.GetData(),
                };
            }
        }

        public static string Get_Triangle_Test_DataItem_DisplayName(MethodInfo methodInfo, object[] data)
        {
            var t = new Triangle_Test_DataItem();
            t.SetData(data);

            return string.Format("TData id=[{0}]", t.Id);
        }


        [DataTestMethod]
        [DynamicData(nameof(Triangle_Test_Data), DynamicDataDisplayName = nameof(Get_Triangle_Test_DataItem_DisplayName))]
        public void Triangle_Test(PointD A, PointD B, double angleBetaDegrees, double Va, double Vb, PointD expectedC, int id)
        {
            var triangle = MathHelper.GetTriangleInfo(A, B, Va, Vb, MathHelper.ToRadians(angleBetaDegrees));
            Assert.AreEqual(expectedC.X, triangle.C.X, 0.00001,"X");
            Assert.AreEqual(expectedC.Y, triangle.C.Y, 0.00001, "Y");
        }



        public static IEnumerable<object[]> TransformDecartToPolar_Test_Data
        {
            get
            {
                return new[]
                {
                    new TransformDecartToPolar_Test_DataItem{ Id = 1, Vector = new PointD(0,0), Expected = new PolarCoordinates{ Angle = MathHelper.ToRadians(0) /* may be any value */, Distance = 0 }}.GetData(),

                    new TransformDecartToPolar_Test_DataItem{ Id = 2, Vector = new PointD(1,0), Expected = new PolarCoordinates{ Angle = MathHelper.ToRadians(90), Distance = 1 }}.GetData(),
                    new TransformDecartToPolar_Test_DataItem{ Id = 3, Vector = new PointD(-1,0), Expected = new PolarCoordinates{ Angle = MathHelper.ToRadians(-90), Distance = 1 }}.GetData(),
                    new TransformDecartToPolar_Test_DataItem{ Id = 4, Vector = new PointD(0,1), Expected = new PolarCoordinates{ Angle = 0, Distance = 1 }}.GetData(),
                    new TransformDecartToPolar_Test_DataItem{ Id = 5, Vector = new PointD(0,-1), Expected = new PolarCoordinates{ Angle = MathHelper.ToRadians(-180), Distance = 1 }}.GetData(),

                    new TransformDecartToPolar_Test_DataItem{ Id = 6, Vector = new PointD(1,1), Expected = new PolarCoordinates{ Angle = MathHelper.ToRadians(45), Distance = 1.4142 }}.GetData(),
                    new TransformDecartToPolar_Test_DataItem{ Id = 7, Vector = new PointD(-1,1), Expected = new PolarCoordinates{ Angle = MathHelper.ToRadians(-45), Distance = 1.4142 }}.GetData(),
                    new TransformDecartToPolar_Test_DataItem{ Id = 8, Vector = new PointD(1,-1), Expected = new PolarCoordinates{ Angle = MathHelper.ToRadians(+45+90), Distance = 1.4142 }}.GetData(),
                    new TransformDecartToPolar_Test_DataItem{ Id = 9, Vector = new PointD(-1,-1), Expected = new PolarCoordinates{ Angle = MathHelper.ToRadians(-45-90), Distance = 1.4142 }}.GetData(),
                };
            }
        }


        public class TransformDecartToPolar_Test_DataItem
        {
            public int Id { get; set; }
            public PointD Vector { get; set; }
            public PolarCoordinates Expected { get; set; }
            public object[] GetData()
            {
                return new object[] { Vector, Expected, Id };
            }
            public void SetData(object[] data)
            {
                this.Vector = (PointD)data[0];
                this.Expected = (PolarCoordinates)data[1];
                this.Id = (int)data[2];
            }
        }

        public static string Get_TransformDecartToPolar_Test_DataItem_DisplayName(MethodInfo methodInfo, object[] data)
        {
            var t = new TransformDecartToPolar_Test_DataItem();
            t.SetData(data);

            return string.Format("TData id=[{0}]", t.Id);
        }

        [DataTestMethod]
        [DynamicData(nameof(TransformDecartToPolar_Test_Data), DynamicDataDisplayName = nameof(Get_TransformDecartToPolar_Test_DataItem_DisplayName))]
        public void TransformDecartToPolar_Test(PointD vector, PolarCoordinates expected, int id)
        {
            var actual = MathHelper.TransformDecartToPolar(vector);
            Assert.AreEqual(expected.Angle, actual.Angle, 0.00001);
            Assert.AreEqual(expected.Distance, actual.Distance, 0.001);
            var actual2 = MathHelper.TransformPolarToDecart(actual);

            Assert.AreEqual(vector.X, actual2.X, 0.00001);
            Assert.AreEqual(vector.Y, actual2.Y, 0.001);
        }

    }
}
