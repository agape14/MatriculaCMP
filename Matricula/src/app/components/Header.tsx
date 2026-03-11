import { User, MapPin, LogOut } from 'lucide-react';
import { Button } from './ui/button';

interface HeaderProps {
  userName: string;
  userRole: string;
  onLogout?: () => void;
}

export function Header({ userName, userRole, onLogout }: HeaderProps) {
  return (
    <header className="bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
      <div className="flex items-center gap-4">
        <div className="flex items-center gap-2 text-gray-700">
          <User className="w-5 h-5" style={{ color: '#78276B' }} />
          <span className="font-medium">{userName}</span>
        </div>
        <div className="flex items-center gap-2 text-gray-600">
          <MapPin className="w-4 h-4" style={{ color: '#F8AD1D' }} />
          <span className="text-sm">{userRole}</span>
        </div>
      </div>

      <Button variant="destructive" className="gap-2" onClick={onLogout}>
        <LogOut className="w-4 h-4" />
        Cerrar Sesión
      </Button>
    </header>
  );
}