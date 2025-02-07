using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace  VRBuilder.UI.Console
{
    public class LogConsole : MonoBehaviour
    {
        [SerializeField]
        private VisualTreeAsset logItemTemplate;
        private UIDocument logConsole;

        private void Start()
        {
            logConsole = GetComponent<UIDocument>();
            var root = logConsole.rootVisualElement;
            var listView = root.Q<ListView>("LogList");

            List<string> listElements = new List<string>()
            {
                "Test", "fjkalödfkjldsafölkasjfölksdjföadsiortuejwnvnfdvölkjdfs kdölsfjasgkjndfsöafdaskflöja", "Potato"
            };
            
            // Bind item data to ListView
            listView.makeItem = () =>logItemTemplate.CloneTree();
            
            listView.bindItem = (element, index) =>
            {
                // Bind data to UI
                var label = element.Q<Label>();
                label.text = listElements[index];
            };

            // Set ListView properties
            listView.itemsSource = listElements;
            //listView.fixedItemHeight = 30; // Optional: Adjust if needed
        }
    }
}

