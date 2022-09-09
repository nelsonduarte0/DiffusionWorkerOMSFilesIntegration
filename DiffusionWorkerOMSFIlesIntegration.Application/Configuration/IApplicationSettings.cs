namespace DiffusionWorkerOMSFIlesIntegration.Application.Configuration
{
    public interface IApplicationSettings
    {
        public Constants Constants { get; set; }
        public TenantsSettings Tenants { get; set; }


    }
}
