import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { createWorkflow, getWorkflows } from '../../../api/client';

interface Workflow {
  id: string;
  name: string;
  status: string;
  createdAt: string;
}

interface CreateWorkflowResponse {
  workflowId: string;
  message: string;
}

export default function WorkflowDashboard() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [workflowName, setWorkflowName] = useState('');

  const { data: workflows, isLoading, error } = useQuery<Workflow[]>({
    queryKey: ['workflows'],
    queryFn: getWorkflows,
  });

  const createMutation = useMutation({
    mutationFn: ({ name, creatorId }: { name: string; creatorId: string }) =>
      createWorkflow(name, creatorId),
    onSuccess: (data: CreateWorkflowResponse) => {
      queryClient.invalidateQueries({ queryKey: ['workflows'] });
      setIsCreateModalOpen(false);
      setWorkflowName('');
      navigate(`/workflows/${data.workflowId}`);
    },
    onError: (error) => {
      console.error('Failed to create workflow:', error);
    },
  });

  const handleCreateWorkflow = (e: React.FormEvent) => {
    e.preventDefault();
    if (workflowName.trim()) {
      // Generate a valid user ID matching domain validation (USRId + GUID)
      const creatorId = `USRId${crypto.randomUUID()}`;
      createMutation.mutate({ name: workflowName, creatorId });
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-gray-600">Loading workflows...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-red-600">Error loading workflows: {(error as Error).message}</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-6xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Workflows</h1>
          <button
            onClick={() => setIsCreateModalOpen(true)}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
          >
            Create New Workflow
          </button>
        </div>

        {workflows && workflows.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {workflows.map((workflow) => (
              <div
                key={workflow.id}
                onClick={() => navigate(`/workflows/${workflow.id}`)}
                className="bg-white rounded-lg shadow-md p-6 cursor-pointer hover:shadow-lg transition-shadow"
              >
                <h2 className="text-xl font-semibold text-gray-900 mb-2">{workflow.name}</h2>
                <div className="text-sm text-gray-600">
                  <p>ID: {workflow.id}</p>
                  <p>Status: {workflow.status}</p>
                  <p>Created: {new Date(workflow.createdAt).toLocaleDateString()}</p>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-12">
            <p className="text-gray-600 text-lg">No workflows found. Create your first workflow to get started.</p>
          </div>
        )}

        {isCreateModalOpen && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 w-full max-w-md">
              <h2 className="text-2xl font-bold mb-4">Create New Workflow</h2>
              <form onSubmit={handleCreateWorkflow}>
                <div className="mb-4">
                  <label htmlFor="workflowName" className="block text-sm font-medium text-gray-700 mb-2">
                    Workflow Name
                  </label>
                  <input
                    type="text"
                    id="workflowName"
                    value={workflowName}
                    onChange={(e) => setWorkflowName(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="Enter workflow name"
                    required
                  />
                </div>
                <div className="flex justify-end space-x-3">
                  <button
                    type="button"
                    onClick={() => {
                      setIsCreateModalOpen(false);
                      setWorkflowName('');
                    }}
                    className="px-4 py-2 text-gray-700 bg-gray-200 rounded-md hover:bg-gray-300 transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={createMutation.isPending}
                    className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors disabled:opacity-50"
                  >
                    {createMutation.isPending ? 'Creating...' : 'Create'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
