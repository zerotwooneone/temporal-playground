import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ReactFlowProvider } from '@xyflow/react';
import WorkflowDashboard from './features/workflows/pages/WorkflowDashboard';
import WorkflowDesigner from './features/workflows/pages/WorkflowDesigner';
import { UserProvider } from './contexts/UserContext';
import UserSelector from './components/UserSelector';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <UserProvider>
        <ReactFlowProvider>
          <BrowserRouter>
            <UserSelector />
            <Routes>
              <Route path="/" element={<Navigate to="/workflows" replace />} />
              <Route path="/workflows" element={<WorkflowDashboard />} />
              <Route path="/workflows/:id" element={<WorkflowDesigner />} />
            </Routes>
          </BrowserRouter>
        </ReactFlowProvider>
      </UserProvider>
    </QueryClientProvider>
  );
}

export default App;
