from typing import Dict
from semantic_kernel import Kernel
from threading import Lock


class IKernelStore:
    def get_kernel(self, agent_id: str) -> Kernel:
        pass

    def add_kernel(self, agent_id: str, kernel: Kernel) -> None:
        pass

    def remove_kernel(self, agent_id: str) -> None:
        pass


class KernelStore(IKernelStore):
    def __init__(self):
        self._kernels: Dict[str, Kernel] = {}
        self._lock = Lock()  # For thread safety

    def get_kernel(self, agent_id: str) -> Kernel:
        with self._lock:
            if agent_id in self._kernels:
                return self._kernels[agent_id]
            raise KeyError(f"Kernel not found for agent ID: {agent_id}")

    def add_kernel(self, agent_id: str, kernel: Kernel) -> None:
        with self._lock:
            self._kernels[agent_id] = kernel

    def remove_kernel(self, agent_id: str) -> None:
        with self._lock:
            self._kernels.pop(agent_id, None)
