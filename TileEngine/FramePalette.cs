using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine
{
    class FramePalette
    {
        public const int GroupCount = 4;

        private PaletteGroup[] Groups { get; set; }
        
        public FramePalette(PaletteGroup[] groups)
        {
            if (groups.Length != GroupCount)
            {
                throw new ArgumentException($"Unexpected number of palette groups provided: {groups.Length} (expected: {GroupCount})");
            }

            Groups = groups;
        }

        public PaletteGroup GetPaletteGroup(int groupIndex)
        {
            if (groupIndex < 0 || groupIndex >= GroupCount)
            {
                throw new ArgumentException($"Requested palette group is out of bounds: {groupIndex} (expected: 0 <= i < {GroupCount})");
            }

            return Groups[groupIndex];
        }
    }
}
