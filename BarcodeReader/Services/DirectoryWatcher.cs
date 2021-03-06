using RawPrint;
using Spire.Pdf;
using System;
using System.IO;
using System.Printing;
using System.Text.RegularExpressions;
using System.Windows;

namespace BarcodeReader.Services
{
    /*
     * DOC = https://docs.microsoft.com/fr-fr/dotnet/api/system.io.filesystemwatcher?view=net-6.0
     */
    public class DirectoryWatcher
    {
        public string Directory { get; set; }
        public static string PrinterName { get; set; }
        public FileSystemWatcher watcher;
        public static string PathFilePrint { get; set; }
        public static string[] ListFileName { get; set; }

        public DirectoryWatcher(string dir, string[] filename = null, string ext = "*", string printerName = "Adobe PDF")
        {
            Directory = dir;
            PrinterName = printerName;
            ListFileName = filename;
            watcher = new FileSystemWatcher(dir);
            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += OnChanged;
            watcher.Filter =  "*." + ext;
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            
            ListFileName = ListFileName ?? new string[] { "*" };
            foreach (string filename in ListFileName)
            {
                if (Regex.IsMatch(e.Name, filename, RegexOptions.IgnoreCase))
                {
                    System.Threading.Thread.Sleep(1000);
                    PathFilePrint = e.FullPath;
                    if (File.Exists(PathFilePrint))
                    {
                        try
                        {
                            PdfDocument printer = new PdfDocument();
                            printer.LoadFromFile(PathFilePrint);
                            printer.PrintSettings.PrinterName = PrinterName;
                            printer.Print();
                            File.Delete(PathFilePrint);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }


                    }
                }
            }
        }
    }
}
