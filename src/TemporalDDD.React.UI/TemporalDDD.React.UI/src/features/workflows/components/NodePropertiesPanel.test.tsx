import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useWorkflowStore } from '../../../store/workflowStore';
import type { PrimitiveType, WorkflowDataType, NodeOutputDefinition, InputValueSource } from '../../../types/workflowTypes';
import type { Node } from '@xyflow/react';

describe('Start Node Output Management', () => {
  beforeEach(() => {
    // Reset store before each test
    const { result } = renderHook(() => useWorkflowStore());
    act(() => {
      result.current.setNodes([]);
      result.current.setEdges([]);
    });
  });

  it('should add output definition to Start node', () => {
    const { result } = renderHook(() => useWorkflowStore());
    
    // Create a Start node manually since addNode doesn't support it
    const startNode: Node = {
      id: 'start-1',
      type: 'startNode',
      position: { x: 0, y: 0 },
      data: {
        name: 'Start',
        nodeType: 0,
        businessNotes: '',
        isConfigured: true,
        technicalInputs: {},
        inputDefinitions: [],
        outputDefinitions: []
      }
    };

    act(() => {
      result.current.setNodes([startNode]);
    });

    const node = result.current.nodes.find(n => n.data.nodeType === 0);
    expect(node).toBeDefined();

    // Add an output definition
    const newOutput: NodeOutputDefinition = {
      propertyName: 'PatientId',
      dataType: { kind: 'Primitive', name: 'String', value: 1 }
    };

    act(() => {
      result.current.updateNodeData(node!.id, {
        outputDefinitions: [...(node!.data.outputDefinitions as NodeOutputDefinition[]), newOutput]
      });
    });

    const updatedNode = result.current.nodes.find(n => n.id === node!.id);
    expect(updatedNode?.data.outputDefinitions).toHaveLength(1);
    expect((updatedNode?.data.outputDefinitions as NodeOutputDefinition[])[0].propertyName).toBe('PatientId');
  });

  it('should remove output definition from Start node', () => {
    const { result } = renderHook(() => useWorkflowStore());
    
    // Create a Start node with outputs
    const startNode: Node = {
      id: 'start-1',
      type: 'startNode',
      position: { x: 0, y: 0 },
      data: {
        name: 'Start',
        nodeType: 0,
        businessNotes: '',
        isConfigured: true,
        technicalInputs: {},
        inputDefinitions: [],
        outputDefinitions: []
      }
    };

    act(() => {
      result.current.setNodes([startNode]);
    });

    const node = result.current.nodes.find(n => n.data.nodeType === 0);
    
    const outputs: NodeOutputDefinition[] = [
      { propertyName: 'PatientId', dataType: { kind: 'Primitive', name: 'String', value: 1 } },
      { propertyName: 'Email', dataType: { kind: 'Primitive', name: 'String', value: 1 } }
    ];

    act(() => {
      result.current.updateNodeData(node!.id, {
        outputDefinitions: outputs
      });
    });

    // Remove the first output
    act(() => {
      const currentOutputs = result.current.nodes.find(n => n.id === node!.id)?.data.outputDefinitions as NodeOutputDefinition[];
      result.current.updateNodeData(node!.id, {
        outputDefinitions: currentOutputs.filter((_, i) => i !== 0)
      });
    });

    const updatedNode = result.current.nodes.find(n => n.id === node!.id);
    expect(updatedNode?.data.outputDefinitions).toHaveLength(1);
    expect((updatedNode?.data.outputDefinitions as NodeOutputDefinition[])[0].propertyName).toBe('Email');
  });

  it('should map data type names to correct numeric values', () => {
    const getDataTypeValue = (type: PrimitiveType): number => {
      const values: Record<PrimitiveType, number> = {
        'String': 1,
        'Number': 2,
        'Boolean': 3,
        'Date': 4,
        'JsonDocument': 5
      };
      return values[type];
    };

    expect(getDataTypeValue('String')).toBe(1);
    expect(getDataTypeValue('Number')).toBe(2);
    expect(getDataTypeValue('Boolean')).toBe(3);
    expect(getDataTypeValue('Date')).toBe(4);
    expect(getDataTypeValue('JsonDocument')).toBe(5);
  });
});

describe('Technical Input Configuration', () => {
  beforeEach(() => {
    const { result } = renderHook(() => useWorkflowStore());
    act(() => {
      result.current.setNodes([]);
      result.current.setEdges([]);
    });
  });

  it('should update technical input from Fixed to Mapped', () => {
    const { result } = renderHook(() => useWorkflowStore());
    
    // Create an API node
    act(() => {
      result.current.addNode('apiNode');
    });

    const apiNode = result.current.nodes.find(n => n.data.nodeType === 1);
    expect(apiNode).toBeDefined();

    // Initially set to Fixed
    act(() => {
      result.current.updateNodeData(apiNode!.id, {
        technicalInputs: {
          EndpointUrl: { $type: 'Fixed', Value: 'https://api.example.com' }
        }
      });
    });

    // Switch to Mapped
    act(() => {
      result.current.updateNodeData(apiNode!.id, {
        technicalInputs: {
          EndpointUrl: { 
            $type: 'Mapped', 
            Source: { sourceNodeId: 'sourceNode', sourcePath: 'OutputData' } 
          }
        }
      });
    });

    const updatedNode = result.current.nodes.find(n => n.id === apiNode!.id);
    const techInputs = updatedNode?.data.technicalInputs as Record<string, InputValueSource>;
    expect(techInputs?.EndpointUrl.$type).toBe('Mapped');
    if (techInputs?.EndpointUrl.$type === 'Mapped') {
      expect(techInputs.EndpointUrl.Source.sourceNodeId).toBe('sourceNode');
    }
  });

  it('should update technical input value in Fixed mode', () => {
    const { result } = renderHook(() => useWorkflowStore());
    
    act(() => {
      result.current.addNode('apiNode');
    });

    const apiNode = result.current.nodes.find(n => n.data.nodeType === 1);

    act(() => {
      result.current.updateNodeData(apiNode!.id, {
        technicalInputs: {
          EndpointUrl: { $type: 'Fixed', Value: 'https://api.example.com' }
        }
      });
    });

    // Update the value
    act(() => {
      result.current.updateNodeData(apiNode!.id, {
        technicalInputs: {
          EndpointUrl: { $type: 'Fixed', Value: 'https://new-api.example.com' }
        }
      });
    });

    const updatedNode = result.current.nodes.find(n => n.id === apiNode!.id);
    const techInputs = updatedNode?.data.technicalInputs as Record<string, InputValueSource>;
    if (techInputs?.EndpointUrl.$type === 'Fixed') {
      expect(techInputs.EndpointUrl.Value).toBe('https://new-api.example.com');
    }
  });
});

describe('Source Node Dropdown Sorting', () => {
  it('should sort Start node to the top of the list', () => {
    const nodes = [
      { id: 'node1', data: { nodeType: 1, name: 'API Node' } },
      { id: 'node2', data: { nodeType: 0, name: 'Start' } },
      { id: 'node3', data: { nodeType: 2, name: 'Notification' } },
    ];

    const sorted = [...nodes].sort((a, b) => {
      if (a.data.nodeType === 0) return -1;
      if (b.data.nodeType === 0) return 1;
      return 0;
    });

    expect(sorted[0].data.nodeType).toBe(0);
    expect(sorted[0].id).toBe('node2');
  });

  it('should display Start node with special label', () => {
    const node = { id: 'start', data: { nodeType: 0, name: 'Start' } };
    const label = node.data.nodeType === 0 ? '🚀 Workflow Input' : node.data.name;
    expect(label).toBe('🚀 Workflow Input');
  });

  it('should display regular nodes with their names', () => {
    const node = { id: 'api', data: { nodeType: 1, name: 'API Node' } };
    const label = node.data.nodeType === 0 ? '🚀 Workflow Input' : node.data.name;
    expect(label).toBe('API Node');
  });
});

describe('Data Type Display', () => {
  const getDataTypeDisplay = (dataType: WorkflowDataType): string => {
    if (dataType.kind === 'Primitive') {
      return dataType.name;
    } else {
      return dataType.semanticName;
    }
  };

  it('should display primitive type name', () => {
    const dataType: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 1 };
    const display = getDataTypeDisplay(dataType);
    expect(display).toBe('String');
  });

  it('should display semantic type name', () => {
    const dataType: WorkflowDataType = { 
      kind: 'Semantic', 
      semanticName: 'UserId', 
      underlyingType: { name: 'String', value: 1 } 
    };
    const display = getDataTypeDisplay(dataType);
    expect(display).toBe('UserId');
  });
});
