using LLama.Common;
using LLama;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LLama.Native;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using AForge.Imaging.Filters;
using Emgu.CV.OCR;
using tessnet2;
using System.IO.Compression;

namespace AutomaticSmartRevise2
{
    public enum SelectOption
    {
        Question,
        Answer1,
        Answer2,
        Answer3,
        Answer4,
        NextQuestion
    }

#pragma warning disable CA1416 // Validate platform compatibility
    public class bot
    {
        HttpClient client = new HttpClient();
        Random thiranya = new();
        public static string tessDataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessData");

        const int imageScalar = 10;

        public void Prerequisites()
        {
            if (File.Exists(Path.Combine(tessDataDirectory, "eng.unicharset")) != true)
            {
                Console.WriteLine("TessData not found, downloading for you...");
                WebClient wc = new();
                string zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdataZip.zip");
                //temporary discord hosting
                wc.DownloadFile("https://cdn.discordapp.com/attachments/840827592177221652/1209946389271019560/tessData.zip?ex=65e8c562&is=65d65062&hm=487d97d38fc1464fa89634ba5a018b89bb68e7fac6a4b98d6752c4e61e1676bf&", zipPath);
                ZipFile.ExtractToDirectory(zipPath, AppDomain.CurrentDomain.BaseDirectory, true);
                File.Delete(zipPath);
                Console.WriteLine("TessData downloaded.");
            }
        }
        public void blabblahblah()
        {
            Prerequisites();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Enter how many questions you want to solve: ");
            int count = int.Parse(Console.ReadLine());
            Console.WriteLine("Starting in five seconds, make sure that your main monitor is 1920x1080 and that the smart revise webpage is open at 100% zoom and " +
                "in the quiz section already. Do not touch your mouse until this is finished as the application will take control of your computer for you.");
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            string tempImages = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempImages");
            Directory.CreateDirectory(tempImages);
            for(int i = 0; i<count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                //Measure time taken to solve;
                Stopwatch st = Stopwatch.StartNew();
                string tempPath = Path.Combine(tempImages, thiranya.Next(99999).ToString());
                Directory.CreateDirectory(tempPath);

                Console.WriteLine("Scanning screen..");

                var questiondata = SaveScreenshotsOfQuestionAndAnswers(tempPath);

                string question = GetTextFromImage(questiondata[0]);
                string answer1 = GetTextFromImage(questiondata[1]);
                string answer2 = GetTextFromImage(questiondata[2]);
                string answer3 = GetTextFromImage(questiondata[3]);
                string answer4 = GetTextFromImage(questiondata[4]);

                Console.WriteLine("Solving question: " + question);
                var formatted = FormatRequest(question, answer1, answer2, answer3, answer4);
                var answer = SendLLamaRequest(formatted).Result;
                Console.WriteLine($"Answer: {answer} Time to solve: {st.Elapsed.Seconds} seconds");

                switch (answer)
                {
                    case "A":
                        SelectAnswer(SelectOption.Answer1);
                        Task.Delay(TimeSpan.FromSeconds(Math.Clamp(8 - st.Elapsed.Seconds, 0, 8) + 1.5)).Wait();
                        SelectAnswer(SelectOption.NextQuestion);
                        break;
                    case "B":
                        SelectAnswer(SelectOption.Answer2);
                        Task.Delay(TimeSpan.FromSeconds(Math.Clamp(8 - st.Elapsed.Seconds, 0, 8) + 1.5)).Wait();
                        SelectAnswer(SelectOption.NextQuestion);
                        break;
                    case "C":
                        SelectAnswer(SelectOption.Answer3);
                        Task.Delay(TimeSpan.FromSeconds(Math.Clamp(8 - st.Elapsed.Seconds, 0, 8) + 1.5)).Wait();
                        SelectAnswer(SelectOption.NextQuestion);
                        break;
                    case "D":
                        SelectAnswer(SelectOption.Answer4);
                        Task.Delay(TimeSpan.FromSeconds(Math.Clamp(8 - st.Elapsed.Seconds, 0, 8) + 1.5)).Wait();
                        SelectAnswer(SelectOption.NextQuestion);
                        break;
                    default:
                        Console.WriteLine("Couldn't solve, retrying.");
                        i = i - 1;
                        break;
                }
                //to prevent memory leak
                GC.Collect();
                Directory.Delete(tempPath, true);
            }
            try
            {
                Directory.Delete(tempImages, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't delete temporary images, " + ex.Message + " - please delete yourself.");
            }
        }

        public void SelectAnswer(SelectOption option)
        {
            switch(option)
            {
                case SelectOption.Question:
                    MouseInterface.SetCursorPosition(500, 360);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
                case SelectOption.Answer1:
                    MouseInterface.SetCursorPosition(500, 425);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
                case SelectOption.Answer2:
                    MouseInterface.SetCursorPosition(500, 475);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
                case SelectOption.Answer3:
                    MouseInterface.SetCursorPosition(500, 515);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
                case SelectOption.Answer4:
                    MouseInterface.SetCursorPosition(500, 575);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
                case SelectOption.NextQuestion:
                    MouseInterface.SetCursorPosition(500, 700);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
            }
        }

        public string[] SaveScreenshotsOfQuestionAndAnswers(string dirPath)
        {
            List<string> screenshotPaths= new List<string>();
            //Question:
            //  Y: 345-385 X:80-920 
            //Answer 1:
            //  Y: 410-435 X:80-920 
            //Answer 2:
            //  Y: 460-490 X:80-920 
            //Answer 3:
            //  Y: 515-545 X:80-920 
            //Answer 4:
            //  Y: 565-595 X:80-920 
            //Next Question:
            //  Y: 700 X: 480

            //Take screenshot of entirety of primary display.
            Bitmap screenshot = new Bitmap(1920, 1080);
            Graphics g = Graphics.FromImage(screenshot);
            g.CopyFromScreen(Point.Empty, Point.Empty, screenshot.Size);
            g.Dispose();

            //Define the crop areas
            Rectangle questionCropArea = new Rectangle(40, 345, 920-80, 385-345);
            Rectangle answer1CropArea = new Rectangle(40, 410, 920- 80, 435 - 395);
            Rectangle answer2CropArea = new Rectangle(40, 460, 920- 80, 490 - 445);
            Rectangle answer3CropArea = new Rectangle(40, 515, 920 - 80, 545 - 515);
            Rectangle answer4CropArea = new Rectangle(40, 565, 920 - 80, 595 - 565);

            Bitmap questionImageCrop = screenshot.Clone(questionCropArea, PixelFormat.Format32bppRgb);
            Bitmap answer1ImageCrop = screenshot.Clone(answer1CropArea, PixelFormat.Format32bppRgb);
            Bitmap answer2ImageCrop = screenshot.Clone(answer2CropArea, PixelFormat.Format32bppRgb);
            Bitmap answer3ImageCrop = screenshot.Clone(answer3CropArea, PixelFormat.Format32bppRgb);
            Bitmap answer4ImageCrop = screenshot.Clone(answer4CropArea, PixelFormat.Format32bppRgb);

            //Upscale by 20x to improve emgu enhancement
            Bitmap questionImageCropResized = ResizeImage(questionImageCrop, questionImageCrop.Width * imageScalar, questionImageCrop.Height * imageScalar);
            Bitmap answer1ImageCropResized = ResizeImage(answer1ImageCrop, answer1ImageCrop.Width * imageScalar, answer1ImageCrop.Height * imageScalar);
            Bitmap answer2ImageCropResized = ResizeImage(answer2ImageCrop, answer2ImageCrop.Width * imageScalar, answer2ImageCrop.Height * imageScalar);
            Bitmap answer3ImageCropResized = ResizeImage(answer3ImageCrop, answer3ImageCrop.Width * imageScalar, answer3ImageCrop.Height * imageScalar);
            Bitmap answer4ImageCropResized = ResizeImage(answer4ImageCrop, answer4ImageCrop.Width * imageScalar, answer4ImageCrop.Height * imageScalar);

            string questionImagePath = Path.Combine(dirPath, "question.png");
            string answer1ImagePath = Path.Combine(dirPath, "answer1.png");
            string answer2ImagePath = Path.Combine(dirPath, "answer2.png");
            string answer3ImagePath = Path.Combine(dirPath, "answer3.png");
            string answer4ImagePath = Path.Combine(dirPath, "answer4.png");

            questionImageCropResized.Save(questionImagePath, ImageFormat.Png);
            answer1ImageCropResized.Save(answer1ImagePath, ImageFormat.Png);
            answer2ImageCropResized.Save(answer2ImagePath, ImageFormat.Png);
            answer3ImageCropResized.Save(answer3ImagePath, ImageFormat.Png);
            answer4ImageCropResized.Save(answer4ImagePath, ImageFormat.Png);

            questionImageCrop.Dispose();
            answer2ImageCrop.Dispose();
            answer3ImageCrop.Dispose();
            answer4ImageCrop.Dispose();
            answer1ImageCrop.Dispose();

            //Upscale by 20x to improve emgu enhancement
            questionImageCropResized.Dispose();
            answer1ImageCropResized.Dispose();
            answer2ImageCropResized.Dispose();
            answer3ImageCropResized.Dispose();
            answer4ImageCropResized.Dispose();


            screenshotPaths.Add(EmguEnhancement(questionImagePath));
            screenshotPaths.Add(EmguEnhancement(answer1ImagePath));
            screenshotPaths.Add(EmguEnhancement(answer2ImagePath));
            screenshotPaths.Add(EmguEnhancement(answer3ImagePath));
            screenshotPaths.Add(EmguEnhancement(answer4ImagePath));


            return screenshotPaths.ToArray();
        }

        public static string EmguEnhancement(string path)
        {
            return path;
            //read in grayscale
            Mat image = CvInvoke.Imread(path, ImreadModes.Grayscale);

            //apply Gaussian blur to smooth the image
            CvInvoke.GaussianBlur(image, image, new Size(1, 1), (int)BorderType.Default);

            //apply adaptive thresholding to binarize the image
            CvInvoke.AdaptiveThreshold(image, image, 255, AdaptiveThresholdType.MeanC, ThresholdType.Binary, 11, 2);

            // Invert the image to make text white and background black
            //CvInvoke.BitwiseNot(image, image);

            string outputPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "enhanced.png");
            image.Save(outputPath);
            image.Dispose();
            return outputPath;
        }


        public static Bitmap ConvertToGrayScale(Bitmap input)
        {
            Bitmap greyscale = new Bitmap(input.Width, input.Height);
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    Color pixelColor = input.GetPixel(x, y);
                    if (pixelColor.R < 50 && pixelColor.G < 50 && pixelColor.B < 50)
                    {
                        greyscale.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    }
                    else
                    {
                        greyscale.SetPixel(x, y, Color.FromArgb(1, 255, 255, 255));
                    }
                }
            }
            return greyscale;
        }
        public string GetTextFromImage(string path)
        {
            Bitmap image = new Bitmap(path);
            tessnet2.Tesseract ocr = new tessnet2.Tesseract();
            ocr.SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,$-/#&=()\"':?"); // Accepted characters
            ocr.SetVariable("preserve_interword_spaces", "true");
            ocr.Init(tessDataDirectory, "eng", false); // Directory of your tessdata folder
            List<tessnet2.Word> result = ocr.DoOCR(image, System.Drawing.Rectangle.Empty);
            string Results = "";
            foreach (tessnet2.Word word in result)
            {
                Results += word.Text+" ";
            }
            return Results;
        }

        public string FormatRequest(string question, string answer1, string answer2, string answer3, string answer4)
        {
            string newRequest = $"Answer the following multiple choice question with the corresponding letter. Encapsulate your letter answer in parenthesis. Limit your response to one letter.\n\nQuestion: {question}\n\n" +
                $"A:{answer1}\nB:{answer2}\nC:{answer3}\nD:{answer4}";
            return newRequest;
        }

        public struct LLamaPayload
        {
            public string audio;
            public string image;
            public int maxTokens;
            public string model;
            public string prompt;
            public string systemPrompt;
            public float temperature;
            public float topP;

            public LLamaPayload()
            {
                audio = null;
                image = null;
                maxTokens = 800;
                model = "meta/llama-2-70b-chat";
                prompt = "";
                systemPrompt = "You are a helpful assistant.";
                temperature = 0.75f;
                topP = 0.9f;
            }
        }

        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            //a holder for the result
            Bitmap result = new Bitmap(width, height);
            //set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
                graphics.Dispose();
            }

            //return the resulting bitmap
            return result;
        }

        public async Task<string> SendLLamaRequest(string message)
        {
            //leech off of llama2.ai/api
            HttpRequestMessage message1 = new HttpRequestMessage();
            message1.Method = HttpMethod.Post;
            message1.RequestUri = new Uri("https://www.llama2.ai/api");
            message1.Headers.Add("Origin", "https://www.llama2.ai");
            message1.Headers.Add("Referer", "https://www.llama2.ai/");
            message1.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            message1.Headers.Add("authority", "www.llama2.ai");
            message1.Headers.Add("method", "POST");
            message1.Headers.Add("Accept", "*/*");
            LLamaPayload payload = new();
            payload.prompt = $"<s>[INST] <<SYS>>\r\nYou are a helpful assistant which only responds in single letters.\r\n<</SYS>>\r\n\r\n{message} [/INST]\r\n";
            message1.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8);
            var response = await client.SendAsync(message1);
            var b = response.StatusCode;
            var formattedstring = await response.Content.ReadAsStringAsync();
            try
            {
                formattedstring = formattedstring.Remove(0, formattedstring.IndexOf("("));
                formattedstring = formattedstring.Remove(formattedstring.IndexOf(")"));
                formattedstring = formattedstring.Replace("(", string.Empty).Trim();
                formattedstring = formattedstring[0].ToString();
            }
            catch (Exception ex) { }
            if(formattedstring.Length == 0) {
                Console.WriteLine("Failed to parse answer from solving, retrying.");
                //recursion might result in stack overflow :(
                return await SendLLamaRequest(message);
            }
            else
            {
                return formattedstring.Replace("(", string.Empty).Trim()[0].ToString().ToUpper();
            }
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
