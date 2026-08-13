import { describe, it, expect } from 'vitest';
import type { Edge } from '@xyflow/react';

describe('isValidConnection (port validation)', () => {
  const isValidConnection = (connection: any, edges: Edge[]) => {
    const sourceHandle = connection.sourceHandle || 'Default';
    
    const existingEdge = edges.find(
      (edge) => 
        edge.source === connection.source && 
        edge.sourceHandle === sourceHandle
    );
    
    return !existingEdge;
  };

  it('should allow first connection to a port', () => {
    const edges: Edge[] = [];
    const connection = { source: 'node1', sourceHandle: 'Default', target: 'node2' };
    expect(isValidConnection(connection, edges)).toBe(true);
  });

  it('should block duplicate connection to same port', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'node1', sourceHandle: 'Default', target: 'node2' }
    ];
    const connection = { source: 'node1', sourceHandle: 'Default', target: 'node3' };
    expect(isValidConnection(connection, edges)).toBe(false);
  });

  it('should allow different ports on same node', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'node1', sourceHandle: 'True', target: 'node2' }
    ];
    const connection = { source: 'node1', sourceHandle: 'False', target: 'node3' };
    expect(isValidConnection(connection, edges)).toBe(true);
  });

  it('should allow connection to different node with same port name', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'node1', sourceHandle: 'Default', target: 'node2' }
    ];
    const connection = { source: 'node2', sourceHandle: 'Default', target: 'node3' };
    expect(isValidConnection(connection, edges)).toBe(true);
  });

  it('should default null sourceHandle to "Default" for validation', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'node1', sourceHandle: 'Default', target: 'node2' }
    ];
    const connection = { source: 'node1', sourceHandle: null, target: 'node3' };
    expect(isValidConnection(connection, edges)).toBe(false);
  });
});

describe('onConnect SourcePort handling', () => {
  const onConnect = (connection: any) => {
    const edgeWithSourcePort = {
      ...connection,
      sourceHandle: connection.sourceHandle || 'Default',
      data: {
        sourcePort: connection.sourceHandle || 'Default'
      }
    };
    return edgeWithSourcePort;
  };

  it('should default null sourceHandle to "Default"', () => {
    const connection = { source: 'n1', target: 'n2', sourceHandle: null };
    const result = onConnect(connection);
    expect(result.sourceHandle).toBe('Default');
    expect(result.data.sourcePort).toBe('Default');
  });

  it('should preserve explicit sourceHandle', () => {
    const connection = { source: 'n1', target: 'n2', sourceHandle: 'True' };
    const result = onConnect(connection);
    expect(result.sourceHandle).toBe('True');
    expect(result.data.sourcePort).toBe('True');
  });

  it('should preserve undefined sourceHandle as "Default"', () => {
    const connection = { source: 'n1', target: 'n2', sourceHandle: undefined };
    const result = onConnect(connection);
    expect(result.sourceHandle).toBe('Default');
    expect(result.data.sourcePort).toBe('Default');
  });

  it('should preserve other connection properties', () => {
    const connection = { 
      source: 'n1', 
      target: 'n2', 
      sourceHandle: 'False',
      id: 'custom-id',
      label: 'test-label'
    };
    const result = onConnect(connection);
    expect(result.source).toBe('n1');
    expect(result.target).toBe('n2');
    expect(result.id).toBe('custom-id');
    expect(result.label).toBe('test-label');
  });
});

describe('loadWorkflow SourcePort mapping', () => {
  const mapTransitionsToEdges = (transitions: any[]) => {
    return transitions.map((transition: any) => ({
      id: `edge-${transition.sourceNodeId}-${transition.targetNodeId}`,
      source: transition.sourceNodeId,
      target: transition.targetNodeId,
      sourceHandle: transition.sourcePort || 'Default',
      data: {
        sourcePort: transition.sourcePort || 'Default'
      }
    }));
  };

  it('should map backend sourcePort to edge sourceHandle', () => {
    const workflowData = {
      transitions: [
        { sourceNodeId: 'n1', targetNodeId: 'n2', sourcePort: 'True' }
      ]
    };
    const edges = mapTransitionsToEdges(workflowData.transitions);
    expect(edges[0].sourceHandle).toBe('True');
    expect(edges[0].data.sourcePort).toBe('True');
  });

  it('should default missing sourcePort to "Default"', () => {
    const workflowData = {
      transitions: [
        { sourceNodeId: 'n1', targetNodeId: 'n2' }
      ]
    };
    const edges = mapTransitionsToEdges(workflowData.transitions);
    expect(edges[0].sourceHandle).toBe('Default');
    expect(edges[0].data.sourcePort).toBe('Default');
  });

  it('should handle empty sourcePort string as "Default"', () => {
    const workflowData = {
      transitions: [
        { sourceNodeId: 'n1', targetNodeId: 'n2', sourcePort: '' }
      ]
    };
    const edges = mapTransitionsToEdges(workflowData.transitions);
    expect(edges[0].sourceHandle).toBe('Default');
  });

  it('should map multiple transitions correctly', () => {
    const workflowData = {
      transitions: [
        { sourceNodeId: 'n1', targetNodeId: 'n2', sourcePort: 'True' },
        { sourceNodeId: 'n1', targetNodeId: 'n3', sourcePort: 'False' },
        { sourceNodeId: 'n2', targetNodeId: 'n4' }
      ]
    };
    const edges = mapTransitionsToEdges(workflowData.transitions);
    expect(edges[0].sourceHandle).toBe('True');
    expect(edges[1].sourceHandle).toBe('False');
    expect(edges[2].sourceHandle).toBe('Default');
  });
});
