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
using System.Xml;

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

        public void PublishApplicationVersion(string filePath) {            
            var fileName = ZipApplicationNet(Path.GetDirectoryName(filePath));
            //Upload application
            _mediusMarketApi.PublishApplication(fileName);
            Console.Write(fileName);
            File.Delete(fileName);

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

        public List<string[]> VerifyApplicationVersionOnDiskFromManifest(string applicationName, string applicationVersion) {
            List<string[]> results = new List<string[]>();
            string xmlApplicationName;
            string xmlApplicationVersion;
            string compareApplicationName;
            string compareApplicationVersion;
            XmlDocument xmlDoc = new XmlDocument();
            
      
            string[] filePaths = Directory.GetFiles(ConfigurationManager.AppSettings.Get("PackageRepositoryPath"), "Manifest.xml", SearchOption.AllDirectories);
            
            foreach (string filePath in filePaths) {
                xmlDoc.Load(filePath);
                xmlApplicationName = xmlDoc.SelectSingleNode("/Package").Attributes["Name"].Value;
                xmlApplicationVersion = xmlDoc.SelectSingleNode("/Package").Attributes["Version"].Value;
                
                if (String.IsNullOrEmpty(applicationName))
                {
                    compareApplicationName = xmlApplicationName;
                }
                else
                {
                    compareApplicationName = applicationName;
                }

                if (String.IsNullOrEmpty(applicationVersion))
                {
                    compareApplicationVersion = xmlApplicationVersion;
                }
                else
                {
                    compareApplicationVersion = applicationVersion;
                }

                if (String.Equals(xmlApplicationName, compareApplicationName, StringComparison.OrdinalIgnoreCase) && String.Equals(xmlApplicationVersion, compareApplicationVersion, StringComparison.OrdinalIgnoreCase))
                {
                    string[] result = new string[3];
                    result[0] = xmlApplicationName; 
                    result[1] = xmlApplicationVersion; 
                    result[2] = filePath;
                    results.Add(result);
                } 
            }
            return results;
        }
    }
}
