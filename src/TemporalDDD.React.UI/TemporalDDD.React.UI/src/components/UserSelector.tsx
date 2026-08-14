import { useUser } from '../contexts/UserContext';

export default function UserSelector() {
  const { role, setRole } = useUser();

  return (
    <div className="fixed top-16 right-4 z-50">
      <select
        value={role}
        onChange={(e) => setRole(e.target.value as 'designer' | 'approver')}
        className="px-4 py-2 bg-white border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm font-medium"
      >
        <option value="designer">Designer</option>
        <option value="approver">Approver</option>
      </select>
    </div>
  );
}
