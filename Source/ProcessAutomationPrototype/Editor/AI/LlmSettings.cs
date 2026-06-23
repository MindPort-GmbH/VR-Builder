using System;
using UnityEditor;

namespace VRBuilder.ProcessAutomationPrototype.Editor.AI
{
    /// <summary>Engine used to generate a process from a text prompt.</summary>
    public enum LlmEngine
    {
        /// <summary>Offline keyword-based generation.</summary>
        None = 0,

        /// <summary>Anthropic Claude (Messages API).</summary>
        Anthropic = 1,

        /// <summary>OpenAI-compatible chat-completions endpoint.</summary>
        OpenAi = 2,
    }

    /// <summary>
    /// Prompt-mode settings persisted in <see cref="EditorPrefs"/>.
    /// </summary>
    public class LlmSettings
    {
        private const string Prefix = "VRBuilder.ProcessAutomationPrototype.";

        public const string DefaultAnthropicModel = "claude-opus-4-8";
        public const string DefaultOpenAiBaseUrl = "https://api.openai.com/v1";
        public const string DefaultOpenAiModel = "gpt-4o";

        public LlmEngine Engine;
        public string AnthropicKey;
        public string AnthropicModel;
        public string OpenAiBaseUrl;
        public string OpenAiModel;
        public string OpenAiKey;

        public static LlmSettings Load()
        {
            return new LlmSettings
            {
                Engine = (LlmEngine)EditorPrefs.GetInt(Prefix + "Engine", (int)LlmEngine.None),
                AnthropicKey = EditorPrefs.GetString(Prefix + "AnthropicKey", string.Empty),
                AnthropicModel = EditorPrefs.GetString(Prefix + "AnthropicModel", DefaultAnthropicModel),
                OpenAiBaseUrl = EditorPrefs.GetString(Prefix + "OpenAiBaseUrl", DefaultOpenAiBaseUrl),
                OpenAiModel = EditorPrefs.GetString(Prefix + "OpenAiModel", DefaultOpenAiModel),
                OpenAiKey = EditorPrefs.GetString(Prefix + "OpenAiKey", string.Empty),
            };
        }

        public void Save()
        {
            EditorPrefs.SetInt(Prefix + "Engine", (int)Engine);
            EditorPrefs.SetString(Prefix + "AnthropicKey", AnthropicKey ?? string.Empty);
            EditorPrefs.SetString(Prefix + "AnthropicModel", string.IsNullOrWhiteSpace(AnthropicModel) ? DefaultAnthropicModel : AnthropicModel.Trim());
            EditorPrefs.SetString(Prefix + "OpenAiBaseUrl", string.IsNullOrWhiteSpace(OpenAiBaseUrl) ? DefaultOpenAiBaseUrl : OpenAiBaseUrl.Trim());
            EditorPrefs.SetString(Prefix + "OpenAiModel", string.IsNullOrWhiteSpace(OpenAiModel) ? DefaultOpenAiModel : OpenAiModel.Trim());
            EditorPrefs.SetString(Prefix + "OpenAiKey", OpenAiKey ?? string.Empty);
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

        public string OpenAiBaseUrlOrDefault()
        {
            return string.IsNullOrWhiteSpace(OpenAiBaseUrl) ? DefaultOpenAiBaseUrl : OpenAiBaseUrl.Trim();
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
