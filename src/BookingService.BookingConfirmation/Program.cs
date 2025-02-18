using BookingService.BookingConfirmation.Handlers;
using BookingService.BookingConfirmation.Middlewares;
using BookingService.BookingConfirmation.Models.Configuration;
using BookingService.BookingConfirmation.Models.Events;
using BookingService.BookingConfirmation.Services;
using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Program
{
    private static ILogger<Program>? _logger;

    static async Task Main(string[] args)
    {
        IHost appHost = Host
            .CreateDefaultBuilder(args)
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = true;
            })
            .ConfigureLogging((hbc, logging) =>
            {
                logging.AddConsole();
            })
            .ConfigureServices((hbc, services) =>
            {
                services.AddSingleton<IMailService, MailService>();
                services.Configure<MailSettings>(hbc.Configuration.GetSection("MailSettings"));

                var kafkaSettings = hbc.Configuration.GetRequiredSection("KafkaSettings").Get<KafkaSettings>();
                var commentsConsumerSettings = hbc.Configuration.GetRequiredSection("BookingCreatedConsumerSettings").Get<TopicSettings>();
                

                services.AddKafkaFlowHostedService(kafka => kafka
                    .UseMicrosoftLog()
                    .AddCluster(cluster => cluster
                    .WithBrokers(kafkaSettings?.BootstrapServers)
                    .WithSchemaRegistry(schema =>
                    {
                        schema.Url = kafkaSettings?.SchemaRegistry;
                        schema.BasicAuthCredentialsSource = Confluent.SchemaRegistry.AuthCredentialsSource.UserInfo;
                        schema.BasicAuthUserInfo = $"{kafkaSettings?.SaslUserName}:{kafkaSettings?.SaslPassword}";
                    })
                    .WithSecurityInformation(information =>
                    {
                        information.SecurityProtocol = SecurityProtocol.SaslSsl;
                        information.SaslMechanism = SaslMechanism.ScramSha256;
                        information.SaslUsername = kafkaSettings?.SaslUserName;
                        information.SaslPassword = kafkaSettings?.SaslPassword;
                    })
                    .AddConsumer(consumer => consumer
                        .Topic(commentsConsumerSettings?.Topic)
                        .WithName(commentsConsumerSettings?.WorkerName)
                        .WithGroupId(commentsConsumerSettings?.GroupId)
                        .WithBufferSize(kafkaSettings!.BufferSize)
                        .WithWorkersCount(kafkaSettings!.WorkersCount)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
                        .AddMiddlewares(middlewares => middlewares
                                                       .AddSingleTypeDeserializer<BookingCreatedEvent, NewtonsoftJsonDeserializer>()
                                                       .Add<EventErrorHandlingMiddleware>()
                                                       .AddTypedHandlers(handlers => handlers
                                                       .AddHandler<BookingCreatedHandler>()
                                                       .WhenNoHandlerFound(context =>
                                                                           Console.WriteLine($"Messages from partition {context.ConsumerContext.Partition} or Offset {context.ConsumerContext.Offset} are unhandled")))))
                    ));
            }).Build();

        _logger = appHost.Services.GetRequiredService<ILogger<Program>>();

        _logger.LogInformation("App Host created successfully");

        await appHost.RunAsync();
    }
}