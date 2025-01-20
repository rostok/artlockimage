using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
// using System.Security.Permissions;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
// using Windows.Storage;
// using Windows.Storage.Streams;
using Windows.System.UserProfile;
using System.Linq;
// using System.Windows;
using System.ComponentModel.DataAnnotations;

[assembly: AssemblyProduct("ArtLockImage")]
[assembly: AssemblyTitle("artlockimage")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("rostok - https://github.com/rostok/")]
[assembly: AssemblyTrademark("rostok")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.6.0")]
[assembly: AssemblyFileVersion("1.0.6")]
[assembly: System.Runtime.Versioning.SupportedOSPlatformAttribute("windows")]

namespace ArtLockImage
{
    internal static class Program
    {
        // Log file path
        public static string logFile = "";
        
        // Control if the console output is shown
        public static bool showConsole = true;

        // Exit the application with a fatal error message
        static void Exit(string msg)
        {
            WriteLine("FATAL ERROR! " + msg);
            System.Environment.Exit(1);
        }

        // DLL Imports for the console and system metrics
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int GetSystemMetrics(int nIndex);

        // Function to write a line to console or a log file
        public static void WriteLine(string s)
        {
            if (GetParam("-q")) showConsole = false;
            if (showConsole)
            {
                if (!AttachConsole(-1)) AllocConsole();
                Console.WriteLine(s);
            }
            else
            {
                if (logFile == "")
                {
                    logFile = GetParam("-t") ? Path.GetTempPath() : "" + "artlockimage.log";
                    File.Delete(logFile);
                }
                File.AppendAllText(logFile, s + "\\n");
            }
        }

        // Function to trim the start of a string
        public static string TrimStart(string source, params string[] str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (source.StartsWith(str[i], System.StringComparison.Ordinal))
                {
                    source = source.Substring(str[i].Length);
                    break;
                }
            };
            return source;
        }

        // Function to trim the end of a string
        public static string TrimEnd(string source, params string[] str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (source.EndsWith(str[i], System.StringComparison.Ordinal))
                {
                    source = source.Substring(0, source.Length - str[i].Length);
                    break;
                }
            };
            return source;
        }

        // Function to trim both ends of a string
        public static string TrimBoth(string source, params string[] str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                source = TrimStart(source, str[i]);
                source = TrimEnd(source, str[i]);
            }
            return source;
        }

        // Function to download a file or content of a url
        static async Task<string> Download(string url, string filename = "")
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var clientHandler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
                var client = new HttpClient(clientHandler);
                clientHandler.UseProxy = false;
                if (filename == "")
                {
                    var s = await client.GetStreamAsync(url);
                    string b = (new StreamReader(s)).ReadToEnd();
                    return b;
                }
                else
                {
                    byte[] fileBytes = await client.GetByteArrayAsync(url);
                    File.WriteAllBytes(filename, fileBytes);
                    return "";
                }
            }
            catch (Exception e)
            {
                WriteLine("connection failed for " + url); //\\nException:"+e);
                WriteLine("Exception:"+e);
            }
            return "";
        }

        // Function to get a single image from a url
        static async Task<string> GetSingleImage(string url, List<string> urls, int total)
        {
            try
            {
                var body = await Download(url);
                var match = Regex.Match(body, @"Art\.nsf\/O\/\w+\/\$File\/.*\.(jpg|JPG)");
                string img = match.Success ? "https://en.most-famous-paintings.com/" + match.Value : "";
                match = Regex.Match(body, @"(h|H)1.*(title|TITLE)=""(.+?)""");
                string tit = match.Success ? match.Groups[3].Value.Trim().Replace(";", ",") : "";
                match = Regex.Match(body, @"\>(.*?)\<\/h2");
                string aut = match.Success ? match.Groups[1].Value.Trim().Replace(";", ",") : "";
                urls.Add(img + ";" + tit + ";" + aut);
                Console.Write(Math.Round(urls.Count * 1000.0f / total) / 10 + "%\r");
                return img;
            }
            catch (Exception e)
            {
                Exit("failed getting image " + url + " e:" + e);
            }
            return "";
        }

        // Function to resize an image
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var g = Graphics.FromImage(destImage))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        // Function to transform an image
        static void Transform(string input, string output, string text = "", string[] args = null)
        {
            if (!File.Exists(input)) return;
            File.Copy(input, output, true);
            int th = 20;
            int m = 40;
            int m2 = m / 2;
            var sw = GetSystemMetrics(0);
            var sh = GetSystemMetrics(1);
            Image ii = Image.FromFile(input);
            Image io = new Bitmap(sw + m, sh + m, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(io))
            {
                int w = ii.Width;
                int h = ii.Height;
                int nh = sh - th;
                int nw = w * nh / h;
                if (nw > sw)
                {
                    nh = nh * sw / nw;
                    nw = sw;
                }
                ii = ResizeImage(ii, nw, nh);
                SolidBrush br = new SolidBrush(Color.FromArgb(32, 32, 32));
                g.FillRectangle(br, new Rectangle(0, 0, io.Width, io.Height));
                g.DrawString(text, new Font("Arial", 12), Brushes.Black, m2 + 2, m2 + 2);
                g.DrawString(text, new Font("Arial", 12), Brushes.White, m2 + 1, m2 + 1);

                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawImage(ii, new Rectangle(m2, m2 + th, nw, nh), 0, 0, nw, nh, GraphicsUnit.Pixel);
            }
            var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            var encParams = new EncoderParameters() { Param = new[] { new EncoderParameter(Encoder.Quality, 90L) } };
            io.Save(output, encoder, encParams);
        }

        // Function to set lock screen image
        static async Task<int> SetLockImage(string path)
        {
            var folder = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);

            try
            {
                var stream = System.IO.File.OpenRead(path);
                await LockScreen.SetImageStreamAsync(stream.AsRandomAccessStream());
            }
            catch (Exception e)
            {
                Exit("exception raised! " + e);
            }
            return 0;
        }

        // Main function with async tasks
        static async Task<int> MainAsync(string[] args)
        {
            foreach (var arg in args)
            {
                if (File.Exists(arg)) {
                    await SetLockImage(args[0]);
                    return 0;
                }
            }
            if (!File.Exists("urls"))
            {
                WriteLine("no urls file, downloading");
                string body = await Download("https://en.most-famous-paintings.com/MostFamousPaintings.nsf/ListOfTop1000MostPopularPainting?OpenForm");
                if (body == "") Exit("body is empty");

                var matches = Regex.Matches(body, "A\\?Open&A=\\w+");
                var urls = new List<string>();
                // var tasks = new List<Task>();
                // for (int i = 0; i < matches.Count; i++) tasks.Add(GetSingleImage("https://en.most-famous-paintings.com/MostFamousPaintings.nsf/" + matches[i].ToString(), urls, matches.Count));
                // await Task.WhenAll(tasks.ToArray());

                var semaphore = new SemaphoreSlim(2); // Limit to 5 concurrent requests

                var tasks = matches.Select(async match =>
                {
                    await Task.Delay(100); // 200ms delay
                    await semaphore.WaitAsync();
                    try
                    {
                        await GetSingleImage($"https://en.most-famous-paintings.com/MostFamousPaintings.nsf/{match}", urls, matches.Count);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);


                File.AppendAllLines("urls", urls);
            }
            if (File.Exists("urls"))
            {
                var lines = File.ReadAllText("urls").Trim().Split('\n');
                var patterns = args.Where(arg => !arg.StartsWith("-")).Select(p=>p.ToLower()).ToArray();
                lines = lines.Where(line => patterns.All(pattern => line.ToLower().Contains(pattern))).ToArray();

                var line = lines[(new Random()).Next(lines.Length)];
                WriteLine(line);
                var vals = line.Split(';');
                if (vals.Length < 1) Exit("bad line in urls file:" + vals);
                string tit = vals.ElementAtOrDefault(1) ?? string.Empty;
                string aut = vals.ElementAtOrDefault(2) ?? string.Empty;
                string temppath = "";
                if (GetParam("-t")) temppath = Path.GetTempPath();
                File.Delete(temppath + "image.jpg");
                await Download(vals[0], temppath + "image.jpg");
                Transform(temppath + "image.jpg", temppath + "output.jpg", TrimBoth(aut + " - " + tit, " - "), args);
                await SetLockImage(temppath + "output.jpg");
            }
            return 0;
        }

        // Function to create a scheduled task
        static void CreateTask()
        {
            var process = Process.GetCurrentProcess(); // Or whatever method you are using
            string fullPath = process.MainModule.FileName;

            var xml = @"
<?xml version='1.0' encoding='UTF-16'?>
<Task version='1.4' xmlns='http://schemas.microsoft.com/windows/2004/02/mit/task'>
  <Triggers>
    <SessionStateChangeTrigger>
      <StateChange>SessionLock</StateChange>
    </SessionStateChangeTrigger>
    <BootTrigger />
  </Triggers>
  <Actions Context='Author'>
    <Exec>
      <Command>" + process.MainModule.FileName + @"</Command>
      <Arguments>" + (GetParam("-t") ? "-t" : " ") + (GetParam("-q") ? "-q" : " ") + @"</Arguments>
      <WorkingDirectory>" + Path.GetDirectoryName(process.MainModule.FileName) + @"</WorkingDirectory>
    </Exec>
  </Actions>
</Task>                
            ";
            File.WriteAllTextAsync("artlockimagetask.xml", xml.Trim());
            System.Diagnostics.Process.Start("schtasks", "/create /tn ArtLockImage /xml artlockimagetask.xml /f");
        }

        // Function to get command line parameters
        static bool GetParam(string param, out string arg)
        {
            arg = "";
            var args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].ToLower() == param.ToLower())
                {
                    if (i + 1 < args.Length) arg = args[i + 1];
                    return true;
                }
            }
            return false;
        }
        static bool GetParam(string param) { string o; return GetParam(param, out o); }

        // Main function
        static int Main(string[] args)
        {
            if (GetParam("-h") || GetParam("/?") || GetParam("--help"))
            {
                WriteLine("ArtLockImage (v" + Assembly.GetExecutingAssembly().GetName().Version + ") changes logon screen background to random image from links provided in urls file.");
                WriteLine("The format for each line is: link;author;title");
                WriteLine("If no urls file is avaiable ~1000 image links are downloaded from most-famous-paintings.com website.");
                WriteLine("");
                WriteLine("syntax: lockimage [imagefile|pattern|options]");
                WriteLine("");
                WriteLine("options:");
                WriteLine("        imagefile  sets lock screen image without any transformation, file must extist");
                WriteLine("        pattern    if provided - filters urls to one or more patterns with and condition");
                WriteLine("        -ct        creates 'ArtLockImage' scheduled Windows task triggerred logoff along with -t -q options");
                WriteLine("        -dt        deletes 'ArtLockImage' scheduled task");
                WriteLine("        -t         use temporary path for image storage");
                WriteLine("        -h         shows this help");
                WriteLine("        -q         quiet, log output to artlockimage.log");
                WriteLine("");
                WriteLine("All this comes with MIT license from rostok - https://github.com/rostok/");
                return 0;
            }
            if (GetParam("-ct"))
            {
                CreateTask();
                return 0;
            }
            if (GetParam("-dt"))
            {
                System.Diagnostics.Process.Start("schtasks", "/delete /tn ArtLockImage /f");
                return 0;
            }
            MainAsync(args).Wait();
            return 0;
        }
   }
}
