using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

namespace Arena
{
    public class LogView : MonoBehaviour
    {
        public int MaxMessages = 2;
        public TextMeshProUGUI LogText;
        private Queue<string> Messages = new();
        private StringBuilder MessageStringBuilder = new();

        public void Clear()
        {
            Messages.Clear();
            MessageStringBuilder.Clear();
            LogText.text = "";
        }

        public void AddMessage(string message)
        {
            if (Messages.Count == MaxMessages)
            {
                Messages.Dequeue();
            }
            Messages.Enqueue(message);
            MessageStringBuilder.Clear();
            foreach (var logMessage in Messages)
            {
                MessageStringBuilder.AppendLine(logMessage);
            }
            LogText.text = MessageStringBuilder.ToString();
        }
    }
}
