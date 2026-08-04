import '@xyflow/react/dist/style.css';
import { ReactFlow, Background, Controls } from '@xyflow/react';
import { useWorkflowStore } from '../../../store/workflowStore';

export default function WorkflowDesigner() {
  const { nodes, edges, onNodesChange, onEdgesChange, onConnect } = useWorkflowStore();

  return (
    <div className="w-full h-full">
      <ReactFlow
        nodes={nodes}
        edges={edges}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onConnect={onConnect}
        fitView
      >
        <Background />
        <Controls />
      </ReactFlow>
    </div>
  );
}
