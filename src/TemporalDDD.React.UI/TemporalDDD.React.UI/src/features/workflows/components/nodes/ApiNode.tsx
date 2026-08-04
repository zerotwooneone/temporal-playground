import { Handle, Position } from '@xyflow/react';
import { Globe } from 'lucide-react';
import type { NodeProps } from '@xyflow/react';

interface ApiNodeData {
  name: string;
  endpointUrl?: string;
  isConfigured?: boolean;
}

export default function ApiNode({ data }: NodeProps) {
  const nodeData = data as unknown as ApiNodeData;
  const isConfigured = nodeData.isConfigured ?? false;

  return (
    <div className="bg-white border-2 border-blue-500 rounded-lg shadow-md min-w-[200px]">
      {/* Target Handle */}
      <Handle type="target" position={Position.Top} className="w-3 h-3 !bg-blue-500" />

      {/* Header */}
      <div className="bg-blue-500 text-white px-3 py-2 rounded-t-lg flex items-center gap-2">
        <Globe size={16} />
        <span className="font-semibold text-sm">API Task</span>
      </div>

      {/* Content */}
      <div className="p-3">
        <div className="font-medium text-gray-900 mb-1">{nodeData.name || 'Unnamed API'}</div>
        {nodeData.endpointUrl && (
          <div className="text-xs text-gray-500 truncate" title={nodeData.endpointUrl}>
            {nodeData.endpointUrl}
          </div>
        )}
      </div>

      {/* Status Badge */}
      <div className="px-3 pb-3">
        {isConfigured ? (
          <span className="inline-flex items-center px-2 py-1 text-xs font-medium text-green-800 bg-green-100 rounded-full">
            Configured
          </span>
        ) : (
          <span className="inline-flex items-center px-2 py-1 text-xs font-medium text-amber-800 bg-amber-100 rounded-full">
            Unconfigured
          </span>
        )}
      </div>

      {/* Source Handle */}
      <Handle type="source" position={Position.Bottom} className="w-3 h-3 !bg-blue-500" />
    </div>
  );
}
