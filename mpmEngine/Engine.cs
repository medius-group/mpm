using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using System.Xml.Serialization;
using System.Configuration;
/*
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
*/
namespace mpmEngine
{
    public class Engine
    {
        private string _foreignPackageStore;
        private string _localPackageStore;
        private string _localPackageRepositoryList;
        private MediusMarketApi _mediusMarketApi;
        string _manifestFileName;
        private List<Application> _applicationCollection;

        public Engine(string foreignPackageStore, string localPackageStore)
        {
            _foreignPackageStore = foreignPackageStore;
            _localPackageStore = localPackageStore;
            _manifestFileName = ConfigurationManager.AppSettings.Get("ManifestFileName");
            _mediusMarketApi = new MediusMarketApi();
        }

        public List<Application> GetApplicationList()
        {
            return _mediusMarketApi.GetApplications();
        }

        public Application GetApplication(string name)
        {
            return _mediusMarketApi.GetApplication(name);
        }

        public Version GetApplicationVersion(string appName, string appVersion)
        {
            return _mediusMarketApi.GetApplicationVersion(appName, appVersion);
        }

        public List<string[]> GetFullDownloadListForApplication(string appName, string appVersion)
        {
            List<string[]> results = new List<string[]>();
            var result = _mediusMarketApi.GetApplicationVersion(appName, appVersion);

            results.Add(new String[] { appName, result.version, result.downloadUrl });
            foreach (Dependency itemDependency in result.dependencies)
            {
                try {
                    result = _mediusMarketApi.GetApplicationVersion(itemDependency.name, itemDependency.version);    
                    results.Add(new String[] {itemDependency.name, result.version, result.downloadUrl});
                }                    
                catch(Exception ex)
                {
                    Console.WriteLine("Application {0}, version {1} could not be added to download list: ", itemDependency.name, itemDependency.version);
                    Console.WriteLine("Error message: {0}", ex.Message);
                }
            }
            return results;
        }

        public void downloadApplicationVersions(List<string[]> downloadVersions) {
            foreach (string[] itemDownloadVersion in downloadVersions)
            {
                //Console.WriteLine("Download application {0}, version {1}, from {2}...", itemDownloadVersion[0], itemDownloadVersion[1], itemDownloadVersion[2]);

                //var destAppDir = destRootDir.CreateSubdirectory(package.ApplicationName +"_"+ package.ApplicationVersion);
                try {
                    WebClient webClient = new WebClient();
                    string fileName = itemDownloadVersion[0].ToLower() + "_" + itemDownloadVersion[1].ToLower() + ".zip";
                    string filePath = Path.Combine(ConfigurationManager.AppSettings.Get("PackageRepositoryTempPath"), fileName); 
                    Console.WriteLine("Download: {0}", itemDownloadVersion[2]);
                    webClient.DownloadFile(itemDownloadVersion[2], filePath);

                    //stream data = webClient.OpenRead(itemDownloadVersion[2]);
                    // This stream cannot be opened with the ZipFile class because CanSeek is false.
                    UnzipFileNet(filePath);
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    while (ex != null)
                        {
                            Console.WriteLine(ex.Message);
                            ex = ex.InnerException;
                        }
                }
            }
        }
/*
        public void UnzipFile(string filePath) {

            // Perform simple parameter checking.
            
            if ( !File.Exists(filePath) ) {
                Console.WriteLine("Cannot find file '{0}'", filePath);
                return;
            }

            string baseDirectoryName = Path.Combine(ConfigurationManager.AppSettings.Get("PackageRepositoryPath"), Path.GetFileNameWithoutExtension(filePath).ToLower());

            if ( baseDirectoryName.Length > 0 ) {
                        Directory.CreateDirectory(baseDirectoryName);
            }

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(filePath))) {
            
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null) {
                    
                    string directoryName = Path.GetDirectoryName(Path.Combine(baseDirectoryName , theEntry.Name));
                    string fileName      = Path.GetFileName(theEntry.Name);
                    
                    // create directory
                    if ( directoryName.Length > 0 ) {
                        Directory.CreateDirectory(directoryName);
                    }
                    
                    if (fileName != String.Empty) {
                        using (FileStream streamWriter = File.Create(Path.Combine(baseDirectoryName , theEntry.Name))) {
                        
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true) {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0) {
                                    streamWriter.Write(data, 0, size);
                                } else {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        */
        public string ZipApplicationNet (string directoryPath)
        {
            directoryPath = Path.GetFullPath(directoryPath);
            Console.WriteLine("directoryPath: {0}", directoryPath);
            string folderName = new DirectoryInfo(directoryPath).Name;
            string zipPath = Path.Combine(ConfigurationManager.AppSettings.Get("PackageRepositoryTempPath"), (folderName + ".zip"));
            ZipFile.CreateFromDirectory(directoryPath, zipPath);
            return zipPath;
        }

        public string UnzipFileNet (string zipPath)
        {
            string extractPath = Path.Combine(ConfigurationManager.AppSettings.Get("PackageRepositoryPath"), Path.GetFileNameWithoutExtension(zipPath).ToLower());

            ZipFile.ExtractToDirectory(zipPath, extractPath);
            return extractPath;
        } 
        /*
        public string ZipApplication (string directoryPath)
        {
            // Perform some simple parameter checking.  More could be done
            // like checking the target file name is ok, disk space, and lots
            // of other things, but for a demo this covers some obvious traps.

            if ( !Directory.Exists(directoryPath) ) {
                Console.WriteLine("Cannot find directory '{0}'", directoryPath);
                return null;
            }

            try
            {
                directoryPath = Path.GetFullPath(directoryPath);
                int trimOffset = Path.GetFullPath(directoryPath).Length + 1;
                Console.WriteLine("directoryPath: {0}", directoryPath);
                //string[] filenames = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
                List<string> fileSystemEntries = new List<string>();
                fileSystemEntries.AddRange(Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories).Select(d => d + "\\"));
                fileSystemEntries.AddRange(Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories));
                string folderName = new DirectoryInfo(directoryPath).Name;
                string fileName = Path.Combine(ConfigurationManager.AppSettings.Get("PackageRepositoryTempPath"), (folderName + ".zip"));
                Console.WriteLine("Zip application to {0}", fileName);
                // 'using' statements guarantee the stream is closed properly which is a big source
                // of problems otherwise.  Its exception safe as well which is great.
                using (ZipOutputStream s = new ZipOutputStream(File.Create(fileName))) {
                
                    s.SetLevel(8); // 0 - store only to 9 - means best compression
            
                    byte[] buffer = new byte[4096];
                    
                    foreach (string file in fileSystemEntries) {
                        // Using GetFileName makes the result compatible with XP
                        // as the resulting path is not absolute.
                        ZipEntry entry = new ZipEntry(file.Substring(trimOffset));
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);

                        if (file.EndsWith(@"\")) {
                            Console.WriteLine("Path: {0}", file.Substring(trimOffset));
                            continue;
                        }
                        // Setup the entry data as required.
                        
                        // Crc and size are handled by the library for seakable streams
                        // so no need to do them here.

                        // Could also use the last write time or similar for the file.
                        
                        
                        using ( FileStream fs = File.OpenRead(file) ) {
            
                            // Using a fixed size buffer here makes no noticeable difference for output
                            // but keeps a lid on memory usage.
                            int sourceBytes;
                            do {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while ( sourceBytes > 0 );
                        }
                    }
                    
                    // Finish/Close arent needed strictly as the using statement does this automatically
                    
                    // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                    // the created file would be invalid.
                    s.Finish();
                    
                    // Close is important to wrap things up and unlock the file.
                    s.Close();
                }
                return fileName;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception during processing {0}", ex);
                return null;
                
                // No need to rethrow the exception as for our purposes its handled.
            }
        }
        */
        public void PublishApplicationVersion(string applicationName, string applicationVersion) {
            //Zip application
            Console.WriteLine("Start zipping application: {0}, version: {1}", applicationName, applicationVersion);
            var fileName = ZipApplicationNet(Path.Combine(ConfigurationManager.AppSettings.Get("PackageRepositoryPath"), (applicationName + "_" + applicationVersion)));
            //Upload application
            _mediusMarketApi.PublishApplication(fileName);

            //Delete zip
        }

        public bool VerifyApplicationVersionInRegistry(string applicationName, string applicationVersion) {
            var result = _mediusMarketApi.GetApplicationVersion(applicationName, applicationVersion);
            if (result == null) {
                return false;
            } else {
                return true;
            }
        }

        public bool VerifyApplicationVersionOnDisk(string applicationName, string applicationVersion) {
            return Directory.Exists(Path.Combine(ConfigurationManager.AppSettings.Get("PackageRepositoryPath"), (applicationName + "_" + applicationVersion)));
        }
    }
}
