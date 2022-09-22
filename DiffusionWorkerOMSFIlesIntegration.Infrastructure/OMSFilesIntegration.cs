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
            for (int i = 0; i < _applicationSettings.Tenants.Wells.BlobStorageSettings.RetryCount; i++)
            {
                try
                {
                    await ProcessBlobContainer(_applicationSettings.Tenants.Wells); //process files from wells
                    _logger.LogInformation("[DiffusionWorkerOMSFIlesIntegration.OMSFilesIntegration.DoAsync] Finished OMS action with [{retryCount}] retries",
                    i);
                    break;

                } catch(Exception ex)
                {
                    _logger.LogError("[DiffusionWorkerOMSFIlesIntegration.OMSFilesIntegration.DoAsync] Got an exception [{retryCount}] retries",
                   i);
                    await Task.Delay(_applicationSettings.Tenants.Wells.BlobStorageSettings.RetryWaitTime);
                }
            }
            for (int i = 0; i < _applicationSettings.Tenants.Continente.BlobStorageSettings.RetryCount; i++)
            {
                try
                {
                    await ProcessBlobContainer(_applicationSettings.Tenants.Continente); //process files from continente
                    _logger.LogInformation("[DiffusionWorkerOMSFIlesIntegration.OMSFilesIntegration.DoAsync] Finished OMS action with [{retryCount}] retries",
                    i);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError("[DiffusionWorkerOMSFIlesIntegration.OMSFilesIntegration.DoAsync] Got an exception [{retryCount}] retries",
                   i);
                    await Task.Delay(_applicationSettings.Tenants.Continente.BlobStorageSettings.RetryWaitTime);
                }
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
            _logger.LogInformation("OMSFilesIntegration.ProcessBlobContainer started at {time} for tenant {tenant}", 
                DateTimeOffset.Now, tenantSettings.Name);

            BlobStorageSettings blobSettings = tenantSettings.BlobStorageSettings;
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(blobSettings.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(blobSettings.Container);

                var filesList = containerClient.GetBlobs().ToArray();

                foreach (BlobItem blobItem in filesList)
                {
                    FileStream fileStream = File.OpenWrite(tenantSettings.FileShareSettings.OutputPath + blobItem.Name);
                    BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                    await blobClient.DownloadToAsync(fileStream);
                    _logger.LogInformation("OMSFilesIntegration.ProcessBlobContainer downloaded file {filename} at {time} for tenant {tenant} to {path}",
                        blobItem.Name, DateTimeOffset.Now, tenantSettings.Name, tenantSettings.FileShareSettings.OutputPath);
                    fileStream.Close();
                    blobClient.Delete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "OMSFilesIntegration.ProcessBlobContainer got exception processing blob container [{blobName}]",
                    tenantSettings.Name
                );
                throw;
            }
        }

        public async Task StartProcessAsync(string process)
        {
            await DoAsync();
        }
    }
}
