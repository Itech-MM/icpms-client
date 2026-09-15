using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using icpms_client.Network.Constants;
using icpms_client.Network.Response;
using log4net;
using Newtonsoft.Json;

namespace icpms_client.Network.Core;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILog _log = LogManager.GetLogger(typeof(ApiClient));

    public ApiClient(IApiConstant apiConstant)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(apiConstant.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(90);
    }

    private void ApplyRequestHeaders(HttpRequestMessage request, bool requiresAuth, string token)
    {
        if (requiresAuth && !string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var localIp = GetLocalIpAddress();
        request.Headers.Remove("gate-ip");
        request.Headers.Add("gate-ip", localIp);
    }

    private string GetLocalIpAddress()
    {
        try
        {
            return "127.0.0.1";
        }
        catch (Exception ex)
        {
            _log.Error("Failed to resolve local IP address for gate-ip header", ex);
            return "127.0.0.1";
        }
    }

    public async Task<Response.Response?> GetAsync<T>(string endpoint, bool requiresAuth = true, string token = "")
    {
        _log.Debug($"Sending GET request to: {endpoint}");

        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        ApplyRequestHeaders(request, requiresAuth, token);

        var response = await _httpClient.SendAsync(request);
        return await HandleResponse<T>(response);
    }

    public async Task<Response.Response?> GetLongPollingAsync<T>(
        string endpoint,
        TimeSpan? timeout = null,
        bool requiresAuth = true,
        string token = "",
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource();
        var finalTimeout = timeout ?? TimeSpan.FromSeconds(30);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCts.Token
        );

        try
        {
            _log.Debug($"Starting long polling request to: {endpoint}");
            timeoutCts.CancelAfter(finalTimeout);

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            ApplyRequestHeaders(request, requiresAuth, token);

            var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                linkedCts.Token
            );

            _log.Debug($"Long polling response received: {response.StatusCode}");

            return await HandleResponse<T>(response);
        }
        catch (TaskCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            _log.Debug("Long polling request timed out");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = "Request timeout",
                Data = "No status change within timeout period"
            };
        }
        catch (Exception ex)
        {
            _log.Error("Long polling request failed", ex);
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = "Long polling failed",
                Data = ex.Message
            };
        }
    }

    public async Task<Response.Response?> PostAsync<T>(string endpoint, object? data = null, bool requiresAuth = true, string token = "")
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        ApplyRequestHeaders(request, requiresAuth, token);

        request.Content = data != null ? CreateJsonContent(data) : null;
        _log.Debug($"POST request to: {endpoint}, Body: {(request.Content != null ? await request.Content.ReadAsStringAsync() : "EMPTY")}");

        var response = await _httpClient.SendAsync(request);
        return await HandleResponse<T>(response);
    }

    public async Task<Response.Response?> PutAsync<T>(string endpoint, object? data = null, bool requiresAuth = true, string token = "")
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
        ApplyRequestHeaders(request, requiresAuth, token);

        request.Content = data != null ? CreateJsonContent(data) : null;
        _log.Debug($"PUT request to: {endpoint}, Body: {(request.Content != null ? await request.Content.ReadAsStringAsync() : "EMPTY")}");

        var response = await _httpClient.SendAsync(request);
        return await HandleResponse<T>(response);
    }
    
    public async Task<Response.Response?> PatchAsync<T>(string endpoint, object? data = null, bool requiresAuth = true, string token = "")
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
        ApplyRequestHeaders(request, requiresAuth, token);

        request.Content = data != null ? CreateJsonContent(data) : null;
        _log.Debug($"PATCH request to: {endpoint}, Body: {(request.Content != null ? await request.Content.ReadAsStringAsync() : "EMPTY")}");

        var response = await _httpClient.SendAsync(request);
        return await HandleResponse<T>(response);
    }

    public async Task<Response.Response?> DeleteAsync<T>(string endpoint, bool requiresAuth = true, string token = "")
    {
        _log.Debug($"DELETE request to: {endpoint}");

        using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        ApplyRequestHeaders(request, requiresAuth, token);

        var response = await _httpClient.SendAsync(request);
        return await HandleResponse<T>(response);
    }

    private async Task<Response.Response?> HandleResponse<T>(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        _log.Debug($"Response: {responseContent}");

        return response.IsSuccessStatusCode
            ? JsonConvert.DeserializeObject<BaseResponse<T>>(responseContent)
            : JsonConvert.DeserializeObject<BaseErrorResponse<string>>(responseContent);
    }

    private StringContent CreateJsonContent(object? data)
    {
        return new StringContent(
            data != null ? JsonConvert.SerializeObject(data) : string.Empty,
            Encoding.UTF8,
            "application/json"
        );
    }
}