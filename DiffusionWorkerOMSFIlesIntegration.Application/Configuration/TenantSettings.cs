using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffusionWorkerOMSFIlesIntegration.Application.Configuration
{
    public class TenantSettings
    {
        public BlobStorageSettings BlobStorageSettings { get; set; }

        public String Name { get; set; }

        public FileShareSettings FileShareSettings { get; set; }

    }
}
