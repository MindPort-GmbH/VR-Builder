using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace  VRBuilder.UI.Console
{
    public class LogConsole : MonoBehaviour, ILogConsole
    {
        [SerializeField]
        private VisualTreeAsset logItemTemplate;
        private UIDocument logConsole;
        private List<ILogMessage> logs = new List<ILogMessage>();

        private void Start()
        {
            logConsole = GetComponent<UIDocument>();
            var root = logConsole.rootVisualElement;
            root.Focus();
            var listView = root.Q<ListView>("LogList");
            
            // Bind item data to ListView
            listView.makeItem = () =>logItemTemplate.CloneTree();

            listView.bindItem = (element, index) => logs[index].Bind(element);

            // Set ListView properties
            listView.itemsSource = logs;
        }

        public void Log(ILogMessage logMessage)
        {
            logs.Add(logMessage);
        }

        public void Clear()
        {
            logs.Clear();
        }
    }
}

