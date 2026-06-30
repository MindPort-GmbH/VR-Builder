using System;
using System.Text.RegularExpressions;

namespace VRBuilder.ProcessAutomationPrototype.Editor.AI
{
    /// <summary>
    /// Turns raw provider API errors into wizard-friendly chat messages.
    /// </summary>
    internal static class LlmApiErrorFormatter
    {
        public static string Format(LlmSettings settings, string rawError)
        {
            if (string.IsNullOrWhiteSpace(rawError))
            {
                return $"The request to {EngineLabel(settings)} failed. Try again or pick another model.";
            }

            string model = SelectedModel(settings);
            string lower = rawError.ToLowerInvariant();
            string apiMessage = ExtractApiMessage(rawError);

            if (IsInvalidApiKey(lower))
            {
                return $"Invalid API key for {EngineLabel(settings)}. Check the key in the settings above."
                    + AppendDetail(apiMessage);
            }

            if (IsQuotaOrBillingIssue(lower))
            {
                return $"Your {EngineLabel(settings)} account has a billing or quota issue."
                    + AppendDetail(apiMessage);
            }

            if (IsRateLimit(lower))
            {
                return $"Rate limit reached for {EngineLabel(settings)}. Wait a moment and try again."
                    + AppendDetail(apiMessage);
            }

            if (IsModelAccessIssue(lower))
            {
                return $"Your API key can't use \"{model}\" on {EngineLabel(settings)}."
                    + AppendDetail(apiMessage)
                    + SuggestFallbackModel(settings);
            }

            if (string.IsNullOrWhiteSpace(model) == false)
            {
                return $"Request to \"{model}\" failed: {rawError}";
            }

            return rawError;
        }

        private static string SelectedModel(LlmSettings settings)
        {
            switch (settings.Engine)
            {
                case LlmEngine.Anthropic:
                    return settings.AnthropicModelOrDefault();
                case LlmEngine.OpenAi:
                    return settings.OpenAiModelOrDefault();
                case LlmEngine.CustomApi:
                    return settings.CustomApiModelOrDefault();
                default:
                    return string.Empty;
            }
        }

        private static string EngineLabel(LlmSettings settings)
        {
            switch (settings.Engine)
            {
                case LlmEngine.Anthropic:
                    return "Claude (Anthropic)";
                case LlmEngine.OpenAi:
                    return "OpenAI";
                case LlmEngine.CustomApi:
                    return "your custom API";
                default:
                    return "the selected engine";
            }
        }

        private static string SuggestFallbackModel(LlmSettings settings)
        {
            switch (settings.Engine)
            {
                case LlmEngine.Anthropic:
                    return "\n\nTry a model your plan includes, such as claude-haiku-4-5 or claude-sonnet-4-6, using the Model dropdown above.";
                case LlmEngine.OpenAi:
                    return "\n\nTry a model your plan includes, such as gpt-4o-mini or gpt-4.1-mini, using the Model dropdown above.";
                case LlmEngine.CustomApi:
                    return "\n\nCheck that the model name and base URL match what your server exposes.";
                default:
                    return string.Empty;
            }
        }

        private static bool IsModelAccessIssue(string lower)
        {
            return lower.Contains("model_not_found")
                || lower.Contains("does not exist")
                || lower.Contains("model:")
                || lower.Contains("not_found_error")
                || lower.Contains("do not have access")
                || lower.Contains("must be verified")
                || lower.Contains("is not available")
                || lower.Contains("no access")
                || lower.Contains("access denied")
                || lower.Contains("not allowed to use")
                || lower.Contains("unsupported model")
                || lower.Contains("unknown model");
        }

        private static bool IsInvalidApiKey(string lower)
        {
            return lower.Contains("invalid_api_key")
                || lower.Contains("incorrect api key")
                || lower.Contains("authentication_error")
                || lower.Contains("invalid x-api-key")
                || lower.Contains("invalid authentication")
                || (lower.Contains("401") && lower.Contains("api key"));
        }

        private static bool IsQuotaOrBillingIssue(string lower)
        {
            return lower.Contains("insufficient_quota")
                || lower.Contains("exceeded your current quota")
                || lower.Contains("credit balance")
                || lower.Contains("billing")
                || lower.Contains("payment required");
        }

        private static bool IsRateLimit(string lower)
        {
            return lower.Contains("rate_limit")
                || lower.Contains("rate limit")
                || lower.Contains("too many requests");
        }

        private static string ExtractApiMessage(string rawError)
        {
            Match match = Regex.Match(rawError, @"(?<=\d{3}:\s).+");
            if (match.Success)
            {
                return match.Value.Trim();
            }

            return rawError.Trim();
        }

        private static string AppendDetail(string apiMessage)
        {
            if (string.IsNullOrWhiteSpace(apiMessage))
            {
                return string.Empty;
            }

            return $"\n\nProvider message: {apiMessage}";
        }
    }
}
