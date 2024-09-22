using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

public static class Config
{
    public static JsonConfig GetConfigItems()
    {
        var assembly = typeof(Config).GetTypeInfo().Assembly;

        string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith("config.json", StringComparison.OrdinalIgnoreCase));

        Stream stream = assembly.GetManifestResourceStream(resourceName);
        using (var reader = new StreamReader(stream))
        {
            var json = reader.ReadToEnd();
            var jsonObj = JObject.Parse(json);

            return new()
            {
                SendGridApiKey = jsonObj["SENDGRID_API_KEY"].ToString(),
                MailFrom = jsonObj["MAIL_FROM"].ToString(),
                MailTo = jsonObj["MAIL_TO"].ToString(),
                UsersData = jsonObj["AUTH_USERS"].ToString(),
                MasterUser = jsonObj["MASTER_USER"].ToString()
            };
        }
    }
}