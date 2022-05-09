// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using VRBuilder.Editor.Analytics;
using UnityEngine;

namespace VRBuilder.Editor.UI.Wizard
{
    internal class AllAboutPage : WizardPage
    {
        public AllAboutPage() : base("Help & Documentation", false ,false )
        {

        }

        public override void Draw(Rect window)
        {
            GUILayout.BeginArea(window);
                GUILayout.Label("Hit Play to Preview", BuilderEditorStyles.Title);
                GUILayout.Label("Have a look at the following resources for further information.", BuilderEditorStyles.Paragraph);
                GUILayout.Label("Help", BuilderEditorStyles.Header);

                BuilderGUILayout.DrawLink("Documentation", "http://documentation.mindport.co/", BuilderEditorStyles.IndentLarge);
                BuilderGUILayout.DrawLink("Tutorials", "https://www.mindport.co/vr-builder/tutorials", BuilderEditorStyles.IndentLarge);
                BuilderGUILayout.DrawLink("Roadmap", "https://www.mindport.co/vr-builder/roadmap", BuilderEditorStyles.IndentLarge);

                GUILayout.Label("Community", BuilderEditorStyles.Header);

                BuilderGUILayout.DrawLink("Community", "http://community.mindport.co", BuilderEditorStyles.IndentLarge);
                BuilderGUILayout.DrawLink("Contact us", "mailto:info@mindport.co", BuilderEditorStyles.IndentLarge);

                GUILayout.Label("Review", BuilderEditorStyles.Header);
                GUILayout.Label("If you like what we are doing, you can help us greatly by leaving a positive review on the Unity Asset Store!", BuilderEditorStyles.Paragraph);

                BuilderGUILayout.DrawLink("Leave a review", "https://assetstore.unity.com/packages/tools/visual-scripting/vr-builder-open-source-toolkit-for-vr-creation-201913#reviews", BuilderEditorStyles.IndentLarge);
            GUILayout.EndArea();
        }
    }
}
