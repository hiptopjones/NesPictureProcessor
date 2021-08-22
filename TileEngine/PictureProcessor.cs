﻿using NLog;
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

        public const int Width = 256; // Game.LogicalWidth
        public const int Height = 240; // Game.LogicalHeight

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        // System data
        private SystemPalette SystemPalette { get; set; }

        // Cartridge data
        private PatternTable BackgroundTiles { get; set; }
        private PatternTable SpriteTiles { get; set; }

        // Game data
        private FramePalette BackgroundPalette { get; set; }
        private FramePalette SpritePalette { get; set; }
        private NameTable Background { get; set; } // TODO: Support multiple nametables
        private ObjectTable Sprites { get; set; }
        private int ScrollPositionX { get; set; }
        private int ScrollPositionY { get; set; }

        private Color[] FrameBuffer { get; set; } = new Color[Width * Height];

        public PictureProcessor()
        {
            InitializeSystemPalette();
            InitializePatternTables();
            InitializePaletteGroups();
            InitializeBackground();
            InitializeSprites();
        }

        private void InitializeSystemPalette()
        {
            SystemPalette = new SystemPalette(Data.SystemPaletteColors);
        }

        private void InitializePatternTables()
        {
            Tile[] spriteTiles = new Tile[PatternTable.TileCount];
            Tile[] backgroundTiles = new Tile[PatternTable.TileCount];

            for (int i = 0; i < 256; i++)
            {
                byte[] bytes = Data.ChrRom[i];
                byte[] lowPackedBytes = bytes.Take(Tile.PackedPixelByteCount).ToArray();
                byte[] highPackedBytes = bytes.Skip(Tile.PackedPixelByteCount).Take(Tile.PackedPixelByteCount).ToArray();

                spriteTiles[i] = new Tile(lowPackedBytes, highPackedBytes);
            }

            for (int i = 0; i < 256; i++)
            {
                byte[] bytes = Data.ChrRom[i + PatternTable.TileCount];
                byte[] lowPackedBytes = bytes.Take(Tile.PackedPixelByteCount).ToArray();
                byte[] highPackedBytes = bytes.Skip(Tile.PackedPixelByteCount).Take(Tile.PackedPixelByteCount).ToArray();

                backgroundTiles[i] = new Tile(lowPackedBytes, highPackedBytes);
            }

            BackgroundTiles = new PatternTable(backgroundTiles);
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
            byte[] patternTableIndexes = Data.NameTable0Ram.Take(NameTable.TileCount).ToArray();
            byte[] packedAttributes = Data.NameTable0Ram.Skip(NameTable.TileCount).Take(NameTable.BlockCount).ToArray();

            Background = new NameTable(patternTableIndexes, packedAttributes);
        }

        private void InitializeSprites()
        {
            Sprites = new ObjectTable(Data.PrimaryOamRam);
        }

        public Color[] GenerateFrame()
        {
            for (int pixelY = 0; pixelY < Height; pixelY++)
            {
                int[] validAttributeIds = Sprites.GetObjectsOnScanline(pixelY);

                for (int pixelX = 0; pixelX < Width; pixelX++)
                {
                    // NOTE: This will result in overdraw
                    Color backgroundColor = GenerateBackgroundColor(pixelX, pixelY);
                    Color spriteColor = GenerateSpriteColor(pixelX, pixelY, validAttributeIds);

                    Color pixelColor = spriteColor != null ? spriteColor : backgroundColor;
                    FrameBuffer[(pixelY * Width) + pixelX] = pixelColor;
                }
            }

            return FrameBuffer;
        }

        private Color GenerateBackgroundColor(int pixelX, int pixelY)
        {
            // Translate pixel to tile location
            int tileX = pixelX / Tile.PixelCountX;
            int tileY = pixelY / Tile.PixelCountY;

            // Get the tile
            int patternTableIndex = Background.GetPatternTableIndex(tileX, tileY);
            Tile tile = BackgroundTiles.GetTile(patternTableIndex);

            // Get the specific pixel in the tile
            int localX = pixelX % Tile.PixelCountX;
            int localY = pixelY % Tile.PixelCountY;
            int pixelValue = tile.GetPixelValue(localX, localY);

            // Get the palette group to use
            // Pixel value = 0 in background tiles means use the universal background color (group 0, entry 0)
            int paletteGroupIndex = pixelValue == 0 ? 0 : Background.GetPaletteGroupIndex(tileX, tileY);
            PaletteGroup paletteGroup = BackgroundPalette.GetPaletteGroup(paletteGroupIndex);

            // Get the specific color to draw
            int systemPaletteIndex = paletteGroup.GetPaletteIndex(pixelValue);
            Color color = SystemPalette.GetColor(systemPaletteIndex);

            return color;
        }

        private Color GenerateSpriteColor(int pixelX, int pixelY, int[] validAttributeIds)
        {
            // NOTE: Returns the first pixel it finds
            // NOTE: Ignores the priority (draw over/under background) flag

            for (int i = 0; i < validAttributeIds.Length; i++)
            {
                int attributeId = validAttributeIds[i];
                if (attributeId < 0)
                {
                    // Once any attributeId is -1 (empty), all subsequent are empty
                    break;
                }

                int spriteX = Sprites.GetX(attributeId);
                if (spriteX <= pixelX && spriteX > pixelX - Tile.PixelCountX)
                {
                    int spriteY = Sprites.GetY(attributeId);

                    int patternTableIndex = Sprites.GetPatternTableIndex(attributeId);
                    Tile tile = SpriteTiles.GetTile(patternTableIndex);

                    // Get the specific pixel in the tile
                    int localX = pixelX - spriteX;
                    int localY = pixelY - spriteY;

                    if (Sprites.IsFlippedHorizontal(attributeId))
                    {
                        localX = Tile.PixelCountX - localX - 1;
                    }

                    if (Sprites.IsFlippedVertical(attributeId))
                    {
                        localY = Tile.PixelCountY - localY - 1;
                    }

                    int pixelValue = tile.GetPixelValue(localX, localY);

                    if (pixelValue == 0)
                    {
                        // Pixel value = 0 in sprites means a transparent pixel
                        continue;
                    }

                    // Get the palette group to use
                    int paletteGroupIndex = Sprites.GetPaletteGroupIndex(attributeId);
                    PaletteGroup paletteGroup = SpritePalette.GetPaletteGroup(paletteGroupIndex);

                    // Get the specific color to draw
                    int systemPaletteIndex = paletteGroup.GetPaletteIndex(pixelValue);
                    Color color = SystemPalette.GetColor(systemPaletteIndex);

                    return color;
                }
            }

            return null;
        }
    }
}
