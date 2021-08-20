using Microsoft.VisualStudio.TestTools.UnitTesting;
using TileEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine.Tests
{
    [TestClass()]
    public class TileTests
    {
        [TestMethod()]
        public void UnderlengthPackedByteConstructorTest()
        {
            int correctPackedByteCount = Tile.PixelCount / 8;
            byte[] packedLow;
            byte[] packedHigh;

            packedLow = new byte[correctPackedByteCount - 1];
            packedHigh = new byte[correctPackedByteCount];
            Assert.ThrowsException<ArgumentException>(() => new Tile(packedLow, packedHigh));

            packedLow = new byte[correctPackedByteCount];
            packedHigh = new byte[correctPackedByteCount - 1];
            Assert.ThrowsException<ArgumentException>(() => new Tile(packedLow, packedHigh));
        }

        [TestMethod()]
        public void OverlengthPackedByteConstructorTest()
        {
            int correctPackedByteCount = Tile.PixelCount / 8;
            byte[] packedLow;
            byte[] packedHigh;
                
            packedLow = new byte[correctPackedByteCount + 1];
            packedHigh = new byte[correctPackedByteCount];
            Assert.ThrowsException<ArgumentException>(() => new Tile(packedLow, packedHigh));

            packedLow = new byte[correctPackedByteCount];
            packedHigh = new byte[correctPackedByteCount + 1];
            Assert.ThrowsException<ArgumentException>(() => new Tile(packedLow, packedHigh));
        }

        [TestMethod()]
        public void GetPixelTest()
        {
            byte[] packedLow = new byte[]
            {
                0b10000000,
                0b00000000,
                0b00000000,
                0b00000000,
                0b00000010,
                0b00000000,
                0b00000000,
                0b00000000,
            };
            byte[] packedHigh = new byte[]
            {
                0b00000000,
                0b00000000,
                0b00000000,
                0b01000000,
                0b00000000,
                0b00000000,
                0b00000000,
                0b00000001,
            };
            Tile tile = new Tile(packedLow, packedHigh);
            int pixelValue;

            pixelValue = tile.GetPixelValue(0, 0);
            Assert.AreEqual(0b01, pixelValue);

            pixelValue = tile.GetPixelValue(7, 7);
            Assert.AreEqual(0b10, pixelValue);

            pixelValue = tile.GetPixelValue(1, 3);
            Assert.AreEqual(0b10, pixelValue);

            pixelValue = tile.GetPixelValue(6, 4);
            Assert.AreEqual(0b01, pixelValue);

            Assert.ThrowsException<ArgumentException>(() => tile.GetPixelValue(-1, 0));
            Assert.ThrowsException<ArgumentException>(() => tile.GetPixelValue(0, -1));
            Assert.ThrowsException<ArgumentException>(() => tile.GetPixelValue(Tile.PixelCountX, 0));
            Assert.ThrowsException<ArgumentException>(() => tile.GetPixelValue(0, Tile.PixelCountY));
        }
    }
}