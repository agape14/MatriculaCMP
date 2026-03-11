import { Home, FileText, ClipboardList, User, LogOut, Award, ChevronDown, ChevronRight } from 'lucide-react';
import { Button } from './ui/button';
import { useState } from 'react';

interface SidebarProps {
  activeView: 'home' | 'registration' | 'tracking' | 'diplomas' | 'matricula';
  onNavigate: (view: 'home' | 'registration' | 'tracking' | 'diplomas' | 'matricula') => void;
  onLogout?: () => void;
  userRole?: string;
}

export function Sidebar({ activeView, onNavigate, onLogout, userRole }: SidebarProps) {
  const [crExpanded, setCrExpanded] = useState(true);
  const [decanatoCNExpanded, setDecanatoCNExpanded] = useState(true);
  const [secretariaCNExpanded, setSecretariaCNExpanded] = useState(true);

  const showCRSection = userRole === 'decano-cr' || userRole === 'secretario-cr';
  const showDecanatoCNSection = userRole === 'decano-nacional';
  const showSecretariaCNSection = userRole === 'secretario-nacional';
  const showMatriculaSection = userRole === 'matricula';

  return (
    <aside className="w-64 text-white min-h-screen flex flex-col" style={{ background: 'linear-gradient(to bottom, #78276B, #5a1d50)' }}>
      {/* Logo */}
      <div className="px-6 py-4 border-b border-white/10">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 bg-white/10 rounded flex items-center justify-center">
            <Home className="w-5 h-5" />
          </div>
          <h1 className="font-semibold text-lg">MatrículaCMP</h1>
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 p-4">
        <div className="space-y-2">
          <button
            onClick={() => onNavigate('home')}
            className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-all ${
              activeView === 'home'
                ? 'shadow-lg'
                : 'hover:bg-white/10'
            }`}
            style={activeView === 'home' ? { backgroundColor: '#F8AD1D' } : {}}
          >
            <Home className="w-5 h-5" />
            <span>Inicio</span>
          </button>

          {/* Solicitante Section - Hide for Decano CR, Secretario CR, Decano Nacional, and Secretario Nacional */}
          {userRole !== 'decano-cr' && userRole !== 'secretario-cr' && userRole !== 'decano-nacional' && userRole !== 'secretario-nacional' && userRole !== 'matricula' && (
            <>
              <div className="mt-6 mb-2 px-4 text-sm flex items-center gap-2" style={{ color: '#F8AD1D' }}>
                <User className="w-4 h-4" />
                <span className="uppercase tracking-wide">Solicitante</span>
              </div>

              <button
                onClick={() => onNavigate('registration')}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-all ${
                  activeView === 'registration'
                    ? 'shadow-lg'
                    : 'hover:bg-white/10'
                }`}
                style={activeView === 'registration' ? { backgroundColor: '#F8AD1D' } : {}}
              >
                <FileText className="w-5 h-5" />
                <span>Reg. Pre-matrícula</span>
              </button>

              <button
                onClick={() => onNavigate('tracking')}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-all ${
                  activeView === 'tracking'
                    ? 'shadow-lg'
                    : 'hover:bg-white/10'
                }`}
                style={activeView === 'tracking' ? { backgroundColor: '#F8AD1D' } : {}}
              >
                <ClipboardList className="w-5 h-5" />
                <span>Seguimiento Trámite</span>
              </button>
            </>
          )}

          {/* Consejo Regional Section - Only for CR roles */}
          {showCRSection && (
            <>
              <div className="mt-6 mb-2 px-4 text-sm flex items-center gap-2" style={{ color: '#F8AD1D' }}>
                <Award className="w-4 h-4" />
                <span className="uppercase tracking-wide">Consejo Regional</span>
              </div>

              {/* Firmas de Diploma */}
              <button
                onClick={() => onNavigate('diplomas')}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-all ${
                  activeView === 'diplomas'
                    ? 'shadow-lg'
                    : 'hover:bg-white/10'
                }`}
                style={activeView === 'diplomas' ? { backgroundColor: '#F8AD1D' } : {}}
              >
                <FileText className="w-5 h-5" />
                <span>Firmas de Diploma</span>
              </button>
            </>
          )}

          {/* Decanato Nacional Section - Only for Decanato Nacional role */}
          {showDecanatoCNSection && (
            <>
              <div className="mt-6 mb-2 px-4 text-sm flex items-center gap-2" style={{ color: '#F8AD1D' }}>
                <Award className="w-4 h-4" />
                <span className="uppercase tracking-wide">Decanato Nacional</span>
              </div>

              {/* Firmas de Diploma */}
              <button
                onClick={() => onNavigate('diplomas')}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-all ${
                  activeView === 'diplomas'
                    ? 'shadow-lg'
                    : 'hover:bg-white/10'
                }`}
                style={activeView === 'diplomas' ? { backgroundColor: '#F8AD1D' } : {}}
              >
                <FileText className="w-5 h-5" />
                <span>Firmas de Diploma</span>
              </button>
            </>
          )}

          {/* Secretaría Nacional Section - Only for Secretaría Nacional role */}
          {showSecretariaCNSection && (
            <>
              <div className="mt-6 mb-2 px-4 text-sm flex items-center gap-2" style={{ color: '#F8AD1D' }}>
                <Award className="w-4 h-4" />
                <span className="uppercase tracking-wide">Secretaría Nacional</span>
              </div>

              {/* Firmas de Diploma */}
              <button
                onClick={() => onNavigate('diplomas')}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-all ${
                  activeView === 'diplomas'
                    ? 'shadow-lg'
                    : 'hover:bg-white/10'
                }`}
                style={activeView === 'diplomas' ? { backgroundColor: '#F8AD1D' } : {}}
              >
                <FileText className="w-5 h-5" />
                <span>Firmas de Diploma</span>
              </button>
            </>
          )}

          {/* Matricula Section - Only for Matricula role */}
          {showMatriculaSection && (
            <>
              <div className="mt-6 mb-2 px-4 text-sm flex items-center gap-2" style={{ color: '#F8AD1D' }}>
                <Award className="w-4 h-4" />
                <span className="uppercase tracking-wide">Matrícula</span>
              </div>

              {/* Validación de Diplomas */}
              <button
                onClick={() => onNavigate('matricula')}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-all ${
                  activeView === 'matricula'
                    ? 'shadow-lg'
                    : 'hover:bg-white/10'
                }`}
                style={activeView === 'matricula' ? { backgroundColor: '#F8AD1D' } : {}}
              >
                <FileText className="w-5 h-5" />
                <span>Validación Diplomas</span>
              </button>
            </>
          )}
        </div>
      </nav>

      {/* Logout Button */}
      {onLogout && (
        <div className="p-4 border-t border-white/10">
          <Button
            onClick={onLogout}
            variant="outline"
            className="w-full flex items-center justify-center gap-2 bg-white/10 hover:bg-white/20 text-white border-white/20 hover:border-white/30"
          >
            <LogOut className="w-4 h-4" />
            <span>Cerrar Sesión</span>
          </Button>
        </div>
      )}
    </aside>
  );
}