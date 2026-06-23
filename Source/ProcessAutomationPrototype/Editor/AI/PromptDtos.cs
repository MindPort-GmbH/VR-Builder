using System.Collections.Generic;

namespace VRBuilder.ProcessAutomationPrototype.Editor.AI
{
    /// <summary>JSON shape returned by the language model.</summary>
    public class PromptProcessDto
    {
        public string processName;
        public List<PromptChapterDto> chapters;
    }

    public class PromptChapterDto
    {
        public string name;
        public List<PromptStepDto> steps;
    }

    public class PromptStepDto
    {
        public string name;
        public List<string> behaviors;
        public List<PromptTransitionDto> transitions;
    }

    public class PromptTransitionDto
    {
        public List<string> conditions;
        public PromptTargetDto target;
    }

    public class PromptTargetDto
    {
        public int chapter;
        public int step;
    }
}
