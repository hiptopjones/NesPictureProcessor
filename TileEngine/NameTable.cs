using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine
{
    public class NameTable
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public const int TileCountX = 32;
        public const int TileCountY = 30;
        public const int TileCount = TileCountX * TileCountY;
        public const int MetaTileSize = 2; // 2 tiles per side
        public const int MetaTileCountX = TileCountX / 2 + TileCountX % 2;
        public const int MetaTileCountY = TileCountY / 2 + TileCountY % 2;
        public const int MetaTileCount = MetaTileCountX * MetaTileCountY; // A meta-tile is a set of 4 tiles
        public const int BlockSize = 4; // 4 tiles per side
        public const int BlockCountX = MetaTileCountX / 2 + MetaTileCountX % 2;
        public const int BlockCountY = MetaTileCountY / 2 + MetaTileCountY % 2;
        public const int BlockCount = BlockCountX * BlockCountY; // A block is a set of 4 meta-tiles
        public const int PackedAttributeByteCount = BlockCount; // 2 bits per meta-tile means 1 byte per block

        private byte[] PatternTableIndexes { get; set; }
        private byte[] PackedAttributes { get; set; }

        public NameTable(byte[] patternTableIndexes, byte[] packedAttributes)
        {
            if (patternTableIndexes.Length != TileCount)
            {
                throw new ArgumentException($"Unexpected number of pattern table indexes provided: {patternTableIndexes.Length} (expected: {TileCount})");
            }

            if (packedAttributes.Length != PackedAttributeByteCount)
            {
                throw new ArgumentException($"Unexpected number of packed attribute bytes provided: {packedAttributes.Length} (expected: {PackedAttributeByteCount})");
            }

            PatternTableIndexes = patternTableIndexes;
            PackedAttributes = packedAttributes;
        }

        // Get the flattened index to the global pattern table for the tile coordinate
        public int GetPatternTableIndex(int tileX, int tileY)
        {
            int tileIndex = GetTileIndex(tileX, tileY);
            return PatternTableIndexes[tileIndex];
        }

        // Get the flattened index of the tile this tile coordinate references
        // Can be used to look up the index to the global pattern table
        public int GetTileIndex(int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= TileCountX)
            {
                throw new ArgumentException($"Requested X coordinate is out of bounds: {tileX} (expected: 0 <= x < {TileCountX})");
            }

            if (tileY < 0 || tileY >= TileCountY)
            {
                throw new ArgumentException($"Requested Y coordinate is out of bounds: {tileY} (expected: 0 <= y < {TileCountY})");
            }

            return (tileY * TileCountX) + tileX;
        }

        // Get the flattened index of the block the tile at this tile coorindate belongs to
        // Can be used to look up the palette group in the local attribute table
        public int GetBlockIndex(int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= TileCountX)
            {
                throw new ArgumentException($"Requested X coordinate is out of bounds: {tileX} (expected: 0 <= x < {TileCountX})");
            }

            if (tileY < 0 || tileY >= TileCountY)
            {
                throw new ArgumentException($"Requested Y coordinate is out of bounds: {tileY} (expected: 0 <= y < {TileCountY})");
            }

            return ((tileY / BlockSize) * BlockCountX) + (tileX / BlockSize);
        }

        // Get the index of the meta-tile within the block the tile coordinate belongs to
        // Can be used to look up the specific palette in the block's palette group
        public int GetBlockLocalMetaTileIndex(int tileX, int tileY)
        {
            int globalMetaTileX = tileX / MetaTileSize;
            int globalMetaTileY = tileY / MetaTileSize;

            int localMetaTileX = globalMetaTileX % 2;
            int localMetaTileY = globalMetaTileY % 2;

            return (localMetaTileY * (BlockSize / MetaTileSize)) + localMetaTileX;
        }

        // Get the index of the palette group to apply to the tile at the tile coordinate
        // Can be used to look up the specific palette group in the frame palette
        public int GetPaletteGroupIndex(int tileX, int tileY)
        {
            int blockIndex = GetBlockIndex(tileX, tileY);
            byte blockAttributes = PackedAttributes[blockIndex];

            int metaTileIndex = GetBlockLocalMetaTileIndex(tileX, tileY);

            // Broken out to facilitate debugging intermediate values
            int shiftSize = metaTileIndex * 2;
            int shiftedBlockAttributes = blockAttributes >> shiftSize;
            int paletteGroupIndex = shiftedBlockAttributes & 0b11;

            return paletteGroupIndex;
        }
    }
}
