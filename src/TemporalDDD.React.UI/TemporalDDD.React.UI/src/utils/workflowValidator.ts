import type { WorkflowDataType } from '../types/workflowTypes';
import type { Node, Edge } from '@xyflow/react';

export function isAssignableTo(source: WorkflowDataType, target: WorkflowDataType): boolean {
  const sourceType = source.kind === 'Semantic' ? source.underlyingType.name : source.name;
  const targetType = target.kind === 'Semantic' ? target.underlyingType.name : target.name;
  return sourceType === targetType;
}

export interface ValidationResult {
  isValid: boolean;
  error?: string;
}

export function validateParameterBinding(
  sourceNodeId: string,
  targetNodeId: string,
  sourceDataType: WorkflowDataType,
  targetDataType: WorkflowDataType,
  edges: Edge[]
): ValidationResult {
  // Check type assignability
  if (!isAssignableTo(sourceDataType, targetDataType)) {
    return {
      isValid: false,
      error: `Type mismatch: cannot assign ${sourceDataType.kind === 'Semantic' ? sourceDataType.semanticName : sourceDataType.name} to ${targetDataType.kind === 'Semantic' ? targetDataType.semanticName : targetDataType.name}`
    };
  }

  // Check scope - source must be an ancestor of target
  if (!isAncestor(sourceNodeId, targetNodeId, edges)) {
    return {
      isValid: false,
      error: `Scope violation: node '${sourceNodeId}' is not an upstream ancestor of node '${targetNodeId}'`
    };
  }

  return { isValid: true };
}

function isAncestor(sourceNodeId: string, targetNodeId: string, edges: Edge[]): boolean {
  // Build adjacency list for the graph
  const adjacency = new Map<string, string[]>();
  edges.forEach(edge => {
    if (!adjacency.has(edge.source)) {
      adjacency.set(edge.source, []);
    }
    adjacency.get(edge.source)!.push(edge.target);
  });

  // BFS from source to find all reachable nodes
  const visited = new Set<string>();
  const queue = [sourceNodeId];

  while (queue.length > 0) {
    const current = queue.shift()!;
    if (current === targetNodeId) {
      return true;
    }

    if (visited.has(current)) {
      continue;
    }
    visited.add(current);

    const neighbors = adjacency.get(current) || [];
    neighbors.forEach(neighbor => {
      if (!visited.has(neighbor)) {
        queue.push(neighbor);
      }
    });
  }

  return false;
}

export function detectCycle(nodes: Node[], edges: Edge[]): ValidationResult {
  const adjacency = new Map<string, string[]>();
  edges.forEach(edge => {
    if (!adjacency.has(edge.source)) {
      adjacency.set(edge.source, []);
    }
    adjacency.get(edge.source)!.push(edge.target);
  });

  const visited = new Set<string>();
  const recursionStack = new Set<string>();

  function hasCycle(nodeId: string): boolean {
    visited.add(nodeId);
    recursionStack.add(nodeId);

    const neighbors = adjacency.get(nodeId) || [];
    for (const neighbor of neighbors) {
      if (!visited.has(neighbor)) {
        if (hasCycle(neighbor)) {
          return true;
        }
      } else if (recursionStack.has(neighbor)) {
        return true;
      }
    }

    recursionStack.delete(nodeId);
    return false;
  }

  for (const node of nodes) {
    if (!visited.has(node.id)) {
      if (hasCycle(node.id)) {
        return {
          isValid: false,
          error: 'Workflow contains a cycle. Workflows must be acyclic (DAG).'
        };
      }
    }
  }

  return { isValid: true };
}
