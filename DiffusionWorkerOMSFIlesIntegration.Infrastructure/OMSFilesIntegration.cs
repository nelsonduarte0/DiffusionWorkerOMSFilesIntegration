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
            ProcessBlobContainer(_applicationSettings.Tenants.Wells); //process files from wells
            ProcessBlobContainer(_applicationSettings.Tenants.Continente); //process files from continente

            return;
        }

        public void ProcessBlobContainer(TenantSettings tenantSettings)
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
                    blobClient.DownloadTo(fileStream);
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
                return;
            }
        }

        public async Task StartProcessAsync(string process)
        {
            await DoAsync();
        }
    }
}
