﻿using System;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.GeometryObjects;
using NUnit.Framework;
using Point = Autodesk.DesignScript.Geometry.Point;
using Solid = Revit.GeometryObjects.Solid;

namespace DSRevitNodesTests.GeometryIntersection
{
    [TestFixture]
    public class FaceIntersectTests : RevitNodeTestBase
    {
        [SetUp]
        public void Setup()
        {
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public void TearDown()
        {
            HostFactory.Instance.ShutDown();
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void IntersectingCurveFace_ValidArgs()
        {

            var cube1 = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(0, 0, 0), 20, 20, 20);
            var faces = cube1.Faces;

            // get face with normal in positive y direction
            var faceFirst = faces.First( x=> Math.Abs( x.NormalAtParameter(0.5,0.5).Dot(Vector.ByCoordinates(0,1,0)) - 1 ) < 1e-6 );

            // form a line passing through the object
            var line =
                Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(
                    Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, -50, 0),
                    Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 50, 0));

            var results = Intersect.CurveFace(line, faceFirst);

            Assert.AreEqual(1, results.Length);

            var result = results.First();

            Assert.AreEqual( 60, result.CurveParameter, 1e-9 );
            Assert.AreEqual(Point.ByCoordinates(0, 10, 0), result.Point);
            Assert.IsNull(result.Edge);
            Assert.AreEqual(0, result.EdgeParameter);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void IntersectingCurveFace_BadArgs()
        {

            var cube1 = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(0, 0, 0), 20, 20, 20);
            var faces = cube1.Faces;

            // get face with normal in positive y direction
            var faceFirst = faces.First(x => Math.Abs(x.NormalAtParameter(0.5, 0.5).Dot(Vector.ByCoordinates(0, 1, 0)) - 1) < 1e-6);

            // form a line passing through the object
            var line =
                Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(
                    Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, -50, 0),
                    Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 50, 0));

            Assert.Throws(typeof(System.ArgumentNullException), () => Intersect.CurveFace(null, faceFirst));
            Assert.Throws(typeof(System.ArgumentNullException), () => Intersect.CurveFace(line, null));

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void NonIntersectingCurveFace_ValidArgs()
        {

            var cube1 = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(0, 0, 0), 20, 20, 20);
            var faces = cube1.Faces;

            // get face with normal in positive y direction
            var faceFirst = faces.First(x => Math.Abs(x.NormalAtParameter(0.5, 0.5).Dot(Vector.ByCoordinates(0, 1, 0)) - 1) < 1e-6);

            // form a line passing through the object
            var line =
                Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(
                    Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, -50, 100),
                    Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 50, 100));

            var rez = Intersect.CurveFace(line, faceFirst);

            Assert.AreEqual(0,rez.Length);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void IntersectingFaceFace_ValidArgs()
        {

            // build cubes
            var cube1 = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(0, 0, 0), 10, 10, 10);
            var cube2 = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(5, 5, 5), 10, 10, 10);

            // get face on cube 1 facing in + x direction
            var face1 = cube1.Faces.First(x => Math.Abs(x.NormalAtParameter(0.5, 0.5).Dot(Vector.ByCoordinates(1, 0, 0)) - 1) < 1e-6);

            // get face on cube 2 facing in - y direction
            var face2 = cube2.Faces.First(x => Math.Abs(x.NormalAtParameter(0.5, 0.5).Dot(Vector.ByCoordinates(0, -1, 0)) - 1) < 1e-6);

            // intersect the faces
            var result = Intersect.FaceFace(face1, face2);

            Assert.AreEqual(1, result.Length);
            var intersectionCurve = result.First();

            Assert.IsAssignableFrom(typeof(Autodesk.DesignScript.Geometry.Line), intersectionCurve);

            var intersectionLine = intersectionCurve as Autodesk.DesignScript.Geometry.Line;

            // intersection is a length 5 line 
            intersectionLine.Length.ShouldBeApproximately(5);
            intersectionLine.Direction.Dot(Vector.ByCoordinates(1, 0, 0)).ShouldBeApproximately(0);
            intersectionLine.Direction.Dot(Vector.ByCoordinates(0, 1, 0)).ShouldBeApproximately(0);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void IntersectingFaceFace_BadArgs()
        {

            // build cubes
            var cube1 = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(0, 0, 0), 10, 10, 10);
            var cube2 = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(5, 5, 5), 10, 10, 10);

            // get face on cube 1 facing in + x direction
            var face1 = cube1.Faces.First(x => Math.Abs(x.NormalAtParameter(0.5, 0.5).Dot(Vector.ByCoordinates(1, 0, 0)) - 1) < 1e-6);

            // get face on cube 2 facing in - y direction
            var face2 = cube2.Faces.First(x => Math.Abs(x.NormalAtParameter(0.5, 0.5).Dot(Vector.ByCoordinates(0, -1, 0)) - 1) < 1e-6);

            Assert.Throws(typeof(System.ArgumentNullException), () => Intersect.FaceFace(null, face2));
            Assert.Throws(typeof(System.ArgumentNullException), () => Intersect.FaceFace(face1, null));

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void NonIntersectingFaceFace_ValidArgs()
        {

            // build cubes
            var cube1 = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(0, 0, 0), 10, 10, 10);
            var cube2 = Solid.BoxByCenterAndDimensions(Point.ByCoordinates(100,100,100), 10, 10, 10);

            // get face on cube 1 facing in + x direction
            var face1 = cube1.Faces.First();

            // get face on cube 2 facing in - y direction
            var face2 = cube2.Faces.First();

            // intersect the faces
            var result = Intersect.FaceFace(face1, face2);

            Assert.AreEqual(0, result.Length);

        }
    }
}