import axios from 'axios';

export const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Optional: Add request/response interceptors for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error);
    return Promise.reject(error);
  }
);

export const createWorkflow = async (name: string, creatorId: string) => {
  const response = await apiClient.post('/workflows', { name, creatorId });
  return response.data; // Returns CreateWorkflowResponse with workflowId
};

export const getWorkflows = async () => {
  const response = await apiClient.get('/workflows');
  return response.data;
};

export const getWorkflowById = async (id: string) => {
  const response = await apiClient.get(`/workflows/${id}`);
  return response.data;
};

export const publishWorkflow = async (id: string) => {
  const response = await apiClient.post(`/workflows/${id}/publish`);
  return response.data;
};

export const submitWorkflowForReview = async (id: string) => {
  const response = await apiClient.post(`/workflows/${id}/submit`);
  return response.data;
};

export const approveWorkflow = async (id: string, reviewerId: string) => {
  const response = await apiClient.post(`/workflows/${id}/approve`, { reviewerId });
  return response.data;
};
