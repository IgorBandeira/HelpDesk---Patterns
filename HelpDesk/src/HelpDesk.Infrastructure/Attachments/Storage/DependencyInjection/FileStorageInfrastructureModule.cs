using Amazon;
using Amazon.S3;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Infrastructure.Attachments.Storage;
using HelpDesk.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Attachments.DependencyInjection
{
    public static class FileStorageInfrastructureModule
    {
        public static IServiceCollection AddFileStorageInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<S3Options>(configuration.GetSection("S3"));

            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();

            services.AddScoped<IFileStoragePort, S3FileStoragePort>();

            return services;
        }
    }
}