using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Utils;
using VRBuilder.Editor.PackageManager;

namespace VRBuilder.Editor.UI.Wizard
{
    /// <summary>
    /// Wizard page which prompts the user to download the XR Interaction Component
    /// </summary>
    internal class InteractionComponentPage : WizardPage
    {
        private const string xrInteractionComponentPackage = "co.mindport.builder.xrinteraction";

        [SerializeField]
        private bool installXRInteractionComponent = true;

        public InteractionComponentPage() : base("Interaction Component")
        {
        }

        public override void Draw(Rect window)
        {
            GUILayout.BeginArea(window);

            GUILayout.Label("Choose Interaction Component", BuilderEditorStyles.Title);

            if (ReflectionUtils.GetConcreteImplementationsOf<IInteractionComponentConfiguration>().Count() == 0)
            {
                HandleMissingInteractionComponent();
            }
            else
            {
                HandleMultipleInteractionComponents();
            }

            GUILayout.EndArea();
        }

        private void HandleMissingInteractionComponent()
        {
            GUILayout.Label("Missing Interaction Component", BuilderEditorStyles.Header);

            GUILayout.Label("No interaction component has been found in the project. You can install the default XR Interaction component (based on Unity's XR Interaction Toolkit 1.0.0pre2) and VR Builder will be ready for use. If you want to install another Interaction Component on your own, please skip for now.", BuilderEditorStyles.Paragraph);

            GUILayout.Space(16);

            if (GUILayout.Toggle(installXRInteractionComponent, "Install default XR Interaction Component and restart the wizard.", BuilderEditorStyles.RadioButton))
            {
                installXRInteractionComponent = true;
                ShouldRestart = true;
            }

            if (GUILayout.Toggle(!installXRInteractionComponent, "Skip for now. I will install a different interaction component.", BuilderEditorStyles.RadioButton))
            {
                installXRInteractionComponent = false;
                ShouldRestart = false;

                EditorGUILayout.HelpBox("VR Builder will not work properly until an interaction component is installed.", MessageType.Warning);
            }

            GUILayout.Space(16);

            GUILayout.Label("More interaction components, such as integrations with our partners, are avaliable on our Add-ons and Integrations page.", BuilderEditorStyles.Paragraph);
            BuilderGUILayout.DrawLink("Add-ons and Integrations", "https://www.mindport.co/vr-builder-add-ons-and-integrations", BuilderEditorStyles.Indent);

            GUILayout.Space(16);

            GUILayout.Label("Here you can find comprehensive guides on how to install non-default interaction components.", BuilderEditorStyles.Paragraph);
            BuilderGUILayout.DrawLink("How to setup VR Builder with Interhaptics VR Interaction Essentials", "https://www.mindport.co/vr-builder-learning-path/interhaptics-integration", BuilderEditorStyles.Indent);
        }

        private void HandleMultipleInteractionComponents()
        {
            installXRInteractionComponent = false;
            GUILayout.Label("Multiple Interaction Components", BuilderEditorStyles.Header);

            GUILayout.Label("The following interaction components have been found in the project.", BuilderEditorStyles.Paragraph);
            GUILayout.Space(16);

            IEnumerable<Type> interactionComponents = ReflectionUtils.GetConcreteImplementationsOf<IInteractionComponentConfiguration>();

            foreach(Type type in interactionComponents)
            {
                IInteractionComponentConfiguration configuration = ReflectionUtils.CreateInstanceOfType(type) as IInteractionComponentConfiguration;
                GUILayout.Label("- " + configuration.DisplayName, BuilderEditorStyles.Paragraph);
            }

            GUILayout.Space(16);
            EditorGUILayout.HelpBox("More than one interaction component may cause issues, please ensure only one is present in a given project.", MessageType.Warning);
        }

        public override void Apply()
        {
            base.Apply();

            if (installXRInteractionComponent)
            {
                EditorUtility.DisplayDialog($"Enabling XR Interaction Component", "Wait until the setup is done.\n\nIMPORTANT: XR Interaction Toolkit will ask to automatically update the interaction layer, should you decide to do so, manual fixes will be required for teleporting to work. ", "Continue");
                PackageOperationsManager.LoadPackage(xrInteractionComponentPackage);
            }
        }
    }
}
