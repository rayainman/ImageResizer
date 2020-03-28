using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;
            ImageProcess imageProcess = new ImageProcess(sourcePath,destinationPath);

            Stopwatch oldStyle = new Stopwatch();
            Stopwatch newStyle = new Stopwatch();
            var oldStyletime = new long();
            var newStyletime = new long();

            for (int i =0; i < 1; i++)
            {
                Console.WriteLine($"同步開始---");

                imageProcess.Clean();
                await imageProcess.SetFiles();

                oldStyle.Start();
                imageProcess.ResizeImages(2.0);
                oldStyle.Stop();
                oldStyletime += oldStyle.ElapsedMilliseconds;
                Console.WriteLine($"同步結束");

                Console.WriteLine($"非同步開始");

                imageProcess.Clean();
                await imageProcess.SetFiles();

                newStyle.Start(); 
                await imageProcess.ResizeImagesAsync(2.0);           
                newStyle.Stop();

                Console.WriteLine($"非同步結束");
                newStyletime += newStyle.ElapsedMilliseconds;
            }

            Console.WriteLine($"非同步花費時間: {newStyle.ElapsedMilliseconds} ms");
            Console.WriteLine($"同步花費時間: {oldStyle.ElapsedMilliseconds} ms");
            Console.WriteLine(string.Format("效能提升比例：{0:0.000}", ((double)oldStyletime - newStyletime) / oldStyletime));


            Console.ReadKey();
        }
    }
}
