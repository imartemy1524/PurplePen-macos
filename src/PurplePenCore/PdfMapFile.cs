using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PurplePen
{
    // Manages a PDF map file and converting it to a bitmap.
    public class PdfMapFile : IDisposable
    {
        private const string BrowserUnsupportedMessage = "PDF map import is not supported in browser builds.";

        private string pdfFileName;
        private string pngFileName;
        private ConversionStatus status;
        private string conversionOutput;
        private StringBuilder stderrOutput;
        private Process process;
        private bool disposed = false;

        private const int Resolution = 600; // Resolution in DPI
        
        public PdfMapFile(string pdfFileName)
        {
            this.pdfFileName = pdfFileName;
            this.status = ConversionStatus.NotStarted;
        }

        public event EventHandler ConversionCompleted;

        public string PdfFileName {
            get { return pdfFileName; }
        }

        public string PngFileName
        {
            get
            {
                if (Status != ConversionStatus.Success)
                    throw new InvalidOperationException("Cannot get PNG file name until conversion is successful.");
                return pngFileName;
            }
        }

        public bool SourceExists {
            get {
                return File.Exists(pdfFileName);
            }
        }

        public bool PdfConverterExists
        {
            get
            {
                if (IsBrowserRuntime())
                    return false;

                return FindPdfConverterExe() != null;
            }
        }

        public ConversionStatus Status
        {
            get
            {
                return status;
            }
        }

        public string ConversionOutput
        {
            get {
                return conversionOutput;
            }
        }

        // Try to begin conversion into bitmap. 
        public ConversionStatus BeginConversion()
        {
            if (IsBrowserRuntime()) {
                conversionOutput = BrowserUnsupportedMessage;
                status = ConversionStatus.Failure;
                return status;
            }

            if (!SourceExists) {
                conversionOutput = string.Format("File '{0}' does not exist.", pdfFileName);
                status = ConversionStatus.Failure;
                return status;
            }

            CleanCacheDirectory();

            string cacheFileName = GetCacheFileName(pdfFileName);
            if (File.Exists(cacheFileName)) {
                // Cached file still exists. Use it.
                conversionOutput = "";
                pngFileName = cacheFileName;
                status = ConversionStatus.Success;
                return status;
            }

            return BeginUncachedConversion(cacheFileName, Resolution);
        }

        // Try to begin conversion into bitmap. 
        public ConversionStatus BeginUncachedConversion(string fileName, int resolution)
        {
            try {
                string converterPath = FindPdfConverterExe();
                if (converterPath == null) {
                    conversionOutput = MiscText.PdfConverterNotFound;
                    status = ConversionStatus.Failure;
                    return status;
                }

                string converterExe = converterPath;
                string arguments = String.Format("{2} \"{0}\" \"{1}\"", pdfFileName, fileName, resolution);
                if (Path.GetExtension(converterPath).Equals(".dll", StringComparison.OrdinalIgnoreCase)) {
                    converterExe = GetDotNetHostPath();
                    arguments = String.Format("\"{0}\" {3} \"{1}\" \"{2}\"", converterPath, pdfFileName, fileName, resolution);
                }

                stderrOutput = new StringBuilder();
                ProcessStartInfo startInfo = new ProcessStartInfo(converterExe, arguments);
                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                process = new Process();
                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;
                process.ErrorDataReceived += ProcessDataReceived;
                process.OutputDataReceived += ProcessDataReceived;
                process.Exited += ProcessExited;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                status = ConversionStatus.Working;
                pngFileName = fileName;
                return status;
            }
            catch (Exception e) {
                status = ConversionStatus.Failure;

                if (!string.IsNullOrWhiteSpace(pngFileName))
                    File.Delete(pngFileName);

                conversionOutput = e.Message;
                return status;
            }
        }

        private void ProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (stderrOutput) {
                stderrOutput.Append(e.Data);
                stderrOutput.Append("\r\n");
            }
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            process.WaitForExit();

            lock (stderrOutput) {
                conversionOutput = stderrOutput.ToString();
            }

            status = process.ExitCode == 0 ? ConversionStatus.Success : ConversionStatus.Failure;
            process.Dispose();
            process = null;

            if (status == ConversionStatus.Failure && !string.IsNullOrWhiteSpace(pngFileName))
                File.Delete(pngFileName);

            if (ConversionCompleted != null)
                ConversionCompleted(this, EventArgs.Empty);
        }

        internal string FindPdfConverterExe()
        {
            if (IsBrowserRuntime())
                return null;

            Uri uri = new Uri(typeof(PdfMapFile).Assembly.Location);
            string applicationDirectory = Path.GetDirectoryName(uri.LocalPath);
            string dllPath = Path.Combine(applicationDirectory, "PdfConverter.dll");
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && IsCurrentProcessDotNetHost() && File.Exists(dllPath))
                return dllPath;

            string windowsExePath = Path.Combine(applicationDirectory, "PdfConverter.exe");
            if (File.Exists(windowsExePath))
                return windowsExePath;

            string unixExePath = Path.Combine(applicationDirectory, "PdfConverter");
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(unixExePath))
                return unixExePath;

            if (File.Exists(dllPath))
                return dllPath;

            return null;
        }

        private static bool IsBrowserRuntime()
        {
#if NET8_0_OR_GREATER
            return OperatingSystem.IsBrowser();
#else
            return false;
#endif
        }

        private static bool IsCurrentProcessDotNetHost()
        {
            string processPath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(processPath))
                return false;

            string fileName = Path.GetFileNameWithoutExtension(processPath);
            return string.Equals(fileName, "dotnet", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetDotNetHostPath()
        {
            if (IsCurrentProcessDotNetHost())
                return Environment.ProcessPath;

            return "dotnet";
        }

        internal string GetCacheFileName(string path)
        {
            string cacheDirectory = GetCacheDirectory();

            return Path.Combine(cacheDirectory, CalculateSha1(path) + ".png");
        }

        private static string GetCacheDirectory()
        {
            string tempPath = Path.GetTempPath();
            string cacheDirectory = Path.Combine(tempPath, "PurplePen");
            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);
            return cacheDirectory;
        }

        // Clean stale caches (over 6 months old).
        private static void CleanCacheDirectory()
        {
            DateTime oldDate = DateTime.Now.Subtract(TimeSpan.FromDays(180));
            string cacheDirectory = GetCacheDirectory();

            try {
                foreach (string filename in Directory.GetFiles(cacheDirectory, "*.png", SearchOption.TopDirectoryOnly)) {
                    FileInfo fileInfo = new FileInfo(filename);
                    if (fileInfo.Exists && fileInfo.LastWriteTime < oldDate) {
                        fileInfo.Delete();
                    }
                }
            }
            catch {
                // Do nothing. Not a problem if we get an exception here.
            }
        }

        internal string CalculateSha1(string path)
        {
            var hashAlgorithm = System.Security.Cryptography.SHA1.Create();
            byte[] hash = hashAlgorithm.ComputeHash(File.ReadAllBytes(path));
            hash[0] ^= 0xd9;   // Change hash so different from previous (GhostScript)
#if !NETFRAMEWORK
            // NET CORE use different hash from NET Framework, because the converters are different.
            hash[1] ^= 0x7b;   
#endif
            return Hexify(hash);
        }

        private string Hexify(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes) {
                builder.Append(b.ToString("X2"));
            }
            return builder.ToString();
        }

        // Implement IDisposable to ensure the process field is disposed.
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Dispose managed resources.
                try {
                    if (process != null)
                    {
                        process.Dispose();
                        process = null;
                    }
                }
                catch {
                    // Swallow exceptions during dispose.
                }
            }

            disposed = true;
        }

        public enum ConversionStatus
        {
            NotStarted, Success, Failure, Working
        }
    }
}
