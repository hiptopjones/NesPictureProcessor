using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine
{
    class PatternTable
    {
        public const int TileCount = 256;

        private Tile[] Tiles { get; set; }

        public PatternTable(Tile[] tiles)
        {
            if (tiles.Length != TileCount)
            {
                throw new ArgumentException($"Unexpected number of tiles provided: {tiles.Length} (expected: {TileCount})");
            }

            Tiles = tiles;
        }

        public Tile GetTile(int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= TileCount)
            {
                throw new ArgumentException($"Requested tile index is out of bounds: {tileIndex} (expected: 0 <= i < {TileCount})");
            }

            return Tiles[tileIndex];
        }
    }
}
