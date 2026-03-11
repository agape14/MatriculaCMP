import { Card, CardContent } from './ui/card';
import { CheckCircle2, Circle, AlertCircle } from 'lucide-react';
import { Button } from './ui/button';
import { Badge } from './ui/badge';
import { ProgressCircle } from './ProgressCircle';
import { DecanoCRDashboard } from './DecanoCRDashboard';

interface HomeViewProps {
  onNavigate: (view: 'registration' | 'tracking') => void;
  userRole?: string;
}

interface TrackingStep {
  id: number;
  title: string;
  status: 'completed' | 'current' | 'pending';
}

const trackingSteps: TrackingStep[] = [
  { id: 1, title: 'Pendiente de curso Ética', status: 'completed' },
  { id: 2, title: 'Registrado', status: 'completed' },
  { id: 3, title: 'Aprobado por Of. Matrícula', status: 'completed' },
  { id: 4, title: 'Firmado por Consejo Regional', status: 'current' },
  { id: 5, title: 'Firmado por Consejo Nacional', status: 'pending' },
  { id: 6, title: 'Diploma Firmado - Entregado', status: 'pending' },
];

export function HomeView({ onNavigate, userRole }: HomeViewProps) {
  // If user is Decano CR, Secretario CR, Decano Nacional, Secretario Nacional, or Matricula, show the Decano CR dashboard
  if (userRole === 'decano-cr' || userRole === 'secretario-cr' || userRole === 'decano-nacional' || userRole === 'secretario-nacional' || userRole === 'matricula') {
    return <DecanoCRDashboard />;
  }

  const completedSteps = trackingSteps.filter(step => step.status === 'completed').length;
  const currentStep = trackingSteps.find(step => step.status === 'current');
  const progressPercentage = Math.round((completedSteps / trackingSteps.length) * 100);

  const getRowColor = (status: string) => {
    switch (status) {
      case 'completed':
        return 'hover:bg-gray-50';
      case 'current':
        return 'hover:bg-orange-50';
      default:
        return 'hover:bg-gray-50';
    }
  };

  return (
    <div className="p-8 space-y-6">
      <div>
        <h1 className="text-3xl font-semibold text-gray-900 mb-2">Bienvenido al Sistema de Matrícula</h1>
        <p className="text-gray-600">Gestiona tu proceso de colegiatura de manera sencilla</p>
      </div>

      {/* Status Alert */}
      <Card className="border-l-4" style={{ borderLeftColor: '#F8AD1D', backgroundColor: '#FFF8E8' }}>
        
      </Card>

      {/* Status Table and Progress */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Status Table */}
        <Card className="lg:col-span-2">
          <CardContent className="p-0">
            <div className="overflow-hidden">
              <table className="w-full">
                <thead>
                  <tr className="text-white" style={{ backgroundColor: '#78276B' }}>
                    <th className="px-6 py-4 text-left text-sm font-semibold w-20">#</th>
                    <th className="px-6 py-4 text-left text-sm font-semibold">Estado del Proceso</th>
                    <th className="px-6 py-4 text-center text-sm font-semibold w-32">Estado</th>
                  </tr>
                </thead>
                <tbody>
                  {trackingSteps.map((step) => (
                    <tr
                      key={step.id}
                      className={`border-b border-gray-200 transition-colors ${getRowColor(step.status)}`}
                    >
                      <td className="px-6 py-4 font-medium text-gray-900">{step.id}</td>
                      <td className="px-6 py-4 text-gray-800">{step.title}</td>
                      <td className="px-6 py-4 text-center">
                        {step.status === 'completed' ? (
                          <div className="flex justify-center">
                            <CheckCircle2 className="w-5 h-5" style={{ color: '#78276B' }} />
                          </div>
                        ) : step.status === 'current' ? (
                          <Badge variant="default" style={{ backgroundColor: '#F8AD1D', color: 'white' }}>
                            En Proceso
                          </Badge>
                        ) : (
                          <div className="flex justify-center">
                            <Circle className="w-5 h-5 text-gray-300" />
                          </div>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        {/* Progress Card */}
        <Card>
          <CardContent className="p-6 flex flex-col items-center justify-center space-y-6">
            <ProgressCircle percentage={progressPercentage} />
            
            <div className="text-center space-y-2 pt-4 border-t border-gray-200 w-full">
              <div className="flex items-center justify-center gap-2 text-sm text-gray-600">
                <Circle className="w-3 h-3" style={{ fill: '#F8AD1D', color: '#F8AD1D' }} />
                <span>Estado Actual</span>
              </div>
              <p className="font-medium text-gray-900">
                {currentStep?.title || 'Proceso completado'}
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}