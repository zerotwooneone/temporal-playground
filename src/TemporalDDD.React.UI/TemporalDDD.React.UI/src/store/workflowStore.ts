import { create } from 'zustand';
import {
  applyNodeChanges,
  applyEdgeChanges,
  addEdge,
  type Node,
  type Edge,
  type OnNodesChange,
  type OnEdgesChange,
  type OnConnect,
} from '@xyflow/react';

interface WorkflowStore {
  nodes: Node[];
  edges: Edge[];
  selectedNodeId: string | null;
  onNodesChange: OnNodesChange;
  onEdgesChange: OnEdgesChange;
  onConnect: OnConnect;
  setNodes: (nodes: Node[]) => void;
  setEdges: (edges: Edge[]) => void;
  setSelectedNodeId: (id: string | null) => void;
  addNode: (type: 'apiNode' | 'notificationNode') => void;
  updateNodeData: (id: string, data: Record<string, any>) => void;
  loadWorkflow: (workflowData: any) => void;
  clearWorkflow: () => void;
}

export const useWorkflowStore = create<WorkflowStore>((set) => ({
  nodes: [],
  edges: [],
  selectedNodeId: null,
  onNodesChange: (changes) =>
    set((state) => ({
      nodes: applyNodeChanges(changes, state.nodes),
    })),
  onEdgesChange: (changes) =>
    set((state) => ({
      edges: applyEdgeChanges(changes, state.edges),
    })),
  onConnect: (connection) =>
    set((state) => ({
      edges: addEdge(connection, state.edges),
    })),
  setNodes: (nodes) => set({ nodes }),
  setEdges: (edges) => set({ edges }),
  setSelectedNodeId: (id) => set({ selectedNodeId: id }),
  addNode: (type) =>
    set((state) => {
      const guid = crypto.randomUUID();
      const newNode: Node = {
        id: `WFNId${guid}`,
        type,
        position: { x: 250, y: 150 },
        data: {
          name: type === 'apiNode' ? 'New API Task' : 'New Notification',
          nodeType: type === 'apiNode' ? 1 : 2,
          isConfigured: false,
        },
      };
      return { nodes: [...state.nodes, newNode] };
    }),
  updateNodeData: (id, data) =>
    set((state) => ({
      nodes: state.nodes.map((node) =>
        node.id === id
          ? { ...node, data: { ...node.data, ...data } }
          : node
      ),
    })),
  loadWorkflow: (workflowData) =>
    set(() => {
      let nodes: Node[] = [];
      let edges: Edge[] = [];

      // Check if flowJson exists and has saved canvas state
      if (workflowData.flowJson) {
        try {
          const flowData = JSON.parse(workflowData.flowJson);
          nodes = flowData.nodes || [];
          edges = flowData.edges || [];
        } catch (e) {
          console.error('Failed to parse flowJson:', e);
        }
      }

      // If no saved canvas state, auto-generate from backend nodes
      if (nodes.length === 0 && workflowData.nodes.length > 0) {
        nodes = workflowData.nodes.map((node: any) => {
          let nodeType: string;
          let position: { x: number; y: number };

          // Map node types to React Flow types and assign default positions
          if (node.nodeType === 0) {
            nodeType = 'start';
            position = { x: 100, y: 300 };
          } else if (node.nodeType === 99) {
            nodeType = 'end';
            position = { x: 800, y: 300 };
          } else if (node.nodeType === 1) {
            nodeType = 'apiNode';
            position = { x: 250, y: 150 };
          } else if (node.nodeType === 2) {
            nodeType = 'notificationNode';
            position = { x: 250, y: 150 };
          } else {
            nodeType = 'custom';
            position = { x: 250, y: 150 };
          }

          return {
            id: node.id,
            type: nodeType,
            position,
            data: {
              label: node.name,
              name: node.name,
              nodeType: node.nodeType,
              businessNotes: node.businessNotes,
              isConfigured: node.isConfigured,
              endpointUrl: node.endpointUrl,
              authToken: node.authToken,
              retryPolicyMaxAttempts: node.retryPolicyMaxAttempts,
              retryPolicyBackoffCoefficient: node.retryPolicyBackoffCoefficient,
              contractMappingConvertXmlToJson: node.contractMappingConvertXmlToJson,
              contractMappingQueryParameters: node.contractMappingQueryParameters,
              contractMappingRequestMapping: node.contractMappingRequestMapping,
              contractMappingResponseMapping: node.contractMappingResponseMapping,
              messageTemplate: node.messageTemplate,
            },
          };
        });
      }

      return { nodes, edges, selectedNodeId: null };
    }),
  clearWorkflow: () =>
    set(() => ({
      nodes: [],
      edges: [],
      selectedNodeId: null,
    })),
}));
