using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Debug = UnityEngine.Debug;



namespace AGoodNameLib
{
    public class webhook_discord
    {
        public string defaultBotName = "\"A-Good-Name-Lib - Bot\"";
            public void SendDiscordMessage(string message, string webhookUrl, string botName = null)
            {
                botName = botName ?? defaultBotName;
                try
                {
                    Debug.Log("Sending discord message");
                    string webhook = webhookUrl;

                    WebClient client = new WebClient();
                    client.Headers.Add("Content-Type", "application/json");
                    string payload = $"{{\"content\": \"{message}\", \"username\": \"{botName}\"}}";
                    client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
                    
                }
                catch (WebException ex)
                {
                    Debug.LogError($"Error sending Discord message: {ex.Message}");

                    if (ex.Response != null)
                    {
                        using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            string responseText = reader.ReadToEnd();
                            Debug.LogError($"Discord response: {responseText}");
                        }
                    }
                }
            }
    }
}
