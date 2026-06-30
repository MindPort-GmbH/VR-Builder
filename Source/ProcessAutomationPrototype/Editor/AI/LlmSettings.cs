using System;
using UnityEditor;

namespace VRBuilder.ProcessAutomationPrototype.Editor.AI
{
    /// <summary>Engine used to generate a process from a text prompt.</summary>
    public enum LlmEngine
    {
        /// <summary>Offline keyword-based generation without AI.</summary>
        Offline = 0,

        /// <summary>Anthropic Claude (Messages API).</summary>
        Anthropic = 1,

        /// <summary>OpenAI chat-completions endpoint.</summary>
        OpenAi = 2,

        /// <summary>Custom OpenAI-compatible chat-completions endpoint.</summary>
        CustomApi = 3,
    }

    /// <summary>
    /// Prompt-mode settings persisted in <see cref="EditorPrefs"/>.
    /// </summary>
    public class LlmSettings
    {
        private const string Prefix = "VRBuilder.ProcessAutomationPrototype.";

        public static readonly string[] AnthropicModels =
        {
            "claude-fable-5",
            "claude-opus-4-8",
            "claude-opus-4-7",
            "claude-opus-4-6",
            "claude-opus-4-5",
            "claude-sonnet-4-6",
            "claude-sonnet-4-5",
            "claude-haiku-4-5",
        };

        public static readonly string[] OpenAiModels =
        {
            "gpt-5.6-sol",
            "gpt-5.6-terra",
            "gpt-5.6-luna",
            "gpt-5.5",
            "gpt-5.5-pro",
            "gpt-5.4",
            "gpt-5.4-pro",
            "gpt-5.4-mini",
            "gpt-5.4-nano",
            "gpt-5.3-codex",
            "gpt-5.2-codex",
            "gpt-5.1-codex",
            "gpt-5.1-codex-max",
            "gpt-5.1-codex-mini",
            "gpt-5-codex",
            "gpt-5.2",
            "gpt-5.2-pro",
            "gpt-5.1",
            "gpt-5",
            "gpt-5-pro",
            "gpt-5-mini",
            "gpt-5-nano",
            "gpt-5-chat-latest",
            "gpt-4.1",
            "gpt-4.1-mini",
            "gpt-4.1-nano",
            "gpt-4o",
            "gpt-4o-mini",
            "o4-mini",
            "o3",
            "o3-pro",
            "o3-mini",
        };

        public const string DefaultAnthropicModel = "claude-opus-4-8";
        public const string DefaultOpenAiBaseUrl = "https://api.openai.com/v1";
        public const string DefaultOpenAiModel = "gpt-4o";
        public const string DefaultCustomApiBaseUrl = "http://localhost:11434/v1";
        public const string DefaultCustomApiModel = "llama3.1";

        public LlmEngine Engine;
        public string AnthropicKey;
        public string AnthropicModel;
        public string OpenAiModel;
        public string OpenAiKey;
        public string CustomApiBaseUrl;
        public string CustomApiModel;
        public string CustomApiKey;

        public static LlmSettings Load()
        {
            return new LlmSettings
            {
                Engine = (LlmEngine)EditorPrefs.GetInt(Prefix + "Engine", (int)LlmEngine.Offline),
                AnthropicKey = EditorPrefs.GetString(Prefix + "AnthropicKey", string.Empty),
                AnthropicModel = EditorPrefs.GetString(Prefix + "AnthropicModel", DefaultAnthropicModel),
                OpenAiModel = EditorPrefs.GetString(Prefix + "OpenAiModel", DefaultOpenAiModel),
                OpenAiKey = EditorPrefs.GetString(Prefix + "OpenAiKey", string.Empty),
                CustomApiBaseUrl = EditorPrefs.GetString(Prefix + "CustomApiBaseUrl", DefaultCustomApiBaseUrl),
                CustomApiModel = EditorPrefs.GetString(Prefix + "CustomApiModel", DefaultCustomApiModel),
                CustomApiKey = EditorPrefs.GetString(Prefix + "CustomApiKey", string.Empty),
            };
        }

        public void Save()
        {
            EditorPrefs.SetInt(Prefix + "Engine", (int)Engine);
            EditorPrefs.SetString(Prefix + "AnthropicKey", AnthropicKey ?? string.Empty);
            EditorPrefs.SetString(Prefix + "AnthropicModel", string.IsNullOrWhiteSpace(AnthropicModel) ? DefaultAnthropicModel : AnthropicModel.Trim());
            EditorPrefs.SetString(Prefix + "OpenAiModel", string.IsNullOrWhiteSpace(OpenAiModel) ? DefaultOpenAiModel : OpenAiModel.Trim());
            EditorPrefs.SetString(Prefix + "OpenAiKey", OpenAiKey ?? string.Empty);
            EditorPrefs.SetString(Prefix + "CustomApiBaseUrl", string.IsNullOrWhiteSpace(CustomApiBaseUrl) ? DefaultCustomApiBaseUrl : CustomApiBaseUrl.Trim());
            EditorPrefs.SetString(Prefix + "CustomApiModel", string.IsNullOrWhiteSpace(CustomApiModel) ? DefaultCustomApiModel : CustomApiModel.Trim());
            EditorPrefs.SetString(Prefix + "CustomApiKey", CustomApiKey ?? string.Empty);
        }

        public string EffectiveAnthropicKey()
        {
            return Fallback(AnthropicKey, "ANTHROPIC_API_KEY");
        }

        public string EffectiveOpenAiKey()
        {
            return Fallback(OpenAiKey, "OPENAI_API_KEY");
        }

        public string AnthropicModelOrDefault()
        {
            return string.IsNullOrWhiteSpace(AnthropicModel) ? DefaultAnthropicModel : AnthropicModel.Trim();
        }

        public string OpenAiModelOrDefault()
        {
            return string.IsNullOrWhiteSpace(OpenAiModel) ? DefaultOpenAiModel : OpenAiModel.Trim();
        }

        public string CustomApiBaseUrlOrDefault()
        {
            return string.IsNullOrWhiteSpace(CustomApiBaseUrl) ? DefaultCustomApiBaseUrl : CustomApiBaseUrl.Trim();
        }

        public string CustomApiModelOrDefault()
        {
            return string.IsNullOrWhiteSpace(CustomApiModel) ? DefaultCustomApiModel : CustomApiModel.Trim();
        }

        public string EffectiveCustomApiKey()
        {
            return Fallback(CustomApiKey, "OPENAI_API_KEY");
        }

        private static string Fallback(string value, string envVar)
        {
            if (string.IsNullOrWhiteSpace(value) == false)
            {
                return value.Trim();
            }

            return Environment.GetEnvironmentVariable(envVar) ?? string.Empty;
        }
    }
}
