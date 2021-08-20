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

		private IntPtr SdlWindow { get; set; }
		private IntPtr SdlRenderer { get; set; }

		private const int WindowWidth = 256;
		private const int WindowHeight = 240;
		private const int FramesPerSecond = 60;

		public void Run()
        {
            try
            {
				SdlSetup();

				//PictureProcessor processor = new PictureProcessor();

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

					// Render loop
					for (int x = 0; x < WindowWidth; x++)
                    {
                        for (int y = 0; y < WindowHeight; y++)
                        {
							SDL.SDL_SetRenderDrawColor(SdlRenderer, (byte)x, (byte)y, (byte)totalFrames, 0xFF);
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
			SdlWindow = SDL.SDL_CreateWindow("TileEngine", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, WindowWidth, WindowHeight, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
			if (SdlWindow == IntPtr.Zero)
			{
				throw new Exception($"Couldn't create window: {SDL.SDL_GetError()}");
			}
			else
			{
				SdlRenderer = SDL.SDL_CreateRenderer(SdlWindow, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
				if (SdlRenderer == IntPtr.Zero)
				{
					throw new Exception($"Couldn't create renderer: {SDL.SDL_GetError()}");
				}
			}
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
