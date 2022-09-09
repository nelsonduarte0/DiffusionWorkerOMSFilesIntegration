using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffusionWorkerOMSFIlesIntegration.Application.Services.Interfaces
{
    public interface IProcessHandler<TProcess>
    {
        public Task StartProcessAsync(TProcess process);
        public Task<ProcessStatus> GetProcessStatusAsync(TProcess process);
    }
}
