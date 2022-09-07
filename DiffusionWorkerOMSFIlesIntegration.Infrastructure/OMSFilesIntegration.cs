using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BitoqueBaseHammer.Application.Common.Interfaces;
using BitoqueBaseHammer.Application.Common.Interfaces.Sink;
using BitoqueBaseHammer.Application.Domain.Enums.Process;
using DiffusionWorkerOMSFIlesIntegration.Application.Configuration;
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
            BlobServiceClient blobServiceClient = new BlobServiceClient(blobSettings.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(blobSettings.Container);

            var filesList = containerClient.GetBlobs().ToArray();

            foreach (BlobItem blobItem in filesList)
            {
                _logger.LogInformation("OMSFilesIntegration.DoAsync readed file {time}", blobItem.Name);
                FileStream fileStream = File.OpenWrite(tenantSettings.FileShareSettings.OutputPath + blobItem.Name);
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                blobClient.DownloadTo(fileStream);
                fileStream.Close();
                blobClient.Delete();
                _logger.LogInformation("OMSFilesIntegration.DoAsync read file {time}", blobItem.Name);
            }
        }

        public async Task StartProcessAsync(string process)
        {
            await DoAsync();
        }
    }
}
