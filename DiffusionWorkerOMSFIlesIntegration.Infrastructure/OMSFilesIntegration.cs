using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DiffusionWorkerOMSFIlesIntegration.Application;
using DiffusionWorkerOMSFIlesIntegration.Application.Configuration;
using DiffusionWorkerOMSFIlesIntegration.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiffusionWorkerOMSFIlesIntegration.Infrastructure
{
    public class OMSFilesIntegration : IProcessHandler<string>
    {
        private readonly ILogger<OMSFilesIntegration> _logger;
        private readonly IApplicationSettings _applicationSettings;

        public OMSFilesIntegration(
            ILogger<OMSFilesIntegration> logger,
            IApplicationSettings applicationSettings
            )
        {
            _logger = logger;
            _applicationSettings = applicationSettings;
        }

        public Task<ProcessStatus> GetProcessStatusAsync(string process)
        {
            throw new NotImplementedException();
        }

        public async Task DoAsync()
        {
            try
            {
                await ProcessBlobContainer(_applicationSettings.Tenants.Wells); //process files for wells
                await ProcessBlobContainer(_applicationSettings.Tenants.Continente); //process files for continente

            } catch(Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[DiffusionWorkerOMSFIlesIntegration.OMSFilesIntegration.DoAsync]: Got exception"
                );
                Environment.Exit(1);
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Downloads files from a blob storage to a local directory defined in OutputPath configuration provided by tenant settings
        /// </summary>
        /// <param name="tenantSettings">the tenant settings</param>
        /// <returns></returns>
        public async Task ProcessBlobContainer(TenantSettings tenantSettings)
        {
            _logger.LogInformation("[OMSFilesIntegration.ProcessBlobContainer]: Started at [{time}] for tenant [{tenant}]", 
                DateTimeOffset.Now, tenantSettings.Name);

            BlobStorageSettings blobSettings = tenantSettings.BlobStorageSettings;
            try
            {
                var options = new BlobClientOptions();
                options.Retry.MaxRetries = blobSettings.MaxRetries;
                options.Retry.Delay = TimeSpan.FromSeconds(blobSettings.DelaySeconds);
                var a = blobSettings.NetworkTimeoutSeconds;
                options.Retry.NetworkTimeout = TimeSpan.FromSeconds(blobSettings.NetworkTimeoutSeconds);

                BlobServiceClient blobServiceClient = new BlobServiceClient(blobSettings.ConnectionString, options);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(blobSettings.Container);

                
                var filesList = containerClient.GetBlobsAsync();

                await foreach (BlobItem blobItem in filesList)
                {

                    var path = tenantSettings.FileShareSettings.OutputPath + blobItem.Name;
                    using (var file = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                        await blobClient.DownloadToAsync(file);
                        var blobCreatedOn = blobItem.Properties.CreatedOn;
                        if (blobCreatedOn != null)
                        {
                            File.SetCreationTimeUtc(path, blobCreatedOn.Value.UtcDateTime);
                            var fileCreationTime = File.GetCreationTime(path);
                        }
                        _logger.LogInformation("[OMSFilesIntegration.ProcessBlobContainer]: Downloaded file [{filename}] at [{time}] for tenant [{tenant}] to [{path}]",
                        blobItem.Name, DateTimeOffset.Now, tenantSettings.Name, tenantSettings.FileShareSettings.OutputPath);
                        blobClient.Delete();

                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[DiffusionWorkerOMSFIlesIntegration.OMSFilesIntegration.ProcessBlobContainer]: Error processing blob container [{blobName}]",
                    tenantSettings.Name
                );
                throw;
            }
            _logger.LogInformation("[DiffusionWorkerOMSFIlesIntegration.OMSFilesIntegration.ProcessBlobContainer]: Finished processing blob container for tenant [{tenant}]", tenantSettings.Name);

        }

        public async Task StartProcessAsync(string process)
        {
            await DoAsync();
        }
    }
}
