import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ReactFlowProvider } from '@xyflow/react';
import WorkflowDesigner from './features/workflows/pages/WorkflowDesigner';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ReactFlowProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Navigate to="/workflows" replace />} />
            <Route path="/workflows" element={<WorkflowDesigner />} />
          </Routes>
        </BrowserRouter>
      </ReactFlowProvider>
    </QueryClientProvider>
  );
}

export default App;
