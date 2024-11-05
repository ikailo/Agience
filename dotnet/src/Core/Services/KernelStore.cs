using Microsoft.SemanticKernel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agience.Core.Services
{
    public interface IKernelStore
    {
        Kernel GetKernel(string agentId);
        void AddKernel(string agentId, Kernel kernel);
        void RemoveKernel(string agentId);
    }

    public class KernelStore : IKernelStore
    {
        private readonly ConcurrentDictionary<string, Kernel> _kernels = new();

        public Kernel GetKernel(string agentId)
        {
            if (_kernels.TryGetValue(agentId, out var kernel))
            {
                return kernel;
            }

            throw new KeyNotFoundException($"Kernel not found for agent ID: {agentId}");
        }

        public void AddKernel(string agentId, Kernel kernel)
        {
            _kernels[agentId] = kernel;
        }

        public void RemoveKernel(string agentId)
        {
            _kernels.TryRemove(agentId, out _);
        }
    }

}
