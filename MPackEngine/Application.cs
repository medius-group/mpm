using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Dependency
{
    public string name { get; set; }
    public string version { get; set; }
    public string _id { get; set; }
}

public class Version
{
    public string version { get; set; }
    public string downloadUrl { get; set; }
    public string releaseNotes { get; set; }
    public string _id { get; set; }
    public List<Dependency> dependencies { get; set; }
    public string latestUpdate { get; set; }
}

public class Application
{
    public int __v { get; set; }
    public string _id { get; set; }
    public string category { get; set; }
    public string description { get; set; }
    public string name { get; set; }
    public Version latestVersion { get; set; }
    public List<Version> versions { get; set; }
    public List<string> keywords { get; set; }
}