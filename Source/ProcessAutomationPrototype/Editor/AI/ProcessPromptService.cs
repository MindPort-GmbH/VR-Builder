using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using VRBuilder.ProcessAutomationPrototype.Editor.Model;

namespace VRBuilder.ProcessAutomationPrototype.Editor.AI
{
    /// <summary>
    /// Generates a <see cref="ProcessBlueprint"/> from a text prompt using the configured engine.
    /// </summary>
    public static class ProcessPromptService
    {
        private const int MaxTokens = 8000;

        public static void Generate(LlmSettings settings, MenuCatalog catalog, string userRequest, Action<ProcessBlueprint> onDone, Action<string> onError)
        {
            switch (settings.Engine)
            {
                case LlmEngine.None:
                    try
                    {
                        ProcessBlueprint blueprint = HeuristicProcessGenerator.Generate(catalog, userRequest);
                        onDone?.Invoke(blueprint);
                    }
                    catch (Exception exception)
                    {
                        onError?.Invoke(exception.Message);
                    }

                    break;

                case LlmEngine.Anthropic:
                    string anthropicKey = settings.EffectiveAnthropicKey();
                    if (string.IsNullOrEmpty(anthropicKey))
                    {
                        onError?.Invoke("Add your Anthropic API key, or switch the engine to Offline.");
                        return;
                    }

                    AnthropicMessageClient.Send(anthropicKey, settings.AnthropicModelOrDefault(), BuildSystemPrompt(catalog), userRequest, MaxTokens,
                        text => HandleLlmText(text, catalog, onDone, onError), onError);
                    break;

                case LlmEngine.OpenAi:
                    string openAiKey = settings.EffectiveOpenAiKey();
                    if (string.IsNullOrEmpty(openAiKey))
                    {
                        onError?.Invoke("Add your OpenAI API key, or switch the engine to Offline.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(settings.OpenAiModel))
                    {
                        onError?.Invoke("Set a model name (for example gpt-4o).");
                        return;
                    }

                    OpenAiMessageClient.Send(settings.OpenAiBaseUrlOrDefault(), openAiKey, settings.OpenAiModel.Trim(), BuildSystemPrompt(catalog), userRequest, MaxTokens,
                        text => HandleLlmText(text, catalog, onDone, onError), onError);
                    break;
            }
        }

        private static void HandleLlmText(string text, MenuCatalog catalog, Action<ProcessBlueprint> onDone, Action<string> onError)
        {
            try
            {
                ProcessBlueprint blueprint = Parse(text, catalog);
                if (blueprint.Chapters.Count == 0)
                {
                    onError?.Invoke("The model didn't return any chapters. Try describing the process in more detail.");
                    return;
                }

                onDone?.Invoke(blueprint);
            }
            catch (Exception exception)
            {
                onError?.Invoke($"Couldn't read the generated process: {exception.Message}");
            }
        }

        private static string BuildSystemPrompt(MenuCatalog catalog)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("You design training processes for VR Builder. Given the user's description, output a single JSON object describing the process.");
            builder.AppendLine("Output ONLY the JSON object — no markdown fences, no commentary.");
            builder.AppendLine();
            builder.AppendLine("JSON shape:");
            builder.AppendLine("{");
            builder.AppendLine("  \"processName\": string,");
            builder.AppendLine("  \"chapters\": [");
            builder.AppendLine("    {");
            builder.AppendLine("      \"name\": string,");
            builder.AppendLine("      \"steps\": [");
            builder.AppendLine("        {");
            builder.AppendLine("          \"name\": string,");
            builder.AppendLine("          \"behaviors\": [string],");
            builder.AppendLine("          \"transitions\": [");
            builder.AppendLine("            { \"conditions\": [string], \"target\": { \"chapter\": int, \"step\": int } }");
            builder.AppendLine("          ]");
            builder.AppendLine("        }");
            builder.AppendLine("      ]");
            builder.AppendLine("    }");
            builder.AppendLine("  ]");
            builder.AppendLine("}");
            builder.AppendLine();
            builder.AppendLine("Rules:");
            builder.AppendLine("- Each entry in \"behaviors\" MUST be exactly one of the allowed behavior names listed below.");
            builder.AppendLine("- Each entry in \"conditions\" MUST be exactly one of the allowed condition names listed below.");
            builder.AppendLine("- \"target\" uses 1-based \"chapter\" and \"step\" numbers for the step the transition leads to. Omit \"target\" (or set it to null) to end the chapter. A step usually has one transition to the next step.");
            builder.AppendLine("- Choose only the TYPES of behaviors and conditions. Scene objects are assigned by the user later.");
            builder.AppendLine("- Keep it reasonable: 1-8 chapters, a few steps each. Give every step a short, clear name.");
            builder.AppendLine();
            builder.AppendLine("Allowed behaviors:");
            foreach (string path in catalog.BehaviorPaths.OrderBy(p => p))
            {
                builder.Append("- ").AppendLine(path);
            }

            builder.AppendLine();
            builder.AppendLine("Allowed conditions:");
            foreach (string path in catalog.ConditionPaths.OrderBy(p => p))
            {
                builder.Append("- ").AppendLine(path);
            }

            return builder.ToString();
        }

        private static ProcessBlueprint Parse(string responseText, MenuCatalog catalog)
        {
            string json = ExtractJsonObject(responseText);
            PromptProcessDto dto = JsonConvert.DeserializeObject<PromptProcessDto>(json);
            if (dto == null)
            {
                throw new Exception("empty response");
            }

            ProcessBlueprint blueprint = new ProcessBlueprint
            {
                ProcessName = string.IsNullOrWhiteSpace(dto.processName) ? "AI Process" : dto.processName.Trim(),
            };

            List<PromptChapterDto> chapters = dto.chapters ?? new List<PromptChapterDto>();
            for (int c = 0; c < chapters.Count; c++)
            {
                PromptChapterDto chapterDto = chapters[c];
                ChapterBlueprint chapter = new ChapterBlueprint
                {
                    Name = string.IsNullOrWhiteSpace(chapterDto?.name) ? $"Chapter {c + 1}" : chapterDto.name.Trim(),
                };

                List<PromptStepDto> steps = chapterDto?.steps ?? new List<PromptStepDto>();
                for (int s = 0; s < steps.Count; s++)
                {
                    chapter.Steps.Add(MapStep(steps[s], s, catalog));
                }

                blueprint.Chapters.Add(chapter);
            }

            return blueprint;
        }

        private static StepBlueprint MapStep(PromptStepDto stepDto, int index, MenuCatalog catalog)
        {
            StepBlueprint step = new StepBlueprint
            {
                Name = string.IsNullOrWhiteSpace(stepDto?.name) ? $"Step {index + 1}" : stepDto.name.Trim(),
            };

            if (stepDto?.behaviors != null)
            {
                foreach (string behavior in stepDto.behaviors)
                {
                    string resolved = catalog.ResolveBehavior(behavior);
                    if (resolved != null)
                    {
                        step.BehaviorPaths.Add(resolved);
                    }
                }
            }

            if (stepDto?.transitions != null)
            {
                foreach (PromptTransitionDto transitionDto in stepDto.transitions)
                {
                    TransitionBlueprint transition = new TransitionBlueprint();

                    if (transitionDto?.conditions != null)
                    {
                        foreach (string condition in transitionDto.conditions)
                        {
                            string resolved = catalog.ResolveCondition(condition);
                            if (resolved != null)
                            {
                                transition.ConditionPaths.Add(resolved);
                            }
                        }
                    }

                    if (transitionDto?.target != null && transitionDto.target.chapter > 0 && transitionDto.target.step > 0)
                    {
                        transition.Target = new StepRef(transitionDto.target.chapter - 1, transitionDto.target.step - 1);
                    }

                    step.Transitions.Add(transition);
                }
            }

            return step;
        }

        private static string ExtractJsonObject(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new Exception("empty response");
            }

            int start = text.IndexOf('{');
            int end = text.LastIndexOf('}');
            if (start < 0 || end <= start)
            {
                throw new Exception("no JSON object found");
            }

            return text.Substring(start, end - start + 1);
        }
    }
}
