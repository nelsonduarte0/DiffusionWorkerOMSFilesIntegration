using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffusionWorkerOMSFIlesIntegration.Application.Configuration
{
    public class BlobStorageSettings
    {
        public string Container { get; set; }
        public string ConnectionString { get; set; }
    }
}
