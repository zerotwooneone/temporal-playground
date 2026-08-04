import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useWorkflowStore } from '../../../store/workflowStore';
import { X } from 'lucide-react';

// API Node Schema
const apiNodeSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  endpointUrl: z.string().url('Invalid URL').optional().or(z.literal('')),
  retryPolicyMaxAttempts: z.number().min(1).max(10).optional(),
  businessNotes: z.string().optional(),
});

// Notification Node Schema
const notificationNodeSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  messageTemplate: z.string().min(1, 'Message template is required'),
  businessNotes: z.string().optional(),
});

type ApiNodeFormData = z.infer<typeof apiNodeSchema>;
type NotificationNodeFormData = z.infer<typeof notificationNodeSchema>;

export default function NodePropertiesPanel() {
  const { selectedNodeId, nodes, updateNodeData, setSelectedNodeId } = useWorkflowStore();
  const selectedNode = nodes.find((n) => n.id === selectedNodeId);

  const nodeType = selectedNode?.data.nodeType as number | undefined;

  const handleClose = () => setSelectedNodeId(null);

  // API Node Form
  const apiForm = useForm<ApiNodeFormData>({
    resolver: zodResolver(apiNodeSchema),
    defaultValues: {
      name: (selectedNode?.data.name as string) || '',
      endpointUrl: (selectedNode?.data.endpointUrl as string) || '',
      retryPolicyMaxAttempts: (selectedNode?.data.retryPolicyMaxAttempts as number) || 3,
      businessNotes: (selectedNode?.data.businessNotes as string) || '',
    },
  });

  const apiOnSubmit = (data: ApiNodeFormData) => {
    if (selectedNodeId) {
      updateNodeData(selectedNodeId, { ...data, isConfigured: true });
    }
  };

  // Notification Node Form
  const notificationForm = useForm<NotificationNodeFormData>({
    resolver: zodResolver(notificationNodeSchema),
    defaultValues: {
      name: (selectedNode?.data.name as string) || '',
      messageTemplate: (selectedNode?.data.messageTemplate as string) || '',
      businessNotes: (selectedNode?.data.businessNotes as string) || '',
    },
  });

  const notificationOnSubmit = (data: NotificationNodeFormData) => {
    if (selectedNodeId) {
      updateNodeData(selectedNodeId, { ...data, isConfigured: true });
    }
  };

  if (!selectedNode || !selectedNodeId) return null;

  // API Node Form
  if (nodeType === 1) {
    const {
      register,
      handleSubmit,
      formState: { errors },
    } = apiForm;

    return (
      <div className="fixed right-0 top-0 h-full w-96 bg-white border-l border-gray-200 shadow-xl z-10 overflow-y-auto">
        <div className="p-4 border-b border-gray-200 flex items-center justify-between">
          <h2 className="text-lg font-semibold text-gray-900">API Node Properties</h2>
          <button
            onClick={handleClose}
            className="p-1 hover:bg-gray-100 rounded-md transition-colors"
          >
            <X size={20} className="text-gray-500" />
          </button>
        </div>

        <form onSubmit={handleSubmit(apiOnSubmit)} className="p-4 space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
            <input
              {...register('name')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="API Task Name"
            />
            {errors.name && (
              <p className="text-sm text-red-600 mt-1">{errors.name.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Endpoint URL</label>
            <input
              {...register('endpointUrl')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="https://api.example.com"
            />
            {errors.endpointUrl && (
              <p className="text-sm text-red-600 mt-1">{errors.endpointUrl.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Retry Max Attempts (1-10)
            </label>
            <input
              type="number"
              {...register('retryPolicyMaxAttempts', { valueAsNumber: true })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              min={1}
              max={10}
            />
            {errors.retryPolicyMaxAttempts && (
              <p className="text-sm text-red-600 mt-1">{errors.retryPolicyMaxAttempts.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Business Notes</label>
            <textarea
              {...register('businessNotes')}
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Optional business context..."
            />
          </div>

          <button
            type="submit"
            className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 transition-colors font-medium"
          >
            Save Changes
          </button>
        </form>
      </div>
    );
  }

  // Notification Node Form
  if (nodeType === 2) {
    const {
      register,
      handleSubmit,
      formState: { errors },
    } = notificationForm;

    return (
      <div className="fixed right-0 top-0 h-full w-96 bg-white border-l border-gray-200 shadow-xl z-10 overflow-y-auto">
        <div className="p-4 border-b border-gray-200 flex items-center justify-between">
          <h2 className="text-lg font-semibold text-gray-900">Notification Node Properties</h2>
          <button
            onClick={handleClose}
            className="p-1 hover:bg-gray-100 rounded-md transition-colors"
          >
            <X size={20} className="text-gray-500" />
          </button>
        </div>

        <form onSubmit={handleSubmit(notificationOnSubmit)} className="p-4 space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
            <input
              {...register('name')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
              placeholder="Notification Name"
            />
            {errors.name && (
              <p className="text-sm text-red-600 mt-1">{errors.name.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Message Template</label>
            <textarea
              {...register('messageTemplate')}
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
              placeholder="Hello {user}, your request has been processed..."
            />
            {errors.messageTemplate && (
              <p className="text-sm text-red-600 mt-1">{errors.messageTemplate.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Business Notes</label>
            <textarea
              {...register('businessNotes')}
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500"
              placeholder="Optional business context..."
            />
          </div>

          <button
            type="submit"
            className="w-full bg-purple-600 text-white py-2 px-4 rounded-md hover:bg-purple-700 transition-colors font-medium"
          >
            Save Changes
          </button>
        </form>
      </div>
    );
  }

  return null;
}
