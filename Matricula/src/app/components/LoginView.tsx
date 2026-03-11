import { UserCircle, Shield, UserCog, Award, FileCheck, GraduationCap } from 'lucide-react';
import { Card } from './ui/card';
import { Button } from './ui/button';

interface LoginViewProps {
  onLogin: (role: string) => void;
}

export function LoginView({ onLogin }: LoginViewProps) {
  const profiles = [
    {
      id: 'solicitante',
      name: 'Solicitante',
      description: 'Acceso para usuarios que solicitan matrícula',
      icon: UserCircle,
      color: '#3B82F6',
      bgColor: '#EFF6FF'
    },
    {
      id: 'decano-nacional',
      name: 'Decano Nacional',
      description: 'Consejo Nacional - Máxima autoridad',
      icon: Award,
      color: '#78276B',
      bgColor: '#F3E8FF'
    },
    {
      id: 'secretario-nacional',
      name: 'Secretario General Nacional',
      description: 'Consejo Nacional - Secretaría General',
      icon: Shield,
      color: '#78276B',
      bgColor: '#F3E8FF'
    },
    {
      id: 'decano-cr',
      name: 'Decano CR',
      description: 'Consejo Regional - Autoridad regional',
      icon: GraduationCap,
      color: '#9333AB',
      bgColor: '#FAF5FF'
    },
    {
      id: 'secretario-cr',
      name: 'Secretario CR',
      description: 'Consejo Regional - Secretaría Regional',
      icon: UserCog,
      color: '#9333AB',
      bgColor: '#FAF5FF'
    },
    {
      id: 'matricula',
      name: 'Oficina de Matrícula',
      description: 'Gestión y administración de matrículas',
      icon: FileCheck,
      color: '#F8AD1D',
      bgColor: '#FFFBEB'
    }
  ];

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 via-purple-50 to-gray-50 p-6">
      <div className="w-full max-w-6xl">
        {/* Header */}
        <div className="text-center mb-12">
          <div className="inline-flex items-center justify-center w-20 h-20 rounded-full mb-6" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
            <Shield className="w-10 h-10 text-white" />
          </div>
          <h1 className="text-4xl font-bold text-gray-900 mb-3">
            Sistema de Matrícula
          </h1>
          <p className="text-lg text-gray-600">
            Selecciona tu perfil para acceder al sistema
          </p>
        </div>

        {/* Profile Cards Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {profiles.map((profile) => {
            const Icon = profile.icon;
            return (
              <Card 
                key={profile.id}
                className="group hover:shadow-xl transition-all duration-300 cursor-pointer border-2 hover:border-opacity-100"
                style={{ borderColor: `${profile.color}40` }}
                onClick={() => onLogin(profile.id)}
              >
                <div className="p-6 space-y-4">
                  {/* Icon */}
                  <div 
                    className="w-16 h-16 rounded-full flex items-center justify-center transition-transform group-hover:scale-110 duration-300"
                    style={{ backgroundColor: profile.bgColor }}
                  >
                    <Icon className="w-8 h-8" style={{ color: profile.color }} />
                  </div>

                  {/* Title */}
                  <div>
                    <h3 className="text-xl font-bold text-gray-900 mb-2">
                      {profile.name}
                    </h3>
                    <p className="text-sm text-gray-600">
                      {profile.description}
                    </p>
                  </div>

                  {/* Button */}
                  <Button
                    className="w-full text-white font-semibold transition-all duration-300 group-hover:shadow-lg"
                    style={{ backgroundColor: profile.color }}
                  >
                    Acceder
                  </Button>
                </div>
              </Card>
            );
          })}
        </div>

        {/* Footer */}
        <div className="mt-12 text-center">
          <p className="text-sm text-gray-500">
            Sistema de Gestión de Matrícula Profesional
          </p>
          <p className="text-xs text-gray-400 mt-2">
            Todos los derechos reservados © 2026
          </p>
        </div>
      </div>
    </div>
  );
}
