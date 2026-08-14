import { useWorkflowStore } from '../../../store/workflowStore';
import { Plus, Save, ChevronDown, Check, Send, Rocket } from 'lucide-react';
import { useMutation } from '@tanstack/react-query';
import { apiClient, approveWorkflow, submitWorkflowForReview, publishWorkflow } from '../../../api/client';
import { useState } from 'react';
import { useUser } from '../../../contexts/UserContext';

interface WorkflowToolbarProps {
  publicId: string;
}

export default function WorkflowToolbar({ publicId }: WorkflowToolbarProps) {
  const { addNode, nodes, edges } = useWorkflowStore();
  const { role } = useUser();
  const [isToolPanelOpen, setIsToolPanelOpen] = useState(false);

  const saveMutation = useMutation({
    mutationFn: async () => {
      const flowJson = JSON.stringify({ nodes, edges });

      const payload = {
        flowJson,
        nodes: nodes.map((node) => ({
          id: node.id,
          nodeType: node.data.nodeType,
          name: node.data.name,
          businessNotes: node.data.businessNotes,
          isConfigured: node.data.isConfigured,
          technicalInputs: node.data.technicalInputs,
          // API Node value object properties
          retryPolicyMaxAttempts: node.data.retryPolicyMaxAttempts,
          retryPolicyBackoffCoefficient: node.data.retryPolicyBackoffCoefficient,
          contractMappingConvertXmlToJson: node.data.contractMappingConvertXmlToJson,
          contractMappingQueryParameters: node.data.contractMappingQueryParameters,
          contractMappingRequestMapping: node.data.contractMappingRequestMapping,
          contractMappingResponseMapping: node.data.contractMappingResponseMapping,
          // Notification Node property
          messageTemplate: node.data.messageTemplate,
        })),
        transitions: edges.map((edge) => ({
          sourceNodeId: edge.source,
          targetNodeId: edge.target,
          sourcePort: edge.sourceHandle || edge.data?.sourcePort || 'Default',
        })),
      };

      const response = await apiClient.put(`/workflows/${publicId}/nodes`, payload);
      return response.data;
    },
    onSuccess: () => {},
    onError: (error) => {
      console.error('Failed to save workflow:', error);
      alert('Failed to save workflow. Please try again.');
    },
  });

  const approveMutation = useMutation({
    mutationFn: async () => {
      // Generate a valid reviewer ID matching domain validation (USRId + GUID)
      const reviewerId = `USRId${crypto.randomUUID()}`;
      return await approveWorkflow(publicId, reviewerId);
    },
    onSuccess: () => {
      alert('Workflow approved successfully!');
    },
    onError: (error) => {
      console.error('Failed to approve workflow:', error);
      alert('Failed to approve workflow. Please try again.');
    },
  });

  const submitMutation = useMutation({
    mutationFn: async () => {
      return await submitWorkflowForReview(publicId);
    },
    onSuccess: () => {
      alert('Workflow submitted for review successfully!');
    },
    onError: (error) => {
      console.error('Failed to submit workflow for review:', error);
      alert('Failed to submit workflow for review. Please try again.');
    },
  });

  const publishMutation = useMutation({
    mutationFn: async () => {
      return await publishWorkflow(publicId);
    },
    onSuccess: () => {
      alert('Workflow published successfully!');
    },
    onError: (error) => {
      console.error('Failed to publish workflow:', error);
      alert('Failed to publish workflow. Please try again.');
    },
  });

  return (
    <div className="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between shadow-sm">
      <div className="flex items-center gap-4">
        <h1 className="text-lg font-semibold text-gray-900">Workflow Designer</h1>
        <span className="text-sm text-gray-500">ID: {publicId}</span>
      </div>

      <div className="flex items-center gap-2">
        {role === 'designer' && (
          <>
            {/* Tool Panel */}
            <div className="relative">
              <button
                onClick={() => setIsToolPanelOpen(!isToolPanelOpen)}
                className="flex items-center gap-2 px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 transition-colors font-medium"
              >
                <Plus size={16} />
                Add Node
                <ChevronDown size={16} className={isToolPanelOpen ? 'rotate-180' : ''} />
              </button>

              {isToolPanelOpen && (
                <div className="absolute right-0 mt-2 w-56 bg-white rounded-md shadow-lg border border-gray-200 z-10">
                  <div className="py-1">
                    <button
                      onClick={() => {
                        addNode('apiNode');
                        setIsToolPanelOpen(false);
                      }}
                      className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 flex items-center gap-2"
                    >
                      <div className="w-3 h-3 bg-blue-500 rounded-full" />
                      API Task
                    </button>
                    <button
                      onClick={() => {
                        addNode('notificationNode');
                        setIsToolPanelOpen(false);
                      }}
                      className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 flex items-center gap-2"
                    >
                      <div className="w-3 h-3 bg-purple-500 rounded-full" />
                      Notification
                    </button>
                    <button
                      onClick={() => {
                        addNode('decisionNode');
                        setIsToolPanelOpen(false);
                      }}
                      className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 flex items-center gap-2"
                    >
                      <div className="w-3 h-3 bg-orange-500 rounded-full" />
                      Decision
                    </button>
                    <button
                      onClick={() => {
                        addNode('humanTaskNode');
                        setIsToolPanelOpen(false);
                      }}
                      className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 flex items-center gap-2"
                    >
                      <div className="w-3 h-3 bg-yellow-500 rounded-full" />
                      Human Task
                    </button>
                  </div>
                </div>
              )}
            </div>

            <button
              onClick={() => saveMutation.mutate()}
              disabled={saveMutation.isPending}
              className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Save size={16} />
              {saveMutation.isPending ? 'Saving...' : 'Save Workflow'}
            </button>

            <button
              onClick={() => submitMutation.mutate()}
              disabled={submitMutation.isPending}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Send size={16} />
              {submitMutation.isPending ? 'Submitting...' : 'Submit for Review'}
            </button>
          </>
        )}

        {role === 'approver' && (
          <>
            <button
              onClick={() => approveMutation.mutate()}
              disabled={approveMutation.isPending}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Check size={16} />
              {approveMutation.isPending ? 'Approving...' : 'Approve'}
            </button>
            <button
              onClick={() => publishMutation.mutate()}
              disabled={publishMutation.isPending}
              className="flex items-center gap-2 px-4 py-2 bg-purple-600 text-white rounded-md hover:bg-purple-700 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Rocket size={16} />
              {publishMutation.isPending ? 'Publishing...' : 'Publish'}
            </button>
          </>
        )}
      </div>
    </div>
  );
}
