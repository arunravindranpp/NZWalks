using System.Net.Http.Json;
using System.Runtime.Serialization.Json;

namespace ConflictAutomation.Utilities;

public static class HttpClientReq
{
    public static string GetResultAsString(string url)
    {
        try
        {
            using HttpClient client = new();                
            client.Timeout = TimeSpan.FromMinutes(5);

            var response = client.GetStringAsync(url).GetAwaiter().GetResult();

            return response;
        }
        catch (Exception)
        {
            throw;
        }
    }


    public static T GetResultAsJSON<T>(string url)
    {
        try
        {
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromMinutes(5);

            var response = client.GetFromJsonAsync<T>(url).GetAwaiter().GetResult();

            return response!;
        }
        catch (Exception)
        {
            throw;
        }
    }


    public static string GetResultAsFile(string url, string filePath)
    {
        try
        {
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromMinutes(5);

            var response = client.GetStreamAsync(url).GetAwaiter().GetResult();

            if(File.Exists(filePath)) File.Delete(filePath);

            response.CopyTo(
                new FileStream(filePath, FileMode.CreateNew)
            );

            return File.Exists(filePath) ? filePath : null;
        }
        catch (Exception)
        {
            throw;
        }
    }


    public static string Post<T>(string url, T request)
    {
        if (request is null)
            return null;

        try
        {
            using HttpClient client = new();

            var js = new DataContractJsonSerializer(typeof(T));
            var msObj = new MemoryStream();
            js.WriteObject(msObj, request);
            msObj.Position = 0;
            var sr = new StreamReader(msObj);
            string jsonReq = sr.ReadToEnd();

            HttpContent content = new StringContent(jsonReq);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            string responseBody = string.Empty;
            client.Timeout = TimeSpan.FromMinutes(5);

            using HttpResponseMessage responseMsg = client.PostAsync(url, content).GetAwaiter().GetResult();

            if (responseMsg.IsSuccessStatusCode)
            {
                var responseContent = responseMsg.Content;
                responseBody = responseContent.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            return responseBody;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
