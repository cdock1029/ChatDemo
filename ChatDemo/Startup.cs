using System.ClientModel;
using System.Diagnostics;
using ChatDemo.Data;
using DevExpress.AIIntegration;
using DevExpress.Blazor;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;

namespace ChatDemo;

#pragma warning disable SKEXP0001

public static class Startup
{
    public static IServiceProvider? Services { get; private set; }

    public static void Init()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((ctx, builder) => { builder.AddUserSecrets<App>(); })
            .ConfigureServices(WireupServices)
            .Build();

        Services = host.Services;
    }

    private static void WireupServices(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddWpfBlazorWebView();
        services.AddDevExpressBlazor(configure => configure.BootstrapVersion = BootstrapVersion.v5);
        services.AddSingleton<WeatherForecastService>();

        ChatConfig? chatConfig = ctx.Configuration.GetSection("ChatConfig").Get<ChatConfig>();


        /* this works */
        IChatClient client =
	        new OpenAIClient(new ApiKeyCredential(chatConfig!.OpenAiApiKey)).AsChatClient("gpt-4o-mini");


        /* this does not work */

        //Kernel kernel = Kernel.CreateBuilder()
        //    .AddOpenAIChatCompletion(apiKey: chatConfig!.OpenAiApiKey, modelId: "gpt-4o-mini").Build();

        //IChatClient client = kernel.GetRequiredService<IChatCompletionService>().AsChatClient();

        services.AddSingleton(client);
        services.AddDevExpressAI(settings => settings.RegisterAIExceptionHandler(new ChatExceptionHandler()));


#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
    }

    private sealed class ChatConfig
    {
        public string OpenAiApiKey { get; init; } = null!;
    }

    private class ChatExceptionHandler : IAIExceptionHandler
    {
        public Exception ProcessException(Exception e)
        {
            Debug.WriteLine($"AI Exception in ChatDemo: {e.Message}");
            return e;
        }
    }
}