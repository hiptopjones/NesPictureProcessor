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
    public class NameTableTests
    {
        [TestMethod()]
        public void GetTileIndexTest()
        {
            int[] tileIndexes = new int[NameTable.TileCount];
            byte[] packedAttributes = new byte[NameTable.PackedAttributeByteCount];

            NameTable nameTable = new NameTable(tileIndexes, packedAttributes);

            int tileIndex;

            tileIndex = nameTable.GetTileIndex(0, 0);
            Assert.AreEqual(0, tileIndex);

            tileIndex = nameTable.GetTileIndex(NameTable.TileCountX - 1, NameTable.TileCountY - 1);
            Assert.AreEqual(NameTable.TileCount - 1, tileIndex);

            tileIndex = nameTable.GetTileIndex(0, 1);
            Assert.AreEqual(NameTable.TileCountX, tileIndex);

            tileIndex = nameTable.GetTileIndex(NameTable.TileCountX - 1, 0);
            Assert.AreEqual(NameTable.TileCountX - 1, tileIndex);
        }

        [TestMethod()]
        public void GetBlockIndexTest()
        {
            int[] tileIndexes = new int[NameTable.TileCount];
            byte[] packedAttributes = new byte[NameTable.PackedAttributeByteCount];

            NameTable nameTable = new NameTable(tileIndexes, packedAttributes);

            int blockIndex;

            // The first few are in the first block
            // (ensure all combinations coordinates resolve correctly)
            blockIndex = nameTable.GetBlockIndex(0, 0);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(1, 0);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(2, 0);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(3, 0);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(0, 1);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(1, 1);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(2, 1);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(3, 1);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(0, 2);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(1, 2);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(2, 2);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(3, 2);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(0, 3);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(1, 3);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(2, 3);
            Assert.AreEqual(0, blockIndex);

            blockIndex = nameTable.GetBlockIndex(3, 3);
            Assert.AreEqual(0, blockIndex);

            // Try the next block to the right
            blockIndex = nameTable.GetBlockIndex(4, 0);
            Assert.AreEqual(1, blockIndex);

            // Try the next block down
            blockIndex = nameTable.GetBlockIndex(0, 4);
            Assert.AreEqual(NameTable.BlockCountX, blockIndex);
        }

        [TestMethod()]
        public void GetMetaTileIndexTest()
        {
            int[] tileIndexes = new int[NameTable.TileCount];
            byte[] packedAttributes = new byte[NameTable.PackedAttributeByteCount];

            NameTable nameTable = new NameTable(tileIndexes, packedAttributes);

            int metaTileIndex;

            // Meta-tile #0
            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(0, 0);
            Assert.AreEqual(0, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(1, 0);
            Assert.AreEqual(0, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(0, 1);
            Assert.AreEqual(0, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(1, 1);
            Assert.AreEqual(0, metaTileIndex);

            // Meta-tile #1
            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(2, 0);
            Assert.AreEqual(1, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(3, 0);
            Assert.AreEqual(1, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(2, 1);
            Assert.AreEqual(1, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(3, 1);
            Assert.AreEqual(1, metaTileIndex);

            // Meta-tile #2
            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(0, 2);
            Assert.AreEqual(2, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(1, 2);
            Assert.AreEqual(2, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(0, 3);
            Assert.AreEqual(2, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(1, 3);
            Assert.AreEqual(2, metaTileIndex);

            // Meta-tile #3
            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(2, 2);
            Assert.AreEqual(3, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(3, 2);
            Assert.AreEqual(3, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(2, 3);
            Assert.AreEqual(3, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(3, 3);
            Assert.AreEqual(3, metaTileIndex);

            // Sample a block to the right and down
            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(4, 4);
            Assert.AreEqual(0, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(6, 4);
            Assert.AreEqual(1, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(4, 6);
            Assert.AreEqual(2, metaTileIndex);

            metaTileIndex = nameTable.GetBlockLocalMetaTileIndex(6, 6);
            Assert.AreEqual(3, metaTileIndex);
        }
    }
}