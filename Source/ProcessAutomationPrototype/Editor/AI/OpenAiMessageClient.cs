using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.Networking;

namespace VRBuilder.ProcessAutomationPrototype.Editor.AI
{
    /// <summary>
    /// Client for OpenAI-compatible chat-completions endpoints.
    /// </summary>
    public static class OpenAiMessageClient
    {
        public static void Send(string baseUrl, string apiKey, string model, string systemPrompt, string userMessage, int maxTokens, Action<string> onSuccess, Action<string> onError, bool useLegacyMaxTokens = false)
        {
            string url = baseUrl.TrimEnd('/') + "/chat/completions";

            JObject requestBody = JObject.FromObject(new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage },
                },
            });
            requestBody[useLegacyMaxTokens ? "max_tokens" : "max_completion_tokens"] = maxTokens;
            string body = requestBody.ToString(Formatting.None);

            UnityWebRequest request = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer(),
            };
            request.SetRequestHeader("content-type", "application/json");
            if (string.IsNullOrWhiteSpace(apiKey) == false)
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            }

            request.SendWebRequest();

            void Poll()
            {
                if (request.isDone == false)
                {
                    return;
                }

                EditorApplication.update -= Poll;

                try
                {
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        onError?.Invoke(DescribeError(request));
                        return;
                    }

                    onSuccess?.Invoke(ExtractText(request.downloadHandler.text));
                }
                catch (Exception exception)
                {
                    onError?.Invoke(exception.Message);
                }
                finally
                {
                    request.Dispose();
                }
            }

            EditorApplication.update += Poll;
        }

        private static string DescribeError(UnityWebRequest request)
        {
            string detail = request.downloadHandler?.text;
            if (string.IsNullOrEmpty(detail) == false)
            {
                try
                {
                    string message = JObject.Parse(detail)["error"]?["message"]?.ToString();
                    if (string.IsNullOrEmpty(message) == false)
                    {
                        return $"{request.responseCode}: {message}";
                    }
                }
                catch (JsonException)
                {
                }

                return $"{request.responseCode}: {detail}";
            }

            return $"{request.responseCode}: {request.error}";
        }

        private static string ExtractText(string responseJson)
        {
            JObject parsed = JObject.Parse(responseJson);
            return parsed["choices"]?[0]?["message"]?["content"]?.ToString() ?? string.Empty;
        }
    }
}
