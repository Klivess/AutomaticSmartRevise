using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using Emgu.CV;
using Emgu.CV.CvEnum;
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
        (int Width, int Height) screenWidth;
        public int widthScalar;
        public int heightScalar;

        int total = 0;
        int correct = 0;
        int incorrect;

        HttpClient client = new HttpClient();
        Random thiranya = new();
        public static string tessDataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessData");
        public static string tempImages = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempImages");

        string previousQuestion = "";

        const int imageScalar = 10;

        public void Prerequisites()
        {
            Console.WriteLine($"Checking prerequisites.");
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
            SetResolutionScalars();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Enter how many questions you want to solve: ");
            int count = int.Parse(Console.ReadLine());
            Console.WriteLine("Starting in five seconds, " +
                "make sure that your main monitor is 1920x1080 " +
                "and that the smart revise webpage is open at 100% zoom and " +
                "in the quiz section already. Do not touch your mouse until " +
                "this is finished as the application will take " +
                "control of your computer for you.");
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
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
                if(question == previousQuestion)
                {
                    Directory.Delete(tempPath, true);
                    Task.Delay(1000).Wait();
                    questiondata = SaveScreenshotsOfQuestionAndAnswers(tempPath);
                    question = GetTextFromImage(questiondata[0]);
                }
                string answer1 = GetTextFromImage(questiondata[1]);
                string answer2 = GetTextFromImage(questiondata[2]);
                string answer3 = GetTextFromImage(questiondata[3]);
                string answer4 = GetTextFromImage(questiondata[4]);

                Console.WriteLine($"Solving question: {question}\nAnswer 1: {answer1}\nAnswer 2: {answer2}\nAnswer 3: {answer3}\nAnswer 4: {answer4}");
                var formatted = FormatRequest(question, answer1, answer2, answer3, answer4);
                var answer = SendLLamaRequest(formatted).Result;
                Console.WriteLine($"Answer: {answer} Time to solve: {st.Elapsed.Seconds} seconds");
                SelectOption option = new SelectOption();
                switch (answer)
                {
                    case "A":
                        option = SelectOption.Answer1;
                        SelectAnswer(SelectOption.Answer1);
                        break;
                    case "B":
                        option = SelectOption.Answer2;
                        SelectAnswer(SelectOption.Answer2);
                        break;
                    case "C":
                        option = SelectOption.Answer3;
                        SelectAnswer(SelectOption.Answer3);
                        break;
                    case "D":
                        option = SelectOption.Answer4;
                        SelectAnswer(SelectOption.Answer4);
                        break;
                    default:
                        Console.WriteLine("Couldn't solve, retrying.");
                        i = i - 1;
                        break;
                }
                if (option != new SelectOption())
                {
                    Task.Delay(TimeSpan.FromSeconds(Math.Clamp(3 - st.Elapsed.Seconds, 0, 3) + 1)).Wait();
                    bool answerResult = CheckIfAnswerIsCorrect(option);
                    Console.WriteLine("Answer is " + (answerResult ? "correct." : "incorrect."));
                    if (answerResult)
                        correct++;
                    else
                        incorrect++;
                    total++;
                    PrintCounter();
                    SelectAnswer(SelectOption.NextQuestion);
                    //to prevent memory leak
                    GC.Collect();
                    Directory.Delete(tempPath, true);
                }
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

        public void PrintCounter()
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"Total questions: {total} Total Correct: {correct} Total Incorrect: {incorrect} %Correct: {Math.Round(d: (correct / total) * 100)}");
        }


        public void SelectAnswer(SelectOption option)
        {
            switch(option)
            {
                case SelectOption.Question:
                    MouseInterface.SetCursorPosition(500*widthScalar, 360 * heightScalar);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
                case SelectOption.Answer1:
                    MouseInterface.SetCursorPosition(500 * widthScalar, 425 * heightScalar);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
                case SelectOption.Answer2:
                    MouseInterface.SetCursorPosition(500 * widthScalar, 475 * heightScalar);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
                case SelectOption.Answer3:
                    MouseInterface.SetCursorPosition(500 * widthScalar, 515 * heightScalar);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
                case SelectOption.Answer4:
                    MouseInterface.SetCursorPosition(500 * widthScalar, 575 * heightScalar);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
                case SelectOption.NextQuestion:
                    MouseInterface.SetCursorPosition(500 * widthScalar, 700 * heightScalar);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftDown);
                    Task.Delay(100).Wait();
                    MouseInterface.MouseEvent(MouseInterface.MouseEventFlags.LeftUp);
                    break;
            }
        }

        public bool CheckIfAnswerIsCorrect(SelectOption option)
        {
            string tempPath = Path.Combine(tempImages, thiranya.Next(99999).ToString());
            var answers = SaveScreenshotsOfQuestionAndAnswers(tempPath, false);
            if (option == SelectOption.Answer1)
            {
                Color avgColor = AverageColorOfImage(answers[1]);
                return avgColor.G > 150;
            }
            else if (option == SelectOption.Answer2)
            {
                Color avgColor = AverageColorOfImage(answers[2]);
                return avgColor.G > 150;
            }
            else if (option == SelectOption.Answer3)
            {
                Color avgColor = AverageColorOfImage(answers[3]);
                return avgColor.G > 150;

            }
            else if (option == SelectOption.Answer4)
            {
                Color avgColor = AverageColorOfImage(answers[4]);
                return avgColor.G > 150;
            }
            //it should never get here
            return false;
        }

        public Color AverageColorOfImage(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            Bitmap input = (Bitmap)Bitmap.FromStream(fs);
            Bitmap greyscale = new Bitmap(input.Width, input.Height);
            KeyValuePair<float, float> aTotal = new(0, 0);
            KeyValuePair<float, float> rTotal = new(0, 0);
            KeyValuePair<float, float> gTotal = new(0, 0);
            KeyValuePair<float, float> bTotal = new(0, 0);
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    Color pixelColor = input.GetPixel(x, y);
                    aTotal = new(aTotal.Key + pixelColor.A, aTotal.Value + 1);
                    rTotal = new(rTotal.Key + pixelColor.R, rTotal.Value+1);
                    gTotal = new(gTotal.Key + pixelColor.G, gTotal.Value + 1);
                    bTotal = new(bTotal.Key + pixelColor.B, bTotal.Value + 1);
                }
            }
            //average it out
            Color value = new Color();
            value = Color.FromArgb(Math.Clamp((int)(aTotal.Key / aTotal.Value), 0, 255),
                Math.Clamp((int)(rTotal.Key / rTotal.Value), 0, 255),
                Math.Clamp((int)(gTotal.Key / gTotal.Value), 0, 255),
                Math.Clamp((int)(bTotal.Key / bTotal.Value), 0, 255));
            fs.Close();
            return value;
        }

        public void SetResolutionScalars()
        {
            //Get resolution of primary display
            screenWidth = DisplayInterface.GetMainDisplaySize();
            widthScalar = Convert.ToInt32(Math.Round((decimal)screenWidth.Width / 1920));
            heightScalar = Convert.ToInt32(Math.Round((decimal)screenWidth.Height / 1080));
            Console.WriteLine($"Display Width: {screenWidth.Width} Display Height: {screenWidth.Height}");
        }

        public string[] SaveScreenshotsOfQuestionAndAnswers(string dirPath, bool enhancement = true)
        {
            Directory.CreateDirectory(dirPath);
            List<string> screenshotPaths= new List<string>();

            //1920X1080 Template
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

            //Take screenshot of primary display.
            Bitmap screenshot = new Bitmap(screenWidth.Width, screenWidth.Height);
            Graphics g = Graphics.FromImage(screenshot);
            g.CopyFromScreen(Point.Empty, Point.Empty, screenshot.Size);
            g.Dispose();

            //Define the crop areas
            Rectangle questionCropArea = new Rectangle(40 *widthScalar, 345 * heightScalar, (920-80) * widthScalar, (385-345) * heightScalar);
            Rectangle answer1CropArea = new Rectangle(40 * widthScalar, 410 * heightScalar, (920 - 80) * widthScalar, (435 - 395) * heightScalar);
            Rectangle answer2CropArea = new Rectangle(40 * widthScalar, 460 * heightScalar, (920 - 80) * widthScalar, (490 - 445) * heightScalar);
            Rectangle answer3CropArea = new Rectangle(40 * widthScalar, 515 * heightScalar, (920 - 80) * widthScalar, (545 - 515) * heightScalar);
            Rectangle answer4CropArea = new Rectangle(40 * widthScalar, 565 * heightScalar, (920 - 80) * widthScalar, (595 - 565) * heightScalar);

            Bitmap questionImageCrop = screenshot.Clone(questionCropArea, PixelFormat.Format32bppRgb);
            Bitmap answer1ImageCrop = screenshot.Clone(answer1CropArea, PixelFormat.Format32bppRgb);
            Bitmap answer2ImageCrop = screenshot.Clone(answer2CropArea, PixelFormat.Format32bppRgb);
            Bitmap answer3ImageCrop = screenshot.Clone(answer3CropArea, PixelFormat.Format32bppRgb);
            Bitmap answer4ImageCrop = screenshot.Clone(answer4CropArea, PixelFormat.Format32bppRgb);

            if (enhancement)
            {
                //Upscale by 20x to improve OCR
                questionImageCrop = ResizeImage(questionImageCrop, questionImageCrop.Width * imageScalar, questionImageCrop.Height * imageScalar);
                answer1ImageCrop = ResizeImage(answer1ImageCrop, answer1ImageCrop.Width * imageScalar, answer1ImageCrop.Height * imageScalar);
                answer2ImageCrop = ResizeImage(answer2ImageCrop, answer2ImageCrop.Width * imageScalar, answer2ImageCrop.Height * imageScalar);
                answer3ImageCrop = ResizeImage(answer3ImageCrop, answer3ImageCrop.Width * imageScalar, answer3ImageCrop.Height * imageScalar);
                answer4ImageCrop = ResizeImage(answer4ImageCrop, answer4ImageCrop.Width * imageScalar, answer4ImageCrop.Height * imageScalar);
            }

            string questionImagePath = Path.Combine(dirPath, "question.png");
            string answer1ImagePath = Path.Combine(dirPath, "answer1.png");
            string answer2ImagePath = Path.Combine(dirPath, "answer2.png");
            string answer3ImagePath = Path.Combine(dirPath, "answer3.png");
            string answer4ImagePath = Path.Combine(dirPath, "answer4.png");

            questionImageCrop.Save(questionImagePath, ImageFormat.Png);
            answer1ImageCrop.Save(answer1ImagePath, ImageFormat.Png);
            answer2ImageCrop.Save(answer2ImagePath, ImageFormat.Png);
            answer3ImageCrop.Save(answer3ImagePath, ImageFormat.Png);
            answer4ImageCrop.Save(answer4ImagePath, ImageFormat.Png);

            //trying to save memory
            questionImageCrop.Dispose();
            answer2ImageCrop.Dispose();
            answer3ImageCrop.Dispose();
            answer4ImageCrop.Dispose();
            answer1ImageCrop.Dispose();

            if (enhancement)
            {
                screenshotPaths.Add((questionImagePath));
                screenshotPaths.Add(EmguEnhancement(answer1ImagePath));
                screenshotPaths.Add(EmguEnhancement(answer2ImagePath));
                screenshotPaths.Add(EmguEnhancement(answer3ImagePath));
                screenshotPaths.Add(EmguEnhancement(answer4ImagePath));
            }
            else
            {
                screenshotPaths.Add((questionImagePath));
                screenshotPaths.Add((answer1ImagePath));
                screenshotPaths.Add((answer2ImagePath));
                screenshotPaths.Add((answer3ImagePath));
                screenshotPaths.Add((answer4ImagePath));
            }

            return screenshotPaths.ToArray();
        }

        public static string EmguEnhancement(string path)
        {
            //read in grayscale
            Mat image = CvInvoke.Imread(path, ImreadModes.Grayscale);

            //apply Gaussian blur to smooth the image
            //CvInvoke.GaussianBlur(image, image, new Size(1, 1), (int)BorderType.Default);

            //apply adaptive thresholding to binarize the image
            CvInvoke.Threshold(image, image, 210, 255, ThresholdType.Binary);

            // Invert the image to make text white and background black
            CvInvoke.BitwiseNot(image, image);

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
            //Try again if read fails
            foreach (tessnet2.Word word in result)
            {
                Results += word.Text+" ";
            }
            result.Clear();
            ocr.Clear();
            ocr.Dispose();
            image.Dispose();
            if (Results.Trim() == "~")
            {
                //return GetTextFromImage(path);
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
                model = "meta/llama-2-13b-chat";
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
