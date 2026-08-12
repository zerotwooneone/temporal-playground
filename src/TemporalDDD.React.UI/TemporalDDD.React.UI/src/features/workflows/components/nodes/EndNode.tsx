import { Handle, Position } from '@xyflow/react';
import { Square } from 'lucide-react';
import type { NodeProps } from '@xyflow/react';

interface EndNodeData {
  name: string;
  isConfigured?: boolean;
}

export default function EndNode({ data }: NodeProps) {
  const nodeData = data as unknown as EndNodeData;

  return (
    <div className="bg-white border-2 border-red-500 rounded-lg shadow-md min-w-[200px]">
      {/* Target Handle */}
      <Handle type="target" position={Position.Top} className="w-3 h-3 !bg-red-500" />

      {/* Header */}
      <div className="bg-red-500 text-white px-3 py-2 rounded-t-lg flex items-center gap-2">
        <Square size={16} />
        <span className="font-semibold text-sm">End</span>
      </div>

      {/* Content */}
      <div className="p-3">
        <div className="font-medium text-gray-900">{nodeData.name || 'End'}</div>
      </div>
    </div>
  );
}
