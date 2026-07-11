using Microsoft.AspNetCore.Hosting;

namespace Clinic.Infrastructure.Helpers;

public interface IEmailBodyBuilder
{
    string GenerateEmailBody(string template, Dictionary<string, string> templateModel);
}

public class EmailBodyBuilder(IWebHostEnvironment env) : IEmailBodyBuilder
{
    public string GenerateEmailBody(string template, Dictionary<string, string> templateModel)
    {
        var templatePath = Path.Combine(env.ContentRootPath, "Templates", $"{template}.html");
        using var streamReader = new StreamReader(templatePath);
        var body = streamReader.ReadToEnd();

        foreach (var item in templateModel)
            body = body.Replace(item.Key, item.Value);

        return body;
    }
}
