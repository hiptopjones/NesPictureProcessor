using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine
{
    class Program
    {

        static void Main(string[] args)
        {
            PictureProcessor processor = new PictureProcessor();

            TimeSpan frameInterval = TimeSpan.FromSeconds(1 / 60.0);
            DateTime startTime = DateTime.Now;
            DateTime nextFrame = startTime + frameInterval;
            while (true)
            {
                // Lock the frame rate
                if (DateTime.Now < nextFrame)
                {
                    continue;
                }

                // This will drift due to precision loss over time
                nextFrame += frameInterval;

                byte[] frameBuffer = processor.GenerateFrame();
                //DrawFrame(frameBuffer);
            }
        }
    }
}
