using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
        readonly string destPath;
        readonly string sourcePath;
        List<string> allFiles;
        
        public ImageProcess(string sourcePath,string destPath)
        {
            this.sourcePath = sourcePath;
            this.destPath = destPath;            
        }
        public async Task SetFiles()
        {
            allFiles =  FindImages();
        }
        private List<string> FindImages()
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(sourcePath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(sourcePath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(sourcePath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }
        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean()
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImages(double scale)
        {        
            foreach (var filePath in allFiles)
            {
                //Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);
                Bitmap processedImage = ProcessBitmap(filePath, scale);
                string destFile = Path.Combine(destPath, imgName + ".jpg");
                processedImage.Save(destFile, ImageFormat.Jpeg);
            }
        }
        public async Task ResizeImagesAsync(double scale)
        {
            List<Task> tasks = new List<Task>();

            foreach (var filePath in allFiles)
            {
                tasks.Add(Task.Run(async () =>
                {
                    string imgName = Path.GetFileNameWithoutExtension(filePath);
                    string destFile = Path.Combine(destPath, imgName + ".jpg");

                    var processedImg = ProcessBitmapAsync(filePath, scale);

                    await processedImg.ContinueWith(t =>
                    {
                        t.Result.Save(destFile, ImageFormat.Jpeg);
                    });
                }));
            }
            await Task.WhenAll(tasks);   
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        private async Task<List<string>> FindImagesAsync()
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(sourcePath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(sourcePath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(sourcePath, "*.jpeg", SearchOption.AllDirectories));
            return await Task.FromResult(files);
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        private Bitmap ProcessBitmap(string filePath, double scale)
        {
            Image img = Image.FromFile(filePath);
            int destionatonWidth = (int)(img.Width * scale);
            int destionatonHeight = (int)(img.Height * scale);
            
            Bitmap resizedbitmap = new Bitmap(destionatonWidth, destionatonHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, destionatonWidth, destionatonHeight),
                new Rectangle(0, 0, img.Width, img.Height),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }

        private async Task<Bitmap> ProcessBitmapAsync(string filePath, double scale)
        {
            return await Task.Run(async () =>
            {
                Image img = Image.FromFile(filePath);
                int destionatonWidth = (int)(img.Width * scale);
                int destionatonHeight = (int)(img.Height * scale);
                Bitmap resizedbitmap = new Bitmap(destionatonWidth, destionatonHeight);

            
                Graphics g = Graphics.FromImage(resizedbitmap);
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                await Task.Run(async () => { 
                    g.Clear(Color.Transparent);
                    g.DrawImage(img,
                    new Rectangle(0, 0, destionatonWidth, destionatonHeight),
                    new Rectangle(0, 0, img.Width, img.Height),
                    GraphicsUnit.Pixel);
                });
            return resizedbitmap;
            });     
        }

        private void Log(string functionName)
        {
            Console.WriteLine(functionName+ $"{ DateTime.Now.ToString("hh:MM:ss-fff")}]");
            //Console.WriteLine(functionName +　$"Thread:{Thread.CurrentThread.ManagedThreadId}]");
        }

    }
}


//   public async Task ResizeImagesAsync(double scale) 另外的寫法
/*
foreach (var filePath in allFiles)
{

    tasks.Add(Task.Run(async () =>
    {
        Console.WriteLine("1");
        string imgName = Path.GetFileNameWithoutExtension(filePath);
        string destFile = Path.Combine(destPath, imgName + ".jpg");


        Console.WriteLine("2");
        var processedImg = await ProcessBitmapAsync(filePath, scale);
        Console.WriteLine("3");
        processedImg.Save(destFile, ImageFormat.Jpeg);

    }));     
}
Task.WhenAll(tasks).Wait();
*/
