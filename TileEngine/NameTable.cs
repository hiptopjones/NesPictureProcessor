using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine
{
    public class NameTable
    {
        // Definition of one of the backgrounds

        public const int TileCountX = 32;
        public const int TileCountY = 30;
        public const int TileCount = TileCountX * TileCountY;
        public const int MetaTileSize = 2; // 2 tiles per side
        public const int MetaTileCountX = TileCountX / 2;
        public const int MetaTileCountY = TileCountY / 2;
        public const int MetaTileCount = MetaTileCountX * MetaTileCountY; // A meta-tile is a set of 4 tiles
        public const int BlockSize = 4; // 4 tiles per side
        public const int BlockCountX = MetaTileCountX / 2;
        public const int BlockCountY = MetaTileCountY / 2;
        public const int BlockCount = BlockCountX * BlockCountY; // A block is a set of 4 meta-tiles
        public const int PackedAttributeByteCount = BlockCount; // 2 bits per meta-tile means 1 byte per block

        private int[] PatternTableIndexes { get; set; }
        private byte[] PackedAttributes { get; set; }

        public NameTable(int[] patternTableIndexes, byte[] packedAttributes)
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
        public int GetPatternTableIndex(int x, int y)
        {
            int tileIndex = GetTileIndex(x, y);
            return PatternTableIndexes[tileIndex];
        }

        // Get the flattened index of the tile this tile coordinate references
        // Can be used to look up the index to the global pattern table
        public int GetTileIndex(int x, int y)
        {
            if (x < 0 || x >= TileCountX)
            {
                throw new ArgumentException($"Requested X coordinate is out of bounds: {x} (expected: 0 <= x < {TileCountX})");
            }

            if (y < 0 || y >= TileCountY)
            {
                throw new ArgumentException($"Requested Y coordinate is out of bounds: {y} (expected: 0 <= x < {TileCountY})");
            }

            return (y * TileCountX) + x;
        }

        // Get the flattened index of the block the tile at this tile coorindate belongs to
        // Can be used to look up the palette group in the local attribute table
        public int GetBlockIndex(int x, int y)
        {
            if (x < 0 || x >= TileCountX)
            {
                throw new ArgumentException($"Requested X coordinate is out of bounds: {x} (expected: 0 <= x < {TileCountX})");
            }

            if (y < 0 || y >= TileCountY)
            {
                throw new ArgumentException($"Requested Y coordinate is out of bounds: {y} (expected: 0 <= x < {TileCountY})");
            }

            return ((y / BlockSize) * BlockCountX) + (x / BlockSize);
        }

        // Get the index of the meta-tile within the block the tile coordinate belongs to
        // Can be used to look up the specific palette in the block's palette group
        public int GetBlockLocalMetaTileIndex(int x, int y)
        {
            int globalMetaTileX = x / MetaTileSize;
            int globalMetaTileY = y / MetaTileSize;

            int localMetaTileX = globalMetaTileX % 2;
            int localMetaTileY = globalMetaTileY % 2;

            return (localMetaTileY * (BlockSize / MetaTileSize)) + localMetaTileX;
        }

        // Get the index of the palette group to apply to the tile at the tile coordinate
        // Can be used to look up the specific palette group in the frame palette
        public int GetPaletteGroupIndex(int x, int y)
        {
            int blockIndex = GetBlockIndex(x, y);
            byte blockAttributes = PackedAttributes[blockIndex];

            int metaTileIndex = GetBlockLocalMetaTileIndex(x, y);
            
            // Broken out to facilitate debugging intermediate values
            int shiftSize = metaTileIndex / 2;
            int shiftedBlockAttributes = blockAttributes >> shiftSize;
            int paletteGroupIndex = shiftedBlockAttributes & 0b11;

            return paletteGroupIndex;
        }
    }
}
