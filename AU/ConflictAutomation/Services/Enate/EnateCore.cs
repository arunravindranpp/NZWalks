using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using Nest;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Serilog;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;

namespace ConflictAutomation.Services.Enate;

#pragma warning disable CS0168 // Variable is declared but never used
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE0290 // Use primary constructor
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0017 // Simplify object initialization
#pragma warning disable IDE0090 // Use 'new(...)'
public class EnateCore
{
    private readonly AppConfigure _configuration;
    public EnateCore(AppConfigure configuration)
    {
        _configuration = configuration;
    }
    public static string Authenticate()
    {

        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(60);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();

                        var enateCredentials = new EnateCredentials() { Username = Program.Enate_UserName, Password = Program.Enate_Password };

                        var request = new StringContent(enateCredentials.ToJson(), Encoding.UTF8, "application/json");

                        try
                        {
                            var response = client.PostAsync(Program.Enate_Authentication_Login, request).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {

                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();


                                if (response.StatusCode == HttpStatusCode.OK) //StatusCode: 204 and jsonResponse: "" implies user successfully authenticated.
                                    return jsonResponse.Replace("\"", ""); //Replacing the additional double quote is vital.
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Authentication_Login} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return "";
    }
    public static bool SetLiveMode(string authToken)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(30);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);

                        try
                        {
                            var response = client.PostAsync(Program.Enate_Authentication_SetLiveMode, null).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                if (response.StatusCode == HttpStatusCode.NoContent) ////StatusCode: 204 and jsonResponse: "" implies user successfully logged out.
                                    return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Authentication_SetLiveMode} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return false;
    }
    public static bool Logout(string authToken)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(30);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);

                        try
                        {
                            var response = client.PostAsync(Program.Enate_Authentication_Logout, null).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                if (response.StatusCode == HttpStatusCode.NoContent) ////StatusCode: 204 and jsonResponse: "" implies user successfully logged out.
                                    return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Authentication_Logout} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return false;
    }
    public static CasePacket Case_CreateCase(string authToken, string CaseAttributeVersionGUID, CheckInfo checkInfo)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(120);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));                        

                        var casePacket = new CasePacket()
                        {
                            CaseAttributeVersionGUID = CaseAttributeVersionGUID,
                            Title = checkInfo.EntityName + " - " + checkInfo.CheckId,
                            Description = checkInfo.EntityName + " - " + checkInfo.CheckId,
                            DataFields = checkInfo,
                            DoNotSendAutomatedEmailsToContacts = false,
                          Problem = false
                        };
                        var t = checkInfo.ToJson();
                        var request = new StringContent(casePacket.ToJson(), Encoding.UTF8, "application/json");

                        try
                        {
                            var response = client.PostAsync(Program.Enate_Case_Create, request).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                CasePacket data = jsonResponse.FromJson<CasePacket>();
                                return data;
                            } else
                            {
                                Log.Information($"Failed enate call {Program.Enate_Case_Create} API." + response);
                                Log.Information($"Failed enate call {Program.Enate_Case_Create} API. Request Content: " + request.ReadAsStringAsync().Result);
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Authentication_Login} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return new CasePacket();
    }
    public static List<ProcessContext_Company> GetCompanies(string authToken)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(60);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        try
                        {
                            var response = client.GetAsync(Program.Enate_ProcessContext_GetCompanies).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                List<ProcessContext_Company> data = jsonResponse.FromJson<List<ProcessContext_Company>>();
                                return data;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_ProcessContext_GetCompanies} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return new List<ProcessContext_Company>();
    }
    public static ProcessContext_Contracts GetContracts(string authToken, string companyGuid, int offset, int pageSize)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(60);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        try
                        {
                            var response = client.GetAsync($"{Program.Enate_ProcessContext_GetContracts}?companyGuid={companyGuid}&offset={offset}&pageSize={pageSize}").GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                ProcessContext_Contracts data = jsonResponse.FromJson<ProcessContext_Contracts>();
                                return data;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_ProcessContext_GetContracts} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return new ProcessContext_Contracts();
    }
    public static List<ProcessContext_Services> GetServices(string authToken, string contractGuid)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(60);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        try
                        {
                            var response = client.GetAsync($"{Program.Enate_ProcessContext_GetServices}?contractGuid={contractGuid}").GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                List<ProcessContext_Services> data = jsonResponse.FromJson<List<ProcessContext_Services>>();
                                return data;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_ProcessContext_GetContracts} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return new List<ProcessContext_Services>();
    }
    public static ProcessContext_Processes GetProcesses(string authToken, string serviceGuid, int offset, int pageSize)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(60);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        try
                        {
                            var response = client.GetAsync($"{Program.Enate_ProcessContext_GetProcesses}?serviceGuid={serviceGuid}&offset={offset}&pageSize={pageSize}").GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                ProcessContext_Processes data = jsonResponse.FromJson<ProcessContext_Processes>();
                                return data;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_ProcessContext_GetProcesses} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return new ProcessContext_Processes();
    }
    public static ProcessContext_Processes GetVersions(string authToken, string caseAttributeGuid)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(60);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        try
                        {
                            var response = client.GetAsync($"{Program.Enate_CaseAttribute_GetVersions}?caseAttributeGuid={caseAttributeGuid}").GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                //ProcessContext_Processes data = jsonResponse.FromJson<ProcessContext_Processes>();
                                //return data;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_CaseAttribute_GetVersions} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return new ProcessContext_Processes();
    }
    public static ProcessContext_Processes CaseAttribute_Get(string authToken, string caseAttributeVersionGuid)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(120);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        try
                        {
                            var response = client.GetAsync($"{Program.Enate_CaseAttribute_Get}?caseAttributeVersionGuid={caseAttributeVersionGuid}").GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                //ProcessContext_Processes data = jsonResponse.FromJson<ProcessContext_Processes>();
                                //return data;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_CaseAttribute_Get} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return new ProcessContext_Processes();
    }
    public static ProcessContext_Processes Case_Get(string authToken, string caseGuid, string existingPacketActivityGUID)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(120);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        try
                        {
                            var response = client.GetAsync($"{Program.Enate_Case_Get}?caseGuid={caseGuid}&existingPacketActivityGUID={existingPacketActivityGUID}").GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                //ProcessContext_Processes data = jsonResponse.FromJson<ProcessContext_Processes>();//Todo. Dilip. Comeback
                                //return data;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Case_Get} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return new ProcessContext_Processes();
    }
    public static bool Case_Update(string authToken)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(120);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);

                        var casePacket = new CasePacket() { CaseAttributeVersionGUID = "todo. obtain." };

                        var request = new StringContent(casePacket.ToJson(), Encoding.UTF8, "application/json");

                        try
                        {
                            var response = client.PostAsync(Program.Enate_Case_Update, request).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                //ToDo. Dilip. Define return type.

                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Case_Update} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return false;
    }
    public static Work_GetMoreWork Work_GetMoreWork(string authToken)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(120);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        try
                        {
                            var response = client.GetAsync(Program.Enate_Work_GetMoreWork).GetAwaiter().GetResult();
                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                if (!jsonResponse.IsNullOrEmpty())
                                {
                                    Work_GetMoreWork data = jsonResponse.FromJson<Work_GetMoreWork>();
                                    return data;
                                }
                                else
                                    return null;
                                
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Work_GetMoreWork} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return new Work_GetMoreWork();
    }
    public static List<Contracts> Packet_GetContexts(string authToken, string processType, string caseflowPacketGUID, string customerGUID)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(120);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        try
                        {
                            var response = client.GetAsync($"{Program.Enate_Packet_GetContexts}?processType={processType}&caseflowPacketGUID={caseflowPacketGUID}&customerGUID={customerGUID}").GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                List<Contracts> data = jsonResponse.FromJson<List<Contracts>>();
                                return data;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Packet_GetContexts} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return new List<Contracts>();
    }
    public static bool Case_SetToDo(string authToken, CasePacket casePacket, string ProcessName)
    {
        bool setToDoSuccess = false;
        
        try
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                handler.UseDefaultCredentials = true;

                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(120);
                    client.BaseAddress = new Uri(Program.Enate_APIURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Add("apikey", authToken);
                    casePacket.DataFields = casePacket.Result.DataFields;
                    casePacket.Result.ContractName = casePacket.Result.DataFields.Region;
                    casePacket.Result.ServiceName = casePacket.Result.DataFields.ServiceLine;

                    string ContractName = casePacket.Result.DataFields.Region;
                    string ServiceName = casePacket.Result.DataFields.ServiceLine;
                    casePacket.Result.CheckProcess = ProcessName;

                    // var j = casePacket.Result.ToJson();
                    var request = new StringContent(casePacket.Result.ToJson(), Encoding.UTF8, "application/json");

                    try
                    {
                        var response = client.PostAsync(Program.Enate_Case_SetToDo, request).GetAwaiter().GetResult();                   
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            //CasePacket data = jsonResponse.FromJson<CasePacket>();
                            //Console.WriteLine("data .." + data.ToString());
                            //data.DataFields = casePacket.Result.DataFields;
                            //Console.WriteLine("data DataFields.." + data.DataFields.ToJson());
                            //data.Result.ContractName = ContractName;
                            //data.Result.ServiceName = ServiceName;
                            //return data;

                            setToDoSuccess= true;
                        }
                        else
                        {
                            Log.Information($"Failed enate call {Program.Enate_Case_SetToDo} API." + response);
                            Log.Information($"Failed enate call {Program.Enate_Case_SetToDo} API. Request content" + request.ReadAsStringAsync().Result);
                        }
                    }
                    catch (Exception ex)
                    {                        
                        Log.Information($"Failed enate call {Program.Enate_Case_SetToDo} API. Request content Message: {ex}" + request.ReadAsStringAsync().Result);
                        LoggerInfo.LogException(ex, " Case_SetToDo For CheckID " + casePacket.Title);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Action_SetInProgress} API. Message: {ex}");
            LoggerInfo.LogException(ex," Case_SetToDo For CheckID " + casePacket.Title);
        }
       
        // return new CasePacket();
        return setToDoSuccess;
    }
    public static bool OpenCertPolicy(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true; // allow all certificates
    }

    public static ActionAttribute Action_Get(string gUID, string authToken)
    {
        try
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                handler.UseDefaultCredentials = true;

                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(120);
                    client.BaseAddress = new Uri(Program.Enate_APIURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Add("apikey", authToken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    try
                    {
                        var response = client.GetAsync($"{Program.Enate_Action_Get}?actionGuid={gUID}").GetAwaiter().GetResult();

                        if (response.IsSuccessStatusCode)
                        {
                            var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            ActionAttribute data = jsonResponse.FromJson<ActionAttribute>();//Todo. Dilip. Comeback
                            return data;
                        } else {
                            Log.Error($"{Program.Enate_Action_Get} failed: " + response);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerInfo.LogException(ex);
                    }

                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }

        return null;
    }

    public static bool Action_SetInProgress (Work_GetMoreWork action, string authToken)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;
                    
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(120);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        
                        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        ActionPacket _setInProgress = new ActionPacket();
                        _setInProgress.GUID = action.GUID;
                        _setInProgress.Title = action.Title;
                        _setInProgress.AffectedRecordCount = 1;
                        
                        var request = new StringContent(JsonConvert.SerializeObject(_setInProgress), Encoding.UTF8, "application/json");

                        try
                        {
                            var response = client.PostAsync(Program.Enate_Action_SetInProgress, request).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            } else
                            {
                                Log.Error($"{Program.Enate_Action_SetInProgress} failed: " + response);
                                Log.Error($"{Program.Enate_Action_SetInProgress} failed request content: " + request.ReadAsStringAsync().Result);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Action_SetInProgress} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return false;
    }

    public static bool Action_ResolveSuccessfully(Work_GetMoreWork action, string authToken)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(120);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        ResolveActionPacket _setInProgress = new ResolveActionPacket();
                        _setInProgress.GUID = action.GUID;
                        _setInProgress.Title = action.Title;
                        _setInProgress.AffectedRecordCount = 1;

                        var request = new StringContent(JsonConvert.SerializeObject(_setInProgress), Encoding.UTF8, "application/json");

                        try
                        {
                            var response = client.PostAsync(Program.Enate_Action_ResolveSuccessfully, request).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            }
                            else
                            {
                                Log.Error($"{Program.Enate_Action_ResolveSuccessfully} failed: " + response);
                                Log.Error($"{Program.Enate_Action_ResolveSuccessfully} request content: " + request.ReadAsStringAsync().Result);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Action_ResolveSuccessfully} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return false;
    }

    public static bool Action_ResolveUnSuccessfully(Work_GetMoreWork action, string authToken, string error)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(120);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        ResolveActionPacket _setInProgress = new ResolveActionPacket();
                        _setInProgress.GUID = action.GUID;
                        _setInProgress.Title = action.Title;
                        _setInProgress.AffectedRecordCount = 1;
                        _setInProgress.NotDoneSuccessfullyComment = error;

                        var request = new StringContent(JsonConvert.SerializeObject(_setInProgress), Encoding.UTF8, "application/json");

                        try
                        {
                            var response = client.PostAsync(Program.Enate_Action_ResolveUnSuccessfully, request).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            }
                            else
                            {
                                Log.Error($"{Program.Enate_Action_ResolveUnSuccessfully} failed: " + response);
                                Log.Error($"{Program.Enate_Action_ResolveUnSuccessfully} request content: " + request.ReadAsStringAsync().Result);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Action_ResolveSuccessfully} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        return false;
    }

    public static bool ChangeUserPassword(string authToken, string userGUID, string NewPassword)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(120);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);
                        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        ChangeUserPasswordPacket _p = new ChangeUserPasswordPacket();
                        _p.NewPassword = NewPassword;

                        var request = new StringContent(JsonConvert.SerializeObject(_p), Encoding.UTF8, "application/json");

                        try
                        {
                            var response = client.PostAsync($"{Program.ReseteNatePassword}?userGUID={userGUID}", request).GetAwaiter().GetResult();
                            

                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.ReseteNatePassword} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
            return false;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public static bool Packet_Assign(UserAssignmentPacket packet, string authToken)
    {
        try
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    handler.UseDefaultCredentials = true;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(120);
                        client.BaseAddress = new Uri(Program.Enate_APIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("apikey", authToken);

                        var request = new StringContent(JsonConvert.SerializeObject(packet), Encoding.UTF8, "application/json");

                        try
                        {
                            var response = client.PostAsync($"{Program.Enate_Packet_Assign}", request).GetAwaiter().GetResult();


                            if (response.IsSuccessStatusCode)
                            {
                                //Log.Information($"{Program.Enate_Packet_Assign} failed: " + response);
                                return true;
                            }
                            else
                            {
                                Log.Error($"{Program.Enate_Packet_Assign} failed: " + response);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerInfo.LogException(ex);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error when attempting to call the Enate {Program.Enate_Packet_Assign} API. Message: {ex}");
                LoggerInfo.LogException(ex);
            }
            return false;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}

#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0017 // Simplify object initialization
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0290 // Use primary constructor
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore CS0168 // Variable is declared but never used
