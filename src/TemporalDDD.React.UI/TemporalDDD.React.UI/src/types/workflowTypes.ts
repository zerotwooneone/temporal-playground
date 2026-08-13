export type PrimitiveType = 'String' | 'Number' | 'Boolean' | 'Date' | 'JsonDocument';

export type WorkflowDataType = 
  | { kind: 'Primitive'; name: PrimitiveType; value: number }
  | { kind: 'Semantic'; semanticName: string; underlyingType: { name: PrimitiveType; value: number } };

export interface NodeInputDefinition {
  propertyName: string;
  dataType: WorkflowDataType;
  isRequired: boolean;
}

export interface NodeOutputDefinition {
  propertyName: string;
  dataType: WorkflowDataType;
}

export interface VariableReference {
  sourceNodeId: string;
  sourcePath: string;
}

export interface ParameterBinding {
  targetInputProperty: string;
  source: VariableReference;
}

export type InputValueSource =
  | { $type: 'Fixed'; Value: string }
  | { $type: 'Mapped'; Source: VariableReference };

export interface NodeData {
  name: string;
  nodeType: number;
  businessNotes?: string;
  isConfigured: boolean;
  technicalInputs: Record<string, InputValueSource>;
  inputDefinitions: NodeInputDefinition[];
  outputDefinitions: NodeOutputDefinition[];
  inputBindings?: ParameterBinding[];
  // Value object properties for API nodes
  retryPolicyMaxAttempts?: number;
  retryPolicyBackoffCoefficient?: number;
  contractMappingConvertXmlToJson?: boolean;
  contractMappingQueryParameters?: string;
  contractMappingRequestMapping?: string;
  contractMappingResponseMapping?: string;
}

export interface WorkflowTransitionDto {
  sourceNodeId: string;
  targetNodeId: string;
  sourcePort?: string;
}
