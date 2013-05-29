using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPack
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("You need to provide an argument");
                MenuItem_Help();
                return;
            }

            var command = args[0];
            var engine = new MPackEngine.Engine(@"E:\GlobalPackageRepository", "");

            switch (args[0].ToUpper())
            {
                case "LIST":
                    MenuItem_List(args, engine);
                    break;
                case "GET":
                    MenuItem_Get(args, engine);
                    break;
                case "PUBLISH":
                    MenuItem_Publish(args, engine);
                    break;
                case "INFO":
                    MenuItem_Info(args, engine);
                    break;
                case "HELP":
                    MenuItem_Help();
                    break;
                case "ABOUT":
                    MenuItem_About();
                    break;
                default:
                    Console.WriteLine("The argument does not exists!");
                    MenuItem_Help();
                    break;
            }

            Console.WriteLine("Done!");
        }

        static private bool checkArg(int index, string[] arg, ref string value)
        {
            if (arg.Length > index)
            {
                value = arg[index];
                return true;
            }
            else
                return false;
        }


        static public void MenuItem_List(string[] args, MPackEngine.Engine engine)
        {
            string filterCommand = "";
            List<Application> result = new List<Application>();

            if (checkArg(1, args, ref filterCommand)) {
                result.Add(engine.GetApplication(filterCommand));
            }
            else {
                result = engine.GetApplicationList();
            }
                
            foreach (var itemApplication in result)
            {
                Console.WriteLine(String.Format("-----------------------------"));
                Console.WriteLine(String.Format("Application: {0}", itemApplication.name));   
                foreach (var itemVersion in itemApplication.versions)
                {
                    Console.WriteLine(String.Format("Version: {0}", itemVersion.version));    
                }
                Console.WriteLine(String.Format("-----------------------------"));   
            }
        }

        static public void MenuItem_Info(string[] args, MPackEngine.Engine engine)
        {
            string appName = "";
            string appVersion = "";
            if (checkArg(1, args, ref appName) && checkArg(2, args, ref appVersion))
            {
                try
                {
                    //var result = engine.GetCurrentPackageRepositoryList().GetPackage(appName, appVersion);
                    var result = engine.GetApplicationVersion(appName, appVersion);
                    Console.WriteLine("Getting app info:");
                    Console.WriteLine(String.Format("Application: {0} - Version: {1}", appName, result.version));

                    Console.WriteLine("Dependecies:");
                    result.dependencies.ForEach(x => Console.WriteLine(" |- Application: {0}, version: {1}", x.name, x.version));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Check input, give app name as first argument and version as the second", appName, appVersion);
            }
        }

        static public void MenuItem_Get(string[] args, MPackEngine.Engine engine)
        {

            string appName = "";
            string appVersion = "";
            if (checkArg(1, args, ref appName) && checkArg(2, args, ref appVersion))
            {
                try
                {
                    //var application = engine.GetCurrentPackageRepositoryList().GetPackage(appName, appVersion);
                    var applicationVersion = engine.GetApplicationVersion(appName, appVersion);
                    Console.WriteLine("Selected app to download: {0} - {1}", appName, applicationVersion.version);

                    //var result = engine.GetCurrentPackageRepositoryList().GetFullDownloadListForApplication(appName, appVersion);

                    var result = engine.GetFullDownloadListForApplication(appName, appVersion);

                    Console.WriteLine("Full download list:");
                    result.ForEach(x => Console.WriteLine("Download URL: {0}", x[2]));

                    Console.WriteLine("Downloading...");
                    engine.downloadApplicationVersions(result);

                    Console.WriteLine("All content was downloaded!");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Check input, give app name as first argument and version as the second", appName, appVersion);
            }

        }

        static public void MenuItem_Publish(string[] args, MPackEngine.Engine engine)
        {

            string appName = "";
            string appVersion = "";
            if (checkArg(1, args, ref appName) && checkArg(2, args, ref appVersion))
            {
                try
                {

                    //Verify that application exists on disk
                    if (engine.VerifyApplicationVersionOnDisk(appName, appVersion)) {
                        Console.WriteLine("Application: {0}, version {1} exists on disk", appName, appVersion);            
                    } else {
                        throw new Exception(string.Format("Application: {0}, version {1} does NOT exist on disk", appName, appVersion));
                    }

                    //Verify if application already exists in registry
                    if (engine.VerifyApplicationVersionInRegistry(appName, appVersion)) {
                        throw new Exception(string.Format("Application: {0}, version {1} exists in registry", appName, appVersion));
                    } else {
                        Console.WriteLine("Application: {0}, version {1} does NOT in registry", appName, appVersion);
                    }
                    
                    engine.PublishApplicationVersion(appName, appVersion);
                    //Zip application

                    //Upload application

                    //Delete zip
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Check input, give app name as first argument and version as the second", appName, appVersion);
            }

        }

        static public void MenuItem_Help()
        {

            Console.WriteLine("Help section:");
            Console.WriteLine("----List [Filter]");
            Console.WriteLine("Lists all applications available on Mediusflow Market, Update needs to be performed the first time or when the list is going to be updated with new applications.");

            Console.WriteLine("----Info [AppName] [AppVersion]");
            Console.WriteLine("Get's more detailed information about an application.");

            Console.WriteLine("----Get [AppName] [AppVersion]");
            Console.WriteLine("Get's the application and it's dependencies.");
        }

        static public void MenuItem_About()
        {
            Console.WriteLine("MPack info");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                Console.WriteLine("Assembly name: {0}", assembly.FullName);
        }
    }
}