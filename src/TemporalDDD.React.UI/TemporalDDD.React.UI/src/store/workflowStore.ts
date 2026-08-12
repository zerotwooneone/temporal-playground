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
import type { ParameterBinding, NodeInputDefinition, NodeOutputDefinition } from '../types/workflowTypes';

interface WorkflowStore {
  nodes: Node[];
  edges: Edge[];
  selectedNodeIds: string[];
  selectedEdgeIds: string[];
  onNodesChange: OnNodesChange;
  onEdgesChange: OnEdgesChange;
  onConnect: OnConnect;
  setNodes: (nodes: Node[]) => void;
  setEdges: (edges: Edge[]) => void;
  setSelectedNodeIds: (ids: string[]) => void;
  setSelectedEdgeIds: (ids: string[]) => void;
  toggleNodeSelection: (id: string, isMultiSelect: boolean) => void;
  toggleEdgeSelection: (id: string, isMultiSelect: boolean) => void;
  addNode: (type: 'apiNode' | 'notificationNode') => void;
  updateNodeData: (id: string, data: Record<string, any>) => void;
  updateNodeInputBindings: (id: string, bindings: ParameterBinding[]) => void;
  deleteSelectedEdges: () => void;
  loadWorkflow: (workflowData: any) => void;
  clearWorkflow: () => void;
}

export const useWorkflowStore = create<WorkflowStore>((set) => ({
  nodes: [],
  edges: [],
  selectedNodeIds: [],
  selectedEdgeIds: [],
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
  setSelectedNodeIds: (ids) => set({ selectedNodeIds: ids }),
  setSelectedEdgeIds: (ids) => set({ selectedEdgeIds: ids }),
  toggleNodeSelection: (id, isMultiSelect) =>
    set((state) => {
      if (isMultiSelect) {
        // Toggle selection: add if not selected, remove if selected
        if (state.selectedNodeIds.includes(id)) {
          return { selectedNodeIds: state.selectedNodeIds.filter((selectedId) => selectedId !== id) };
        } else {
          return { selectedNodeIds: [...state.selectedNodeIds, id] };
        }
      } else {
        // Single select: clear all and select only this one
        return { selectedNodeIds: [id] };
      }
    }),
  toggleEdgeSelection: (id, isMultiSelect) =>
    set((state) => {
      if (isMultiSelect) {
        // Toggle selection: add if not selected, remove if selected
        if (state.selectedEdgeIds.includes(id)) {
          return { selectedEdgeIds: state.selectedEdgeIds.filter((selectedId) => selectedId !== id) };
        } else {
          return { selectedEdgeIds: [...state.selectedEdgeIds, id] };
        }
      } else {
        // Single select: clear all and select only this one
        return { selectedEdgeIds: [id] };
      }
    }),
  addNode: (type) =>
    set((state) => {
      const guid = crypto.randomUUID();
      let inputDefinitions: NodeInputDefinition[] = [];
      let outputDefinitions: NodeOutputDefinition[] = [];

      // Initialize contracts based on node type
      if (type === 'apiNode') {
        // API nodes have fixed output: ApiResponse as JsonDocument
        outputDefinitions = [
          { propertyName: 'ApiResponse', dataType: { kind: 'Primitive', name: 'JsonDocument', value: 5 } }
        ];
      } else if (type === 'notificationNode') {
        // Notification nodes have fixed input: MessageTemplate as String
        inputDefinitions = [
          { propertyName: 'MessageTemplate', dataType: { kind: 'Primitive', name: 'String', value: 1 }, isRequired: true }
        ];
      }

      const newNode: Node = {
        id: `WFNId${guid}`,
        type,
        position: { x: 250, y: 150 },
        data: {
          name: type === 'apiNode' ? 'New API Task' : 'New Notification',
          nodeType: type === 'apiNode' ? 1 : 2,
          isConfigured: false,
          inputDefinitions,
          outputDefinitions,
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
  updateNodeInputBindings: (id, bindings) =>
    set((state) => ({
      nodes: state.nodes.map((node) =>
        node.id === id
          ? { ...node, data: { ...node.data, inputBindings: bindings } }
          : node
      ),
    })),
  deleteSelectedEdges: () =>
    set((state) => {
      if (state.selectedEdgeIds.length === 0) return state;
      return {
        edges: state.edges.filter((edge) => !state.selectedEdgeIds.includes(edge.id)),
        selectedEdgeIds: [],
      };
    }),
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
              inputDefinitions: node.inputDefinitions || [],
              outputDefinitions: node.outputDefinitions || [],
              inputBindings: node.inputBindings || [],
            },
          };
        });
      }

      return { nodes, edges, selectedNodeIds: [], selectedEdgeIds: [] };
    }),
  clearWorkflow: () =>
    set(() => ({
      nodes: [],
      edges: [],
      selectedNodeIds: [],
      selectedEdgeIds: [],
    })),
}));
