using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Configuration;

using RestSharp;

namespace MPackEngine
{
public class MediusMarketApi {
    const string BaseUrl = "http://mf11market.azurewebsites.net/api/applications/";

    readonly string _accountSid;
    readonly string _secretKey;

    /*public MediusMarketApi(string accountSid, string secretKey) {
        _accountSid = accountSid;
        _secretKey = secretKey;
    }*/

    public T Execute<T>(RestRequest request) where T : new()
    {
        var client = new RestClient();
        client.BaseUrl = BaseUrl;
        //client.Authenticator = new HttpBasicAuthenticator(_accountSid, _secretKey);
        //request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
        var response = client.Execute<T>(request);
        //Console.WriteLine("Rest response: {0}", response.Data);
        if (response.ErrorException != null)
        {
            Console.WriteLine("Rest response content: {0}", response.Content);
            Console.WriteLine("Rest response data: {0}", response.Data);
            throw response.ErrorException;
        }

        return response.Data;
    }

    public List<Application> GetApplications() {
        var request = new RestRequest();

        //request.AddParameter("CallSid", callSid, ParameterType.UrlSegment);

        return Execute<List<Application>>(request);
    }

    public Application GetApplication(string name) {
        var request = new RestRequest();
        request.Resource = name;    

        //request.AddParameter("CallSid", callSid, ParameterType.UrlSegment);

        return Execute<Application>(request);
    }

    public Version GetApplicationVersion(string appName, string appVersion) {
        try {
            var request = new RestRequest();
            request.Resource = appName + "/versions/" + appVersion;    

            //request.AddParameter("CallSid", callSid, ParameterType.UrlSegment);

            return Execute<Version>(request);
        }
        catch(Exception ex) {
            Console.WriteLine("Exception during processing {0}", ex);
            return null;
        }
    }

    public void PublishApplication(string fileName) {
        try {
            var request = new RestRequest("", Method.POST);
            request.AddFile("applicationPackage", File.ReadAllBytes(fileName), Path.GetFileName(fileName), "application/octet-stream");

            // Add HTTP Headers
            request.AddHeader("Content-type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.RequestFormat = DataFormat.Json;
            //request.AddParameter("CallSid", callSid, ParameterType.UrlSegment);

            Execute<Version>(request);
        }
        catch(Exception ex) {
            Console.WriteLine("Exception during processing {0}", ex);
           
        }
    }
}
}