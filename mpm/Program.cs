using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mpm
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("You need to provide an argument...");
                MenuItem_Help();
                return;
            }

            var command = args[0];
            var engine = new mpmEngine.Engine(@"E:\GlobalPackageRepository", "");

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


        static public void MenuItem_List(string[] args, mpmEngine.Engine engine)
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
                Console.WriteLine(String.Format("Application: {0}", itemApplication.name));   
                Console.WriteLine(String.Format("Available versions:"));
                foreach (var itemVersion in itemApplication.versions)
                {
                    Console.WriteLine(String.Format(itemVersion.version));    
                }
                Console.WriteLine(String.Format(" "));   
            }
        }

        static public void MenuItem_Info(string[] args, mpmEngine.Engine engine)
        {
            string appName = "";
            string appVersion = "";
            try {
                if (checkArg(1, args, ref appName)) {
                    if (checkArg(2, args, ref appVersion)) {
                        Console.WriteLine(String.Format("Searching for application: {0} ({1})", appName, appVersion));
                    }
                    else {
                        appVersion = "*.*.*.*";
                        Console.WriteLine(String.Format("Searching for application: {0} ({1})", appName, appVersion));
                    }
                } 
                else {
                    throw new System.ArgumentException("Parameter cannot be null.", "Application");
                }

                var result = engine.GetApplicationVersion(appName, appVersion);
                Console.WriteLine(String.Format("Application: {0} ({1})", appName, result.version));

                Console.WriteLine("Dependencies:");
                result.dependencies.ForEach(x => Console.WriteLine(" |- Application: {0} ({1})", x.name, x.version));
            
            }
            catch(Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        static public void MenuItem_Get(string[] args, mpmEngine.Engine engine)
        {

            string appName = "";
            string appVersion = "";
            try {
                if (checkArg(1, args, ref appName)) {
                    if (checkArg(2, args, ref appVersion))
                    {
                        Console.WriteLine(String.Format("Searching for application: {0} ({1})", appName, appVersion));
                    }
                    else
                    {
                        appVersion = "*.*.*.*";
                        Console.WriteLine(String.Format("Searching for application: {0} ({1})", appName, appVersion));
                    }
                } 
                else {
                    throw new System.ArgumentException("Parameter cannot be null.", "Application");
                }

                var applicationVersion = engine.GetApplicationVersion(appName, appVersion);
                Console.WriteLine("Application: {0} ({1})", appName, applicationVersion.version);

                var result = engine.GetFullDownloadListForApplication(appName, appVersion);

                Console.WriteLine("Full download list:");
                result.ForEach(x => Console.WriteLine(x[2]));
                Console.WriteLine(" ");

                Console.WriteLine("Download and unzip files...");
                engine.downloadApplicationVersions(result);

                Console.WriteLine("All content was downloaded and unzipped!");
                }

            catch(Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        static public void MenuItem_Publish(string[] args, mpmEngine.Engine engine)
        {

            string appName = "";
            string appVersion = "";
           
                
                    //Verify that application exists on disk
                    foreach (string[] application in engine.VerifyApplicationVersionOnDiskFromManifest(appName, appVersion))
                    {
                        try
                        {

                        Console.WriteLine("Application: {0}, version {1} exists on disk: " + Environment.NewLine + "{2}", application[0], application[1], application[2]);

                        //Verify if application already exists in registry
                        if (engine.VerifyApplicationVersionInRegistry(application[0], application[1]))
                        {
                            throw new Exception(string.Format("Application: {0}, version {1} exists in registry", application[0], application[1]));
                        }
                        else
                        {
                            Console.WriteLine("Application: {0}, version {1} does NOT in registry", application[0], application[1]);
                        }

                        Console.WriteLine("Are you sure you want to publish application: {0}, version {1}? (y / n)", application[0], application[1]);
                        string line = Console.ReadLine(); // Get string from user
                        if (line != "y") 
                        {
                            Console.WriteLine("Application: {0}, version {1} will NOT be published? ", application[0], application[1]);
                            continue;
                        }

                        Console.WriteLine("Start publishing application: {0}, version: {1}", application[0], application[1]);
                        engine.PublishApplicationVersion(application[2]);
                        Console.WriteLine("Publishing completed!");
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

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
            Console.WriteLine("mpm info");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                Console.WriteLine("Assembly name: {0}", assembly.FullName);
        }
    }
}