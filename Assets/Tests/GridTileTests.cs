using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class MockTransform : ITransform
    {
        private Vector3 position = Vector3.zero;
        private Quaternion rotation = Quaternion.identity;
        public Quaternion Rotation { get; set; }
        public Vector3 Position { get; set; }
    }


    public class TileGridTests
    {
        private readonly StructuralTileSet structuralTileSet = new StructuralTileSet("Tiles");
        private readonly MockTransform transform = new MockTransform();

        private readonly Vector2[] uvMappingEmpty =
        {
            new Vector2(0, 0),
            new Vector2(0, 0),
            new Vector2(0, 0),
            new Vector2(0, 0)
        };

        private readonly Vector2[] uvMappingFull =
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        [SetUp]
        public void SetUp()
        {
            transform.Rotation = Quaternion.identity;
            transform.Position = Vector3.zero;
        }

        [Test]
        public void Creation()
        {
            var tileGrid = new TileGrid(1, 1, 1f, transform, structuralTileSet, new TileData());
            Assert.AreEqual(tileGrid.TileSize, 1f);
        }

        [Test]
        public void InBounds([Values(1, 2, 3, 4, 5)] int size)
        {
            var tileGrid = new TileGrid(size, size, 1f, transform, structuralTileSet, new TileData());
            Assert.IsTrue(tileGrid.InGridBounds(-size, -size));
            Assert.IsTrue(tileGrid.InGridBounds(0, -size));
        }

        [Test]
        public void OutOfBounds([Values(1, 2, 3, 4, 5)] int size)
        {
            var tileGrid = new TileGrid(size, size, 1f, transform, structuralTileSet, new TileData());
            Assert.IsFalse(tileGrid.InGridBounds(size, size));
            Assert.IsFalse(tileGrid.InGridBounds(0, size));
        }

        [Test]
        public void WorldToGrid([Values(1e-6f, 0.5f, 1 - 1e-6f)] float xPos,
            [Values(1e-6f, 0.5f, 1 - 1e-6f)] float yPos)
        {
            var tileGrid = new TileGrid(1, 1, 1f, transform, structuralTileSet, new TileData());
            var (x, y) = tileGrid.Get_XY(new Vector3(xPos, yPos));
            Assert.AreEqual(0, x);
            Assert.AreEqual(0, y);
        }

        [Test]
        public void WorldToGridTranslation([Values(1e-6f, 0.5f, 1 - 1e-6f)] float xPos,
            [Values(1e-6f, 0.5f, 1 - 1e-6f)] float yPos)
        {
            transform.Position = new Vector3(1f, 1f);
            var tileGrid = new TileGrid(1, 1, 1f, transform, structuralTileSet, new TileData());
            var (x, y) = tileGrid.Get_XY(new Vector3(xPos, yPos));
            Assert.AreEqual(-1, x);
            Assert.AreEqual(-1, y);
        }

        [Test]
        public void WorldToGridRotation([Values(1e-6f, 0.5f, 1.41421356237f / 2f - 1e-6f)]
            float xPos)
        {
            transform.Rotation = Quaternion.Euler(0, 0, 45);
            var tileGrid = new TileGrid(1, 1, 1f, transform, structuralTileSet, new TileData());
            var (x, y) = tileGrid.Get_XY(new Vector3(xPos, 1.41421356237f / 2f));
            Assert.AreEqual(0, x);
            Assert.AreEqual(0, y);
        }

        [Test]
        public void WorldToGridTransform([Values(1e-6f, 0.5f, 1.41421356237f / 2f - 1e-6f)]
            float xPos)
        {
            transform.Position = new Vector3(0f, 1.41421356237f);
            transform.Rotation = Quaternion.Euler(0, 0, 45);
            var tileGrid = new TileGrid(1, 1, 1f, transform, structuralTileSet, new TileData());
            var (x, y) = tileGrid.Get_XY(new Vector3(xPos, 1.41421356237f / 2f));
            // Left handed coordinate system, so rotation is clockwise
            Assert.AreEqual(-1, x);
            Assert.AreEqual(-1, y);
        }

        [Test]
        public void UpdateUV()
        {
            var tileGrid = new TileGrid(1, 1, 1f, transform, structuralTileSet, new TileData());
            tileGrid.Update_UV(0, 0, uvMappingFull);
            Assert.AreEqual(tileGrid.RenderMesh.uv[12], uvMappingFull[0]);
            Assert.AreEqual(tileGrid.RenderMesh.uv[13], uvMappingFull[1]);
            Assert.AreEqual(tileGrid.RenderMesh.uv[14], uvMappingFull[2]);
            Assert.AreEqual(tileGrid.RenderMesh.uv[15], uvMappingFull[3]);
        }

        [Test]
        public void UpdateTile([Values(-2, -1, 0, 1)] int x, [Values(-2, -1, 0, 1)] int y)
        {
            var tileGrid = new TileGrid(2, 2, 1f, transform, structuralTileSet, new TileData());
            var data = new TileData(typeID: 1);
            tileGrid.UpdateTile(x, y, data);
            Assert.AreEqual(tileGrid.GetValue(x, y), data);
        }

        [Test]
        public void RefreshTile()
        {
            var tileGrid = new TileGrid(1, 1, 1f, transform, structuralTileSet, new TileData());
            tileGrid.Update_UV(0, 0, uvMappingFull);
            tileGrid.RefreshTile(0, 0);
            var emptyUV = uvMappingEmpty;
            Assert.AreEqual(tileGrid.RenderMesh.uv[12], emptyUV[0]);
            Assert.AreEqual(tileGrid.RenderMesh.uv[13], emptyUV[1]);
            Assert.AreEqual(tileGrid.RenderMesh.uv[14], emptyUV[2]);
            Assert.AreEqual(tileGrid.RenderMesh.uv[15], emptyUV[3]);
        }

        [Test]
        public void GetWorldPosition()
        {
            transform.Position = new Vector3(1f, 1f);
            transform.Rotation = Quaternion.Euler(0, 0, 45);

            var tileGrid = new TileGrid(2, 2, 1f, transform, structuralTileSet, new TileData());
            var pos = tileGrid.GetWorldPosition(0, -1);
            var expectedPos = new Vector3(1 + Mathf.Sqrt(2) / 2f, 1 - Mathf.Sqrt(2) / 2f);
            Assert.AreEqual(pos.x, expectedPos.x, 1e-6f);
            Assert.AreEqual(pos.y, expectedPos.y, 1e-6f);
        }

        [Test]
        public void BasicEdges()
        {
            var tileGrid = new TileGrid(2, 2, 1f, transform, structuralTileSet, new TileData(typeID: 1));

            var polygon = tileGrid.Polygons;
            var pointSet = new HashSet<(int x, int y)>(polygon[0]);
            Assert.AreEqual(polygon.Count, 1);
            Assert.That(pointSet, Is.EquivalentTo(new HashSet<(int x, int y)>
            {
                (-2, -2),
                (2, -2),
                (2, 2),
                (-2, 2)
            }));
        }

        [Test]
        public void EdgePointOrder()
        {
            var tileGrid = new TileGrid(2, 2, 1f, transform, structuralTileSet, new TileData(typeID: 1));

            var polygon = tileGrid.Polygons[0];
            var len = polygon.Count;

            for (var i = 0; i < len; i++)
                Assert.IsTrue(polygon[i].x == polygon[(i + 1) % len].x || polygon[i].y == polygon[(i + 1) % len].y);
        }
    }
}