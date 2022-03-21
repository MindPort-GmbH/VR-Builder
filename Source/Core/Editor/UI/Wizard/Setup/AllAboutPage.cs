// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021 MindPort GmbH

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

                BuilderGUILayout.DrawLink("Video tutorials", "https://www.youtube.com/playlist?list=PLDebLYuCiIXSsh80SDe23iJc8fdmNeq27", BuilderEditorStyles.IndentLarge);
                BuilderGUILayout.DrawLink("VR Builder documentation", "https://www.mindport.co/documentation", BuilderEditorStyles.IndentLarge);

                GUILayout.Label("Community", BuilderEditorStyles.Header);

                BuilderGUILayout.DrawLink("Community", "http://community.mindport.co", BuilderEditorStyles.IndentLarge);
                BuilderGUILayout.DrawLink("Contact us", "mailto:info@mindport.co", BuilderEditorStyles.IndentLarge);
            GUILayout.EndArea();
        }
    }
}
