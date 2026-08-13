import { describe, it, expect } from 'vitest';
import type { Edge } from '@xyflow/react';

describe('saveWorkflow SourcePort DTO mapping', () => {
  const mapEdgesToTransitions = (edges: Edge[]) => {
    return edges.map((edge) => ({
      sourceNodeId: edge.source,
      targetNodeId: edge.target,
      sourcePort: edge.sourceHandle || edge.data?.sourcePort || 'Default',
    }));
  };

  it('should include sourcePort in transition DTO', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'n1', target: 'n2', sourceHandle: 'True' }
    ];
    const transitions = mapEdgesToTransitions(edges);
    expect(transitions[0].sourcePort).toBe('True');
    expect(transitions[0].sourceNodeId).toBe('n1');
    expect(transitions[0].targetNodeId).toBe('n2');
  });

  it('should default missing sourceHandle to "Default"', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'n1', target: 'n2' }
    ];
    const transitions = mapEdgesToTransitions(edges);
    expect(transitions[0].sourcePort).toBe('Default');
  });

  it('should use data.sourcePort as fallback when sourceHandle is missing', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'n1', target: 'n2', data: { sourcePort: 'False' } }
    ];
    const transitions = mapEdgesToTransitions(edges);
    expect(transitions[0].sourcePort).toBe('False');
  });

  it('should prefer sourceHandle over data.sourcePort when both exist', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'n1', target: 'n2', sourceHandle: 'True', data: { sourcePort: 'False' } }
    ];
    const transitions = mapEdgesToTransitions(edges);
    expect(transitions[0].sourcePort).toBe('True');
  });

  it('should handle multiple edges with different ports', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'n1', target: 'n2', sourceHandle: 'True' },
      { id: 'e2', source: 'n1', target: 'n3', sourceHandle: 'False' },
      { id: 'e3', source: 'n2', target: 'n4' }
    ];
    const transitions = mapEdgesToTransitions(edges);
    expect(transitions[0].sourcePort).toBe('True');
    expect(transitions[1].sourcePort).toBe('False');
    expect(transitions[2].sourcePort).toBe('Default');
  });

  it('should handle empty sourceHandle string as "Default"', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'n1', target: 'n2', sourceHandle: '' }
    ];
    const transitions = mapEdgesToTransitions(edges);
    expect(transitions[0].sourcePort).toBe('Default');
  });

  it('should handle null sourceHandle as "Default"', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'n1', target: 'n2', sourceHandle: null }
    ];
    const transitions = mapEdgesToTransitions(edges);
    expect(transitions[0].sourcePort).toBe('Default');
  });

  it('should handle undefined sourceHandle as "Default"', () => {
    const edges: Edge[] = [
      { id: 'e1', source: 'n1', target: 'n2', sourceHandle: undefined }
    ];
    const transitions = mapEdgesToTransitions(edges);
    expect(transitions[0].sourcePort).toBe('Default');
  });
});
