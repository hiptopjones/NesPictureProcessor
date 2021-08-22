using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using SDL2;

namespace TileEngine
{
    class Game
	{
		private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

		// Actual window dimensions (scales up or down from logical dimensions)
		private const int WindowWidth = 768;
		private const int WindowHeight = 720;

		// NES rendering dimensions
		private const int LogicalWidth = PictureProcessor.ScreenWidth;
		private const int LogicalHeight = PictureProcessor.ScreenHeight;

		// SDL handles
		private IntPtr SdlWindow { get; set; }
		private IntPtr SdlRenderer { get; set; }


		public void Run()
        {
            try
            {
				SdlSetup();

                PictureProcessor processor = new PictureProcessor();

				int scrollPositionX = 0;
				int scrollPositionY = 0;

				bool isRunning = true;
				uint totalFrames = 0;
				uint totalFrameTicks = 0;

				// Pattern from here: https://thenumbat.github.io/cpp-course/sdl2/08/08.html
				while (isRunning)
				{
					// Start frame timing
					totalFrames++;
					uint startTicks = SDL.SDL_GetTicks();
					ulong startPerf = SDL.SDL_GetPerformanceCounter();

					// Clear frame
					SDL.SDL_SetRenderDrawColor(SdlRenderer, 0xFF, 0xFF, 0xFF, 0xFF);
					SDL.SDL_RenderClear(SdlRenderer);

					// Event loop
					while (SDL.SDL_PollEvent(out SDL.SDL_Event sdlEvent) != 0)
					{
						if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
						{
							isRunning = false;
						}
					}

					processor.SetScrollPositionX(scrollPositionX++);
					processor.SetScrollPositionY(scrollPositionY++);

					Color[] frameBuffer = processor.GenerateFrame();

					// Render loop
					for (int y = 0; y < LogicalHeight; y++)
					{
						for (int x = 0; x < LogicalWidth; x++)
	                    {
							Color color = frameBuffer[(y * LogicalWidth) + x];

							SDL.SDL_SetRenderDrawColor(SdlRenderer, color.R, color.G, color.B, 0xFF);
							SDL.SDL_RenderDrawPoint(SdlRenderer, x, y);
						}
					}

					// End frame timing
					uint endTicks = SDL.SDL_GetTicks();
					ulong endPerf = SDL.SDL_GetPerformanceCounter();
					ulong framePerf = endPerf - startPerf;
					double frameTime = (endTicks - startTicks) / 1000.0f;
					totalFrameTicks += endTicks - startTicks;

					if (totalFrames % 100 == 0)
					{
						// Strings to display
						string fps = $"Current FPS: {(1.0f / frameTime)}";
						string avg = $"Average FPS: {(1000.0f / ((float)totalFrameTicks / totalFrames))}";
						string perf = $"Current Perf: {framePerf}";

						Logger.Info($"{fps} {avg} {perf}");
					}

					// Display everything
					SDL.SDL_RenderPresent(SdlRenderer);
				}
			}
            finally
            {
				SdlTeardown();
            }
		}

		private void SdlSetup()
		{
			if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
				throw new Exception($"Unable to init video: {SDL.SDL_GetError()}");
			}

			SdlWindow = SDL.SDL_CreateWindow("TileEngine", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, WindowWidth, WindowHeight, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
			if (SdlWindow == IntPtr.Zero)
			{
				throw new Exception($"Unable to create window: {SDL.SDL_GetError()}");
			}

			SdlRenderer = SDL.SDL_CreateRenderer(SdlWindow, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
			if (SdlRenderer == IntPtr.Zero)
			{
				throw new Exception($"Unable to create renderer: {SDL.SDL_GetError()}");
			}

			SDL.SDL_RenderSetLogicalSize(SdlRenderer, LogicalWidth, LogicalHeight);
		}

		private void SdlTeardown()
		{
			if (SdlRenderer != IntPtr.Zero)
			{
				SDL.SDL_DestroyRenderer(SdlRenderer);
				SdlRenderer = IntPtr.Zero;
			}

			if (SdlWindow != IntPtr.Zero)
            {
				SDL.SDL_DestroyWindow(SdlWindow);
				SdlWindow = IntPtr.Zero;
			}

			SDL.SDL_Quit();
		}
	}
}
