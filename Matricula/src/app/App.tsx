import { useState } from 'react';
import { ThemeProvider } from 'next-themes';
import { Sidebar } from './components/Sidebar';
import { Header } from './components/Header';
import { HomeView } from './components/HomeView';
import { RegistrationView } from './components/RegistrationView';
import { TrackingView } from './components/TrackingView';
import { DiplomasView } from './components/DiplomasView';
import { MatriculaView } from './components/MatriculaView';
import { LoginView } from './components/LoginView';

type View = 'home' | 'registration' | 'tracking' | 'diplomas' | 'matricula';

export default function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userRole, setUserRole] = useState('');
  const [activeView, setActiveView] = useState<View>('home');

  const handleNavigate = (view: View) => {
    setActiveView(view);
  };

  const handleLogin = (role: string) => {
    setUserRole(role);
    setIsLoggedIn(true);
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setUserRole('');
    setActiveView('home');
  };

  // Map roles to user info
  const getUserInfo = () => {
    switch (userRole) {
      case 'solicitante':
        return { name: 'MARLENE BRILLITH ORNA POVIS', role: 'Médico' };
      case 'decano-nacional':
        return { name: 'Dr. RICARDO VARGAS MONTENEGRO', role: 'Decano Nacional' };
      case 'secretario-nacional':
        return { name: 'Dr. CARLOS JIMÉNEZ RAMOS', role: 'Secretario General Nacional' };
      case 'decano-cr':
        return { name: 'Dr. ROBERTO CASTILLO VEGA', role: 'Decano CR' };
      case 'secretario-cr':
        return { name: 'Dra. ANA LÓPEZ MENDOZA', role: 'Secretario CR' };
      case 'matricula':
        return { name: 'MARÍA GONZÁLEZ TORRES', role: 'Oficina de Matrícula' };
      default:
        return { name: 'Usuario', role: 'Sin asignar' };
    }
  };

  if (!isLoggedIn) {
    return (
      <ThemeProvider attribute="class" defaultTheme="light">
        <LoginView onLogin={handleLogin} />
      </ThemeProvider>
    );
  }

  const userInfo = getUserInfo();

  return (
    <ThemeProvider attribute="class" defaultTheme="light">
      <div className="flex min-h-screen bg-gray-50">
        <Sidebar activeView={activeView} onNavigate={handleNavigate} onLogout={handleLogout} userRole={userRole} />
        
        <div className="flex-1 flex flex-col">
          <Header userName={userInfo.name} userRole={userInfo.role} onLogout={handleLogout} />
          
          <main className="flex-1">
            {activeView === 'home' && <HomeView onNavigate={handleNavigate} userRole={userRole} />}
            {activeView === 'registration' && <RegistrationView />}
            {activeView === 'tracking' && <TrackingView />}
            {activeView === 'diplomas' && <DiplomasView userRole={userRole} />}
            {activeView === 'matricula' && <MatriculaView />}
          </main>
        </div>
      </div>
    </ThemeProvider>
  );
}