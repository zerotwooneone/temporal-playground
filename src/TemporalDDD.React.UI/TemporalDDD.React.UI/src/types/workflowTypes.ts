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
