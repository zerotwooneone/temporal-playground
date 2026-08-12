import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useWorkflowStore } from '../../../store/workflowStore';
import { X, Plus, Trash2 } from 'lucide-react';
import { useEffect, useState } from 'react';
import type { ParameterBinding } from '../../../types/workflowTypes';

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

// Start/End Node Schema
const basicNodeSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  businessNotes: z.string().optional(),
});

type ApiNodeFormData = z.infer<typeof apiNodeSchema>;
type NotificationNodeFormData = z.infer<typeof notificationNodeSchema>;
type BasicNodeFormData = z.infer<typeof basicNodeSchema>;

export default function NodePropertiesPanel() {
  const { selectedNodeIds, nodes, updateNodeData, updateNodeInputBindings, setSelectedNodeIds } = useWorkflowStore();
  const selectedNodeId = selectedNodeIds[0] || null;
  const selectedNode = nodes.find((n) => n.id === selectedNodeId);

  const nodeType = selectedNode?.data.nodeType as number | undefined;
  const [inputBindings, setInputBindings] = useState<ParameterBinding[]>(
    (selectedNode?.data.inputBindings as ParameterBinding[]) || []
  );

  const handleClose = () => setSelectedNodeIds([]);

  // API Node Form
  const apiForm = useForm<ApiNodeFormData>({
    resolver: zodResolver(apiNodeSchema),
    defaultValues: {
      name: '',
      endpointUrl: '',
      retryPolicyMaxAttempts: 3,
      businessNotes: '',
    },
  });

  // Reset form when selected node changes
  useEffect(() => {
    if (selectedNode && nodeType === 1) {
      apiForm.reset({
        name: (selectedNode.data.name as string) || '',
        endpointUrl: (selectedNode.data.endpointUrl as string) || '',
        retryPolicyMaxAttempts: (selectedNode.data.retryPolicyMaxAttempts as number) || 3,
        businessNotes: (selectedNode.data.businessNotes as string) || '',
      });
    }
  }, [selectedNode, nodeType, apiForm]);

  const apiOnSubmit = (data: ApiNodeFormData) => {
    if (selectedNodeId) {
      updateNodeData(selectedNodeId, { ...data, isConfigured: true });
    }
  };

  // Notification Node Form
  const notificationForm = useForm<NotificationNodeFormData>({
    resolver: zodResolver(notificationNodeSchema),
    defaultValues: {
      name: '',
      messageTemplate: '',
      businessNotes: '',
    },
  });

  // Reset form when selected node changes
  useEffect(() => {
    if (selectedNode && nodeType === 2) {
      notificationForm.reset({
        name: (selectedNode.data.name as string) || '',
        messageTemplate: (selectedNode.data.messageTemplate as string) || '',
        businessNotes: (selectedNode.data.businessNotes as string) || '',
      });
    }
  }, [selectedNode, nodeType, notificationForm]);

  const notificationOnSubmit = (data: NotificationNodeFormData) => {
    if (selectedNodeId) {
      updateNodeData(selectedNodeId, { ...data, isConfigured: true });
    }
  };

  const addInputBinding = () => {
    const newBinding: ParameterBinding = {
      targetInputProperty: '',
      source: { sourceNodeId: '', sourcePath: '' }
    };
    setInputBindings([...inputBindings, newBinding]);
  };

  const removeInputBinding = (index: number) => {
    const updated = inputBindings.filter((_, i) => i !== index);
    setInputBindings(updated);
    if (selectedNodeId) {
      updateNodeInputBindings(selectedNodeId, updated);
    }
  };

  const updateInputBinding = (index: number, field: keyof ParameterBinding, value: any) => {
    const updated = [...inputBindings];
    if (field === 'source') {
      updated[index].source = value;
    } else {
      updated[index][field] = value;
    }
    setInputBindings(updated);
    if (selectedNodeId) {
      updateNodeInputBindings(selectedNodeId, updated);
    }
  };

  // Sync inputBindings when selected node changes
  useEffect(() => {
    if (selectedNode) {
      setInputBindings((selectedNode.data.inputBindings as ParameterBinding[]) || []);
    }
  }, [selectedNode]);

  // Start/End Node Form
  const basicForm = useForm<BasicNodeFormData>({
    resolver: zodResolver(basicNodeSchema),
    defaultValues: {
      name: '',
      businessNotes: '',
    },
  });

  // Reset form when selected node changes
  useEffect(() => {
    if (selectedNode && (nodeType === 0 || nodeType === 99)) {
      basicForm.reset({
        name: (selectedNode.data.name as string) || '',
        businessNotes: (selectedNode.data.businessNotes as string) || '',
      });
    }
  }, [selectedNode, nodeType, basicForm]);

  const basicOnSubmit = (data: BasicNodeFormData) => {
    if (selectedNodeId) {
      updateNodeData(selectedNodeId, { ...data, isConfigured: true });
    }
  };

  if (!selectedNode || !selectedNodeId) return null;

  // Start/End Node Form
  if (nodeType === 0 || nodeType === 99) {
    const {
      register,
      handleSubmit,
      formState: { errors },
    } = basicForm;

    const nodeLabel = nodeType === 0 ? 'Start Node' : 'End Node';
    const buttonColor = nodeType === 0 ? 'green' : 'red';

    return (
      <div className="fixed right-0 top-0 h-full w-96 bg-white border-l border-gray-200 shadow-xl z-10 overflow-y-auto">
        <div className="p-4 border-b border-gray-200 flex items-center justify-between">
          <h2 className="text-lg font-semibold text-gray-900">{nodeLabel} Properties</h2>
          <button
            onClick={handleClose}
            className="p-1 hover:bg-gray-100 rounded-md transition-colors"
          >
            <X size={20} className="text-gray-500" />
          </button>
        </div>

        <form onSubmit={handleSubmit(basicOnSubmit)} className="p-4 space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
            <input
              {...register('name')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
              placeholder={nodeLabel}
            />
            {errors.name && (
              <p className="text-sm text-red-600 mt-1">{errors.name.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Business Notes</label>
            <textarea
              {...register('businessNotes')}
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500"
              placeholder="Optional business context..."
            />
          </div>

          <button
            type="submit"
            className={`w-full bg-${buttonColor}-600 text-white py-2 px-4 rounded-md hover:bg-${buttonColor}-700 transition-colors font-medium`}
            style={{ backgroundColor: buttonColor === 'green' ? '#16a34a' : '#dc2626' }}
          >
            Save Changes
          </button>
        </form>
      </div>
    );
  }

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

          <div className="border-t border-gray-200 pt-4 mt-4">
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-sm font-semibold text-gray-900">Input Bindings</h3>
              <button
                type="button"
                onClick={addInputBinding}
                className="flex items-center gap-1 text-sm text-blue-600 hover:text-blue-700"
              >
                <Plus size={16} />
                Add Binding
              </button>
            </div>

            {inputBindings.map((binding, index) => (
              <div key={index} className="space-y-2 p-3 bg-gray-50 rounded-md mb-2">
                <div className="flex items-center justify-between">
                  <span className="text-xs text-gray-500">Binding {index + 1}</span>
                  <button
                    type="button"
                    onClick={() => removeInputBinding(index)}
                    className="text-red-500 hover:text-red-600"
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Target Property</label>
                  <input
                    type="text"
                    value={binding.targetInputProperty}
                    onChange={(e) => updateInputBinding(index, 'targetInputProperty', e.target.value)}
                    className="w-full px-2 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="e.g., MessageTemplate"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Source Node</label>
                  <select
                    value={binding.source.sourceNodeId}
                    onChange={(e) => updateInputBinding(index, 'source', { ...binding.source, sourceNodeId: e.target.value })}
                    className="w-full px-2 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-blue-500"
                  >
                    <option value="">Select source node...</option>
                    {nodes.map(node => (
                      <option key={node.id} value={node.id}>{(node.data.name as string) || node.id}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Source Property</label>
                  <input
                    type="text"
                    value={binding.source.sourcePath}
                    onChange={(e) => updateInputBinding(index, 'source', { ...binding.source, sourcePath: e.target.value })}
                    className="w-full px-2 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="e.g., ApiResult"
                  />
                </div>
              </div>
            ))}
          </div>
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

          <div className="border-t border-gray-200 pt-4 mt-4">
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-sm font-semibold text-gray-900">Input Bindings</h3>
              <button
                type="button"
                onClick={addInputBinding}
                className="flex items-center gap-1 text-sm text-purple-600 hover:text-purple-700"
              >
                <Plus size={16} />
                Add Binding
              </button>
            </div>

            {inputBindings.map((binding, index) => (
              <div key={index} className="space-y-2 p-3 bg-gray-50 rounded-md mb-2">
                <div className="flex items-center justify-between">
                  <span className="text-xs text-gray-500">Binding {index + 1}</span>
                  <button
                    type="button"
                    onClick={() => removeInputBinding(index)}
                    className="text-red-500 hover:text-red-600"
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Target Property</label>
                  <input
                    type="text"
                    value={binding.targetInputProperty}
                    onChange={(e) => updateInputBinding(index, 'targetInputProperty', e.target.value)}
                    className="w-full px-2 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-purple-500"
                    placeholder="e.g., MessageTemplate"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Source Node</label>
                  <select
                    value={binding.source.sourceNodeId}
                    onChange={(e) => updateInputBinding(index, 'source', { ...binding.source, sourceNodeId: e.target.value })}
                    className="w-full px-2 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-purple-500"
                  >
                    <option value="">Select source node...</option>
                    {nodes.map(node => (
                      <option key={node.id} value={node.id}>{(node.data.name as string) || node.id}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Source Property</label>
                  <input
                    type="text"
                    value={binding.source.sourcePath}
                    onChange={(e) => updateInputBinding(index, 'source', { ...binding.source, sourcePath: e.target.value })}
                    className="w-full px-2 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-purple-500"
                    placeholder="e.g., ApiResult"
                  />
                </div>
              </div>
            ))}
          </div>
        </form>
      </div>
    );
  }

  return null;
}
