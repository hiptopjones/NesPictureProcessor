using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine
{
    class PictureProcessor
    {
        // Screen - 256 px * 240 px
        // Pattern table - All possible tiles for background and sprites (2 banks * 256 tiles * 8 * 8 px * 2 bits / px = 8192 bytes)
        // Nametable - Describes the frame's background (32 * 30 tiles * 1 byte / tile = 960 bytes)
        // Background attribute table = Describes the palette entries used for groups of 4 neighboring tiles (960 tiles / 4 tiles / meta-tile * 2 bits / meta-tile = 60 bytes)
        // Sprite attribute table - Describes the frame's active sprites (64 tiles * 4 bytes / tile = 256 bytes)
        // System palette - All possible colors, ever (64 colors)
        // Frame palette - Colors available to this frame (8 groups * 4 colors, 4 groups for background and 4 groups for sprites)

        // System data
        private SystemPalette SystemPalette { get; set; }

        // Cartridge data
        private PatternTable BackgroundTiles { get; set; }
        private PatternTable SpriteTiles { get; set; }

        // Game data
        private FramePalette BackgroundPalette { get; set; }
        private FramePalette SpritePalette { get; set; }
        private NameTable[] Backgrounds { get; set; }
        private ObjectAttributes Sprites { get; set; }
        private int ScrollPositionX { get; set; }
        private int ScrollPositionY { get; set; }

        public PictureProcessor()
        {
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            InitializeSystemPalette();
            InitializePatternTables();
            InitializePaletteGroups();
            InitializeBackground();
            InitializeSprites();
        }

        private void InitializeSystemPalette()
        {
            // Colors from here: https://wiki.nesdev.com/w/index.php/PPU_palettes
            Color[] systemPaletteColors = new Color[SystemPalette.ColorCount]
            {
                // Row 1
                /* 0x00 */ new Color(84, 84, 84),
                /* 0x01 */ new Color(0, 30, 116),
                /* 0x02 */ new Color(8, 16, 144),
                /* 0x03 */ new Color(48, 0, 136),
                /* 0x04 */ new Color(68, 0, 100),
                /* 0x05 */ new Color(92, 0, 48),
                /* 0x06 */ new Color(84, 4, 0),
                /* 0x07 */ new Color(60, 24, 0),
                /* 0x08 */ new Color(32, 42, 0),
                /* 0x09 */ new Color(8, 58, 0),
                /* 0x0a */ new Color(0, 64, 0),
                /* 0x0b */ new Color(0, 60, 0),
                /* 0x0c */ new Color(0, 50, 60),
                /* 0x0d */ new Color(0, 0, 0),
                /* 0x0e */ new Color(0, 0, 0),
                /* 0x0f */ new Color(0, 0, 0),

                // Row 2
                /* 0x10 */ new Color(152, 150, 152),
                /* 0x11 */ new Color(8, 76, 196),
                /* 0x12 */ new Color(48, 50, 236),
                /* 0x13 */ new Color(92, 30, 228),
                /* 0x14 */ new Color(136, 20, 176),
                /* 0x15 */ new Color(160, 20, 100),
                /* 0x16 */ new Color(152, 34, 32),
                /* 0x17 */ new Color(120, 60, 0),
                /* 0x18 */ new Color(84, 90, 0),
                /* 0x19 */ new Color(40, 114, 0),
                /* 0x1a */ new Color(8, 124, 0),
                /* 0x1b */ new Color(0, 118, 40),
                /* 0x1c */ new Color(0, 102, 120),
                /* 0x1d */ new Color(0, 0, 0),
                /* 0x1e */ new Color(0, 0, 0),
                /* 0x1f */ new Color(0, 0, 0),

                // Row 3
                /* 0x20 */ new Color(236, 238, 236),
                /* 0x21 */ new Color(76, 154, 236),
                /* 0x22 */ new Color(120, 124, 236),
                /* 0x23 */ new Color(176, 98, 236),
                /* 0x24 */ new Color(228, 84, 236),
                /* 0x25 */ new Color(236, 88, 180),
                /* 0x26 */ new Color(236, 106, 100),
                /* 0x27 */ new Color(212, 136, 32),
                /* 0x28 */ new Color(160, 170, 0),
                /* 0x29 */ new Color(116, 196, 0),
                /* 0x2a */ new Color(76, 208, 32),
                /* 0x2b */ new Color(56, 204, 108),
                /* 0x2c */ new Color(56, 180, 204),
                /* 0x2d */ new Color(60, 60, 60),
                /* 0x2e */ new Color(0, 0, 0),
                /* 0x2f */ new Color(0, 0, 0),

                // Row 4
                /* 0x30 */ new Color(236, 238, 236),
                /* 0x31 */ new Color(168, 204, 236),
                /* 0x32 */ new Color(188, 188, 236),
                /* 0x33 */ new Color(212, 178, 236),
                /* 0x34 */ new Color(236, 174, 236),
                /* 0x35 */ new Color(236, 174, 212),
                /* 0x36 */ new Color(236, 180, 176),
                /* 0x37 */ new Color(228, 196, 144),
                /* 0x38 */ new Color(204, 210, 120),
                /* 0x39 */ new Color(180, 222, 120),
                /* 0x3a */ new Color(168, 226, 144),
                /* 0x3b */ new Color(152, 226, 180),
                /* 0x3c */ new Color(160, 214, 228),
                /* 0x3d */ new Color(160, 162, 160),
                /* 0x3e */ new Color(0, 0, 0),
                /* 0x3f */ new Color(0, 0, 0)
            };

            SystemPalette = new SystemPalette(systemPaletteColors);
        }

        private void InitializePatternTables()
        {
            Tile[] backgroundTiles = new Tile[PatternTable.TileCount];
            BackgroundTiles = new PatternTable(backgroundTiles);

            Tile[] spriteTiles = new Tile[PatternTable.TileCount];
            SpriteTiles = new PatternTable(spriteTiles);
        }

        private void InitializePaletteGroups()
        {
            // Using Donkey Kong sample frame palette here:
            // https://austinmorlan.com/posts/nes_rendering_overview/

            PaletteGroup[] backgroundPaletteGroups = new PaletteGroup[FramePalette.GroupCount]
            {
                new PaletteGroup(new int[PaletteGroup.IndexCount] { 0x0F, 0x15, 0x2c, 0x12 }),
                new PaletteGroup(new int[PaletteGroup.IndexCount] { 0x0F, 0x27, 0x02, 0x17 }),
                new PaletteGroup(new int[PaletteGroup.IndexCount] { 0x0F, 0x30, 0x36, 0x06 }),
                new PaletteGroup(new int[PaletteGroup.IndexCount] { 0x0F, 0x30, 0x2c, 0x24 })
            };
            BackgroundPalette = new FramePalette(backgroundPaletteGroups);

            PaletteGroup[] spritePaletteGroups = new PaletteGroup[FramePalette.GroupCount]
            {
                new PaletteGroup(new int[PaletteGroup.IndexCount] { 0x0F, 0x02, 0x36, 0x16 }),
                new PaletteGroup(new int[PaletteGroup.IndexCount] { 0x0F, 0x30, 0x27, 0x24 }),
                new PaletteGroup(new int[PaletteGroup.IndexCount] { 0x0F, 0x16, 0x30, 0x37 }),
                new PaletteGroup(new int[PaletteGroup.IndexCount] { 0x0F, 0x06, 0x27, 0x02 })
            };
            SpritePalette = new FramePalette(spritePaletteGroups);
        }

        private void InitializeBackground()
        {
            throw new NotImplementedException();
        }

        private void InitializeSprites()
        {
            throw new NotImplementedException();
        }

        public byte[] GenerateFrame()
        {
            throw new NotImplementedException();
        }
    }
}
