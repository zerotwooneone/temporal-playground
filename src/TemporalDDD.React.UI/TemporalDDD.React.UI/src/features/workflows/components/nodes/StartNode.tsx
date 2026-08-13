import { Handle, Position } from '@xyflow/react';
import { Play } from 'lucide-react';
import type { NodeProps } from '@xyflow/react';

interface StartNodeData {
  name: string;
  isConfigured?: boolean;
}

export default function StartNode({ data }: NodeProps) {
  const nodeData = data as unknown as StartNodeData;

  return (
    <div className="bg-white border-2 border-green-500 rounded-lg shadow-md min-w-[200px]">
      {/* Source Handle */}
      <Handle type="source" id="Default" position={Position.Bottom} className="w-3 h-3 !bg-green-500" />

      {/* Header */}
      <div className="bg-green-500 text-white px-3 py-2 rounded-t-lg flex items-center gap-2">
        <Play size={16} />
        <span className="font-semibold text-sm">Start</span>
      </div>

      {/* Content */}
      <div className="p-3">
        <div className="font-medium text-gray-900">{nodeData.name || 'Start'}</div>
      </div>
    </div>
  );
}
