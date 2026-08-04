import { useWorkflowStore } from '../../../store/workflowStore';
import { Plus, Save } from 'lucide-react';
import { useMutation } from '@tanstack/react-query';
import { apiClient } from '../../../api/client';

export default function WorkflowToolbar() {
  const { addNode, nodes } = useWorkflowStore();

  const saveMutation = useMutation({
    mutationFn: async () => {
      const payload = {
        workflowId: 'workflow-draft-123', // TODO: Get from route or context
        nodes: nodes.map((node) => ({
          id: node.id,
          nodeType: node.data.nodeType,
          name: node.data.name,
          businessNotes: node.data.businessNotes,
          isConfigured: node.data.isConfigured,
          // API Node properties
          endpointUrl: node.data.endpointUrl,
          authToken: node.data.authToken,
          retryPolicyMaxAttempts: node.data.retryPolicyMaxAttempts,
          retryPolicyBackoffCoefficient: node.data.retryPolicyBackoffCoefficient,
          contractMappingConvertXmlToJson: node.data.contractMappingConvertXmlToJson,
          contractMappingQueryParameters: node.data.contractMappingQueryParameters,
          contractMappingRequestMapping: node.data.contractMappingRequestMapping,
          contractMappingResponseMapping: node.data.contractMappingResponseMapping,
          // Notification Node properties
          messageTemplate: node.data.messageTemplate,
        })),
      };

      const response = await apiClient.put('/workflows/workflow-draft-123/nodes', payload);
      return response.data;
    },
    onSuccess: () => {
      alert('Workflow saved successfully!');
    },
    onError: (error) => {
      console.error('Failed to save workflow:', error);
      alert('Failed to save workflow. Please try again.');
    },
  });

  return (
    <div className="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between shadow-sm">
      <div className="flex items-center gap-4">
        <h1 className="text-lg font-semibold text-gray-900">Workflow Designer</h1>
        <span className="text-sm text-gray-500">ID: workflow-draft-123</span>
      </div>

      <div className="flex items-center gap-2">
        <button
          onClick={() => addNode('apiNode')}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors font-medium"
        >
          <Plus size={16} />
          Add API Task
        </button>

        <button
          onClick={() => addNode('notificationNode')}
          className="flex items-center gap-2 px-4 py-2 bg-purple-600 text-white rounded-md hover:bg-purple-700 transition-colors font-medium"
        >
          <Plus size={16} />
          Add Notification
        </button>

        <button
          onClick={() => saveMutation.mutate()}
          disabled={saveMutation.isPending}
          className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <Save size={16} />
          {saveMutation.isPending ? 'Saving...' : 'Save Workflow'}
        </button>
      </div>
    </div>
  );
}
