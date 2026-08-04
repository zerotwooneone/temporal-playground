import '@xyflow/react/dist/style.css';
import { ReactFlow, Background, Controls, type Node } from '@xyflow/react';
import { useWorkflowStore } from '../../../store/workflowStore';
import ApiNode from '../components/nodes/ApiNode';
import NotificationNode from '../components/nodes/NotificationNode';
import WorkflowToolbar from '../components/WorkflowToolbar';
import NodePropertiesPanel from '../components/NodePropertiesPanel';

const nodeTypes = {
  apiNode: ApiNode,
  notificationNode: NotificationNode,
};

export default function WorkflowDesigner() {
  const { nodes, edges, onNodesChange, onEdgesChange, onConnect, setSelectedNodeId } = useWorkflowStore();

  const onNodeClick = (_: React.MouseEvent, node: Node) => {
    setSelectedNodeId(node.id);
  };

  const onPaneClick = () => {
    setSelectedNodeId(null);
  };

  return (
    <div className="flex flex-col h-full">
      <WorkflowToolbar />
      
      <div className="flex-1 relative">
        <ReactFlow
          nodes={nodes}
          edges={edges}
          onNodesChange={onNodesChange}
          onEdgesChange={onEdgesChange}
          onConnect={onConnect}
          onNodeClick={onNodeClick}
          onPaneClick={onPaneClick}
          nodeTypes={nodeTypes}
          fitView
        >
          <Background />
          <Controls />
        </ReactFlow>
        
        <NodePropertiesPanel />
      </div>
    </div>
  );
}
