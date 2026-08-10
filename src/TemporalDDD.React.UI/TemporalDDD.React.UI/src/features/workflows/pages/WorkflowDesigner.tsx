import '@xyflow/react/dist/style.css';
import { ReactFlow, Background, Controls, type Node } from '@xyflow/react';
import { useParams } from 'react-router-dom';
import { useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useWorkflowStore } from '../../../store/workflowStore';
import { getWorkflowById } from '../../../api/client';
import ApiNode from '../components/nodes/ApiNode';
import NotificationNode from '../components/nodes/NotificationNode';
import WorkflowToolbar from '../components/WorkflowToolbar';
import NodePropertiesPanel from '../components/NodePropertiesPanel';

const nodeTypes = {
  apiNode: ApiNode,
  notificationNode: NotificationNode,
};

export default function WorkflowDesigner() {
  const { id } = useParams<{ id: string }>();
  const { nodes, edges, onNodesChange, onEdgesChange, onConnect, setSelectedNodeId, loadWorkflow, clearWorkflow } = useWorkflowStore();

  const { data: workflowData, isLoading, error } = useQuery({
    queryKey: ['workflow', id],
    queryFn: () => getWorkflowById(id!),
    enabled: !!id,
  });

  useEffect(() => {
    if (workflowData) {
      loadWorkflow(workflowData);
    }
  }, [workflowData, loadWorkflow]);

  useEffect(() => {
    // Clear workflow when navigating away (no id)
    if (!id) {
      clearWorkflow();
    }
  }, [id, clearWorkflow]);

  const onNodeClick = (_: React.MouseEvent, node: Node) => {
    setSelectedNodeId(node.id);
  };

  const onPaneClick = () => {
    setSelectedNodeId(null);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-gray-600">Loading workflow...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-red-600">Error loading workflow: {(error as Error).message}</div>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full">
      <WorkflowToolbar workflowId={id || `WFLId${crypto.randomUUID()}`} />

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
