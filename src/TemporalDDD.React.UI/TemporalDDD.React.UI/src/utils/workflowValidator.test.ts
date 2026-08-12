import { describe, it, expect } from 'vitest';
import { isAssignableTo, validateParameterBinding, detectCycle } from './workflowValidator';
import type { WorkflowDataType } from '../types/workflowTypes';
import type { Node, Edge } from '@xyflow/react';

describe('isAssignableTo', () => {
  it('should return true for matching primitive types', () => {
    const source: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 0 };
    const target: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 0 };
    expect(isAssignableTo(source, target)).toBe(true);
  });

  it('should return false for mismatched primitive types', () => {
    const source: WorkflowDataType = { kind: 'Primitive', name: 'Number', value: 1 };
    const target: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 0 };
    expect(isAssignableTo(source, target)).toBe(false);
  });

  it('should return true for semantic types with matching underlying types', () => {
    const source: WorkflowDataType = { 
      kind: 'Semantic', 
      semanticName: 'UserId', 
      underlyingType: { name: 'String', value: 0 } 
    };
    const target: WorkflowDataType = { 
      kind: 'Semantic', 
      semanticName: 'Email', 
      underlyingType: { name: 'String', value: 0 } 
    };
    expect(isAssignableTo(source, target)).toBe(true);
  });

  it('should return false for semantic types with mismatched underlying types', () => {
    const source: WorkflowDataType = { 
      kind: 'Semantic', 
      semanticName: 'UserId', 
      underlyingType: { name: 'String', value: 0 } 
    };
    const target: WorkflowDataType = { 
      kind: 'Semantic', 
      semanticName: 'Age', 
      underlyingType: { name: 'Number', value: 1 } 
    };
    expect(isAssignableTo(source, target)).toBe(false);
  });

  it('should return true for primitive to semantic with matching underlying type', () => {
    const source: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 0 };
    const target: WorkflowDataType = { 
      kind: 'Semantic', 
      semanticName: 'UserId', 
      underlyingType: { name: 'String', value: 0 } 
    };
    expect(isAssignableTo(source, target)).toBe(true);
  });
});

describe('validateParameterBinding', () => {
  const edges: Edge[] = [
    { id: 'e1', source: 'node1', target: 'node2' },
    { id: 'e2', source: 'node2', target: 'node3' },
  ];

  it('should pass for valid type and scope', () => {
    const sourceDataType: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 0 };
    const targetDataType: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 0 };
    
    const result = validateParameterBinding('node1', 'node2', sourceDataType, targetDataType, edges);
    expect(result.isValid).toBe(true);
    expect(result.error).toBeUndefined();
  });

  it('should fail for type mismatch', () => {
    const sourceDataType: WorkflowDataType = { kind: 'Primitive', name: 'Number', value: 1 };
    const targetDataType: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 0 };
    
    const result = validateParameterBinding('node1', 'node2', sourceDataType, targetDataType, edges);
    expect(result.isValid).toBe(false);
    expect(result.error).toContain('Type mismatch');
  });

  it('should fail for scope violation (non-ancestor)', () => {
    const sourceDataType: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 0 };
    const targetDataType: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 0 };
    
    const result = validateParameterBinding('node3', 'node1', sourceDataType, targetDataType, edges);
    expect(result.isValid).toBe(false);
    expect(result.error).toContain('Scope violation');
  });

  it('should fail for scope violation (parallel branch)', () => {
    const parallelEdges: Edge[] = [
      { id: 'e1', source: 'start', target: 'branchA' },
      { id: 'e2', source: 'start', target: 'branchB' },
    ];

    const sourceDataType: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 0 };
    const targetDataType: WorkflowDataType = { kind: 'Primitive', name: 'String', value: 0 };
    
    const result = validateParameterBinding('branchB', 'branchA', sourceDataType, targetDataType, parallelEdges);
    expect(result.isValid).toBe(false);
    expect(result.error).toContain('Scope violation');
  });
});

describe('detectCycle', () => {
  it('should pass for acyclic graph', () => {
    const nodes: Node[] = [
      { id: 'node1', type: 'apiNode', position: { x: 0, y: 0 }, data: {} },
      { id: 'node2', type: 'apiNode', position: { x: 100, y: 0 }, data: {} },
      { id: 'node3', type: 'apiNode', position: { x: 200, y: 0 }, data: {} },
    ];

    const edges: Edge[] = [
      { id: 'e1', source: 'node1', target: 'node2' },
      { id: 'e2', source: 'node2', target: 'node3' },
    ];

    const result = detectCycle(nodes, edges);
    expect(result.isValid).toBe(true);
    expect(result.error).toBeUndefined();
  });

  it('should fail for cyclic graph', () => {
    const nodes: Node[] = [
      { id: 'node1', type: 'apiNode', position: { x: 0, y: 0 }, data: {} },
      { id: 'node2', type: 'apiNode', position: { x: 100, y: 0 }, data: {} },
      { id: 'node3', type: 'apiNode', position: { x: 200, y: 0 }, data: {} },
    ];

    const edges: Edge[] = [
      { id: 'e1', source: 'node1', target: 'node2' },
      { id: 'e2', source: 'node2', target: 'node3' },
      { id: 'e3', source: 'node3', target: 'node1' }, // Creates cycle
    ];

    const result = detectCycle(nodes, edges);
    expect(result.isValid).toBe(false);
    expect(result.error).toContain('cycle');
  });

  it('should fail for self-loop', () => {
    const nodes: Node[] = [
      { id: 'node1', type: 'apiNode', position: { x: 0, y: 0 }, data: {} },
    ];

    const edges: Edge[] = [
      { id: 'e1', source: 'node1', target: 'node1' }, // Self-loop
    ];

    const result = detectCycle(nodes, edges);
    expect(result.isValid).toBe(false);
    expect(result.error).toContain('cycle');
  });
});
