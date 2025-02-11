using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.UI.Console
{
    public class LogConsole : MonoBehaviour, ILogConsole
    {
        [SerializeField]
        private VisualTreeAsset logItemTemplate;
        private UIDocument logConsole;
        private List<LogMessage> logs = new List<LogMessage>();
        private ListView listView;

        private void Start()
        {
            logs.Add(new LogMessage("This is a hardcoded message", "gidsfuoihsd", LogType.Log));


            logConsole = GetComponent<UIDocument>();
            var root = logConsole.rootVisualElement;
            root.Focus();
            listView = root.Q<ListView>("LogList");

            // Bind item data to ListView
            listView.makeItem = () => logItemTemplate.CloneTree();

            listView.bindItem = Bind;

            // Set ListView properties
            listView.itemsSource = logs;
        }

        private void Bind(VisualElement element, int index)
        {
            Label message = element.Q<Label>("Message");
            Label details = element.Q<Label>("Details");
            VisualElement image = element.Q<VisualElement>("Icon");

            if (message != null)
            {
                message.text = logs[index].Message;
            }

            if (details != null)
            {
                details.text = logs[index].Details;
            }

            if (image != null)
            {
                //switch (logs[index].LogType)
                //{
                //    case LogType.Log:
                //        image.style.backgroundImage =
                //            new StyleBackground((Texture2D)EditorGUIUtility.IconContent("console.infoicon").image);
                //        break;
                //    case LogType.Warning:
                //        image.image = EditorGUIUtility.IconContent("console.warnicon").image;
                //        break;
                //    case LogType.Error:
                //        image.image = EditorGUIUtility.IconContent("console.erroricon").image;
                //        break;
                //    case LogType.Exception:
                //        image.image = EditorGUIUtility.IconContent("console.erroricon").image;
                //        break;
                //}
            }
        }

        public void Clear()
        {
            logs.Clear();
            listView?.RefreshItems();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void LogMessage(string message, string details, LogType logType)
        {
            logs.Add(new LogMessage(message, details, logType));
            listView?.RefreshItems();
        }
    }
}

