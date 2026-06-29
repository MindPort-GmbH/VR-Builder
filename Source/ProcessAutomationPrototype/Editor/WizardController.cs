using System.Collections.Generic;

using VRBuilder.ProcessAutomationPrototype.Editor.Model;

using VRBuilder.ProcessAutomationPrototype.Editor.Questions;



namespace VRBuilder.ProcessAutomationPrototype.Editor

{

    /// <summary>

    /// Drives the guided question flow and fills a <see cref="ProcessBlueprint"/>.

    /// </summary>

    public class WizardController

    {

        private readonly MenuCatalog catalog;

        private readonly List<object> answers = new List<object>();



        private IEnumerator<IWizardQuestion> flow;

        private object lastAnswer;

        private int estimatedTotalQuestions;



        public ProcessBlueprint Blueprint { get; private set; }

        public IWizardQuestion Current { get; private set; }

        public bool IsFinished { get; private set; }

        public bool CanGoBack => answers.Count > 0;



        public int CurrentQuestionNumber => IsFinished ? estimatedTotalQuestions : answers.Count + 1;



        public int EstimatedTotalQuestions => estimatedTotalQuestions;



        public string ProgressText

        {

            get

            {

                if (IsFinished)

                {

                    return $"Completed ({estimatedTotalQuestions} questions)";

                }



                if (estimatedTotalQuestions > 0)

                {

                    return $"Question {CurrentQuestionNumber} of ~{estimatedTotalQuestions}";

                }



                return $"Question {CurrentQuestionNumber}";

            }

        }



        public WizardController(MenuCatalog catalog)

        {

            this.catalog = catalog;

        }



        public void Start()

        {

            answers.Clear();

            Rebuild(0);

        }



        /// <summary>Records the answer for the current question and advances to the next one.</summary>

        public void Submit(object answer)

        {

            answers.Add(answer);

            lastAnswer = answer;

            MoveNext();

        }



        /// <summary>Removes the most recent answer and returns it for pre-filling the previous question.</summary>

        public object Back()

        {

            object popped = answers[answers.Count - 1];

            answers.RemoveAt(answers.Count - 1);

            Rebuild(answers.Count);

            return popped;

        }



        private void Rebuild(int replayCount)

        {

            Blueprint = new ProcessBlueprint();

            estimatedTotalQuestions = SimulateTotalQuestionCount();

            flow = BuildFlow().GetEnumerator();

            IsFinished = false;

            Current = null;



            MoveNext();

            for (int i = 0; i < replayCount && IsFinished == false; i++)

            {

                lastAnswer = answers[i];

                MoveNext();

            }

        }



        private int SimulateTotalQuestionCount()

        {

            object savedLastAnswer = lastAnswer;

            List<object> simulatedAnswers = new List<object>(answers);

            Blueprint = new ProcessBlueprint();



            IEnumerator<IWizardQuestion> counter = BuildFlow().GetEnumerator();

            int count = 0;

            while (counter.MoveNext())

            {

                count++;

                if (simulatedAnswers.Count < count)

                {

                    simulatedAnswers.Add(counter.Current.SimulationDefault);

                }



                lastAnswer = simulatedAnswers[count - 1];

            }



            lastAnswer = savedLastAnswer;

            Blueprint = new ProcessBlueprint();

            return count;

        }



        private void MoveNext()

        {

            if (flow.MoveNext())

            {

                Current = flow.Current;

            }

            else

            {

                Current = null;

                IsFinished = true;

            }

        }



        private static int AsInt(object value, int fallback)

        {

            return value is int i ? i : fallback;

        }



        private static string AsString(object value, string fallback)

        {

            return value is string s ? s : fallback;

        }



        private IEnumerable<IWizardQuestion> BuildFlow()

        {

            yield return new TextQuestion("What should we call this process?", "New Process");

            Blueprint.ProcessName = AsString(lastAnswer, "New Process");



            yield return new IntQuestion("How many chapters do you want?", 1, 1);

            int chapterCount = AsInt(lastAnswer, 1);



            yield return new MenuPickQuestion("Should every chapter have the same number of steps?", "Choose", new[]

            {

                new MenuPickOption("Same number for every chapter", "same", "Same for every chapter"),

                new MenuPickOption("Different number per chapter", "different", "Different per chapter"),

            });

            bool sameForAll = (string)lastAnswer == "same";



            int[] stepsPerChapter = new int[chapterCount];

            if (sameForAll)

            {

                yield return new IntQuestion("How many steps should each chapter have?", 1, 1);

                int count = AsInt(lastAnswer, 1);

                for (int c = 0; c < chapterCount; c++)

                {

                    stepsPerChapter[c] = count;

                }

            }

            else

            {

                for (int c = 0; c < chapterCount; c++)

                {

                    yield return new IntQuestion($"How many steps in Chapter {c + 1}?", 1, 1);

                    stepsPerChapter[c] = AsInt(lastAnswer, 1);

                }

            }



            Blueprint.Chapters.Clear();

            for (int c = 0; c < chapterCount; c++)

            {

                ChapterBlueprint chapter = new ChapterBlueprint { Name = $"Chapter {c + 1}" };

                for (int s = 0; s < stepsPerChapter[c]; s++)

                {

                    chapter.Steps.Add(new StepBlueprint { Name = $"Step {s + 1}" });

                }



                Blueprint.Chapters.Add(chapter);

            }



            for (int c = 0; c < chapterCount; c++)

            {

                for (int s = 0; s < stepsPerChapter[c]; s++)

                {

                    StepBlueprint step = Blueprint.Chapters[c].Steps[s];

                    string where = $"Chapter {c + 1} · Step {s + 1}";



                    yield return new TextQuestion($"{where}: what's the name of this step?", step.Name);

                    step.Name = AsString(lastAnswer, step.Name);



                    yield return new IntQuestion($"{where}: how many behaviors does it have?", 0, 0);

                    int behaviorCount = AsInt(lastAnswer, 0);

                    step.BehaviorPaths.Clear();

                    for (int b = 0; b < behaviorCount; b++)

                    {

                        yield return new MenuPickQuestion($"{where}: pick behavior {b + 1} of {behaviorCount}.", "Select behavior", catalog.BehaviorOptions);

                        step.BehaviorPaths.Add((string)lastAnswer);

                    }



                    yield return new IntQuestion($"{where}: how many transitions (outgoing paths) does it have?", 0, 0);

                    int transitionCount = AsInt(lastAnswer, 0);

                    step.Transitions.Clear();

                    for (int t = 0; t < transitionCount; t++)

                    {

                        TransitionBlueprint transition = new TransitionBlueprint();

                        step.Transitions.Add(transition);



                        yield return new IntQuestion($"{where}: how many conditions does transition {t + 1} of {transitionCount} need?", 0, 0);

                        int conditionCount = AsInt(lastAnswer, 0);

                        for (int cd = 0; cd < conditionCount; cd++)

                        {

                            yield return new MenuPickQuestion($"{where}: transition {t + 1}, pick condition {cd + 1} of {conditionCount}.", "Select condition", catalog.ConditionOptions);

                            transition.ConditionPaths.Add((string)lastAnswer);

                        }

                    }



                    for (int t = 0; t < step.Transitions.Count; t++)

                    {

                        yield return new MenuPickQuestion($"{where}: where should transition {t + 1} lead?", "Select next step", BuildTargetOptions());

                        step.Transitions[t].Target = lastAnswer as StepRef?;

                    }

                }

            }

        }



        private IEnumerable<MenuPickOption> BuildTargetOptions()

        {

            List<MenuPickOption> options = new List<MenuPickOption>

            {

                new MenuPickOption("End of chapter (no next step)", null, "End of chapter"),

            };



            for (int c = 0; c < Blueprint.Chapters.Count; c++)

            {

                ChapterBlueprint chapter = Blueprint.Chapters[c];

                for (int s = 0; s < chapter.Steps.Count; s++)

                {

                    string stepName = chapter.Steps[s].Name;

                    string menuPath = $"Chapter {c + 1}/Step {s + 1}  {stepName}";

                    string label = $"Ch {c + 1} · Step {s + 1} ({stepName})";

                    options.Add(new MenuPickOption(menuPath, new StepRef(c, s), label));

                }

            }



            return options;

        }

    }

}


