using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.Networking;

namespace VRBuilder.ProcessAutomationPrototype.Editor.AI
{
    /// <summary>
    /// Client for the Anthropic Messages API.
    /// </summary>
    public static class AnthropicMessageClient
    {
        private const string Endpoint = "https://api.anthropic.com/v1/messages";
        private const string ApiVersion = "2023-06-01";

        /// <summary>
        /// Sends a user message with a system prompt and invokes the callback with the response text.
        /// </summary>
        public static void Send(string apiKey, string model, string systemPrompt, string userMessage, int maxTokens, Action<string> onSuccess, Action<string> onError)
        {
            string body = JsonConvert.SerializeObject(new
            {
                model,
                max_tokens = maxTokens,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userMessage },
                },
            });

            UnityWebRequest request = new UnityWebRequest(Endpoint, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer(),
            };
            request.SetRequestHeader("content-type", "application/json");
            request.SetRequestHeader("x-api-key", apiKey);
            request.SetRequestHeader("anthropic-version", ApiVersion);

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
                    JObject parsed = JObject.Parse(detail);
                    string message = parsed["error"]?["message"]?.ToString();
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
            JArray content = parsed["content"] as JArray;
            if (content == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            foreach (JToken block in content)
            {
                if ((block["type"]?.ToString()) == "text")
                {
                    builder.Append(block["text"]?.ToString());
                }
            }

            return builder.ToString();
        }
    }
}
