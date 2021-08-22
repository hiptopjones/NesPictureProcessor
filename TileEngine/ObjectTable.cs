using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine
{
    class ObjectTable
    {
        // Representation of the OAM memory in the PPU

        public const int AttributeCount = 64;
        public const int AttributeSize = 4;

        private byte[][] Attributes { get; set; }

        public ObjectTable(byte[][] attributes)
        {
            if (attributes.Length != AttributeCount)
            {
                throw new ArgumentException($"Unexpected number of attributes provided: {attributes.Length} (expected: {AttributeCount})");
            }

            if (attributes[0].Length != AttributeSize)
            {
                throw new ArgumentException($"Unexpected number of bytes per attribute provided: {attributes[0].Length} (expected: {AttributeSize})");
            }

            Attributes = attributes;
        }

        public int[] GetObjectsOnScanline(int y)
        {
            // Max 8 sprites per scanline
            // TODO: Use a global structure to avoid this constant memory allocation
            int[] validAttributeIds = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };
            int currentIndex = 0;

            for (int attributeId = 0; attributeId < Attributes.Length; attributeId++)
            {
                int spriteY = GetY(attributeId);
                if (spriteY <= y && spriteY > y - Tile.PixelCountY)
                {
                    validAttributeIds[currentIndex++] = attributeId;
                }
            }

            return validAttributeIds;
        }

        public int GetX(int attributeIndex)
        {
            return Attributes[attributeIndex][3];
        }

        public int GetY(int attributeIndex)
        {
            return Attributes[attributeIndex][0];
        }

        public int GetPatternTableIndex(int attributeIndex)
        {
            return Attributes[attributeIndex][1];
        }

        public int GetPaletteGroupIndex(int attributeIndex)
        {
            return Attributes[attributeIndex][2] & 0b11;
        }

        public bool IsFlippedHorizontal(int attributeIndex)
        {
            int flippedHorizontalFlag = (Attributes[attributeIndex][2] >> 6) & 0b1;
            return flippedHorizontalFlag == 1;
        }

        public bool IsFlippedVertical(int attributeIndex)
        {
            int flippedVerticalFlag = (Attributes[attributeIndex][2] >> 7) & 0b1;
            return flippedVerticalFlag == 1;
        }

        public bool IsBehindBackground(int attributeIndex)
        {
            int behindBackgroundFlag = (Attributes[attributeIndex][2] >> 5) & 0b1;
            return behindBackgroundFlag == 1;
        }
    }
}
