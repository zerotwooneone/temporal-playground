import { Handle, Position } from '@xyflow/react';
import { GitBranch } from 'lucide-react';
import type { NodeProps } from '@xyflow/react';

interface DecisionNodeData {
  name: string;
  isConfigured?: boolean;
}

export default function DecisionNode({ data }: NodeProps) {
  const nodeData = data as unknown as DecisionNodeData;
  const isConfigured = nodeData.isConfigured ?? false;

  return (
    <div className="bg-white border-2 border-orange-500 rounded-lg shadow-md min-w-[200px]">
      {/* Target Handle */}
      <Handle type="target" position={Position.Top} className="w-3 h-3 !bg-orange-500" />

      {/* Header */}
      <div className="bg-orange-500 text-white px-3 py-2 rounded-t-lg flex items-center gap-2">
        <GitBranch size={16} />
        <span className="font-semibold text-sm">Decision</span>
      </div>

      {/* Content */}
      <div className="p-3">
        <div className="font-medium text-gray-900 mb-1">{nodeData.name || 'Unnamed Decision'}</div>
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

      {/* Source Handles - True (left) and False (right) */}
      <div className="flex justify-between px-3 pb-3">
        <div className="flex items-center gap-2">
          <Handle 
            type="source" 
            id="True" 
            position={Position.Left} 
            className="w-3 h-3 !bg-green-500"
          />
          <span className="text-xs font-medium text-green-700">True</span>
        </div>
        <div className="flex items-center gap-2">
          <span className="text-xs font-medium text-red-700">False</span>
          <Handle 
            type="source" 
            id="False" 
            position={Position.Right} 
            className="w-3 h-3 !bg-red-500"
          />
        </div>
      </div>
    </div>
  );
}
