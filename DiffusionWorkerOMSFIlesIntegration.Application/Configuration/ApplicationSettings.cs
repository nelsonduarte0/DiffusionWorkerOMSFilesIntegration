namespace DiffusionWorkerOMSFIlesIntegration.Application.Configuration
{
    public class ApplicationSettings : IApplicationSettings
    {
        public Constants Constants { get; set; }
        public TenantsSettings Tenants { get; set; }
    }
}
