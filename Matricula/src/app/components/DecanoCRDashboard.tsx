import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { Users, FileText, Menu, ClipboardList, ChevronRight, FileCheck, UserCheck, FileSignature, FileClock } from 'lucide-react';
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend } from 'recharts';

interface StatCardProps {
  title: string;
  value: number;
  icon: React.ReactNode;
  color: string;
}

function StatCard({ title, value, icon, color }: StatCardProps) {
  return (
    <Card className="hover:shadow-lg transition-shadow">
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          <div className="space-y-1">
            <p className="text-sm text-gray-600">{title}</p>
            <p className="text-3xl font-bold text-gray-900">{value}</p>
          </div>
          <div 
            className="w-14 h-14 rounded-full flex items-center justify-center"
            style={{ backgroundColor: color + '20' }}
          >
            <div style={{ color: color }}>
              {icon}
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

const monthlyData = [
  { month: 'Ene', total: 28, pendientes: 5, enMatricula: 12, firmaSecCR: 4, firmaDecanoCR: 3, firmaDecanoNacional: 2, firmaSecGeneral: 2 },
  { month: 'Feb', total: 32, pendientes: 6, enMatricula: 14, firmaSecCR: 5, firmaDecanoCR: 3, firmaDecanoNacional: 2, firmaSecGeneral: 2 },
  { month: 'Mar', total: 45, pendientes: 8, enMatricula: 20, firmaSecCR: 7, firmaDecanoCR: 4, firmaDecanoNacional: 3, firmaSecGeneral: 3 },
  { month: 'Abr', total: 38, pendientes: 7, enMatricula: 16, firmaSecCR: 6, firmaDecanoCR: 4, firmaDecanoNacional: 3, firmaSecGeneral: 2 },
  { month: 'May', total: 52, pendientes: 10, enMatricula: 22, firmaSecCR: 8, firmaDecanoCR: 5, firmaDecanoNacional: 4, firmaSecGeneral: 3 },
  { month: 'Jun', total: 41, pendientes: 8, enMatricula: 18, firmaSecCR: 6, firmaDecanoCR: 4, firmaDecanoNacional: 3, firmaSecGeneral: 2 },
  { month: 'Jul', total: 35, pendientes: 6, enMatricula: 15, firmaSecCR: 5, firmaDecanoCR: 4, firmaDecanoNacional: 3, firmaSecGeneral: 2 },
  { month: 'Ago', total: 48, pendientes: 9, enMatricula: 21, firmaSecCR: 7, firmaDecanoCR: 5, firmaDecanoNacional: 3, firmaSecGeneral: 3 },
  { month: 'Sep', total: 42, pendientes: 8, enMatricula: 18, firmaSecCR: 6, firmaDecanoCR: 4, firmaDecanoNacional: 3, firmaSecGeneral: 3 },
  { month: 'Oct', total: 39, pendientes: 7, enMatricula: 17, firmaSecCR: 6, firmaDecanoCR: 4, firmaDecanoNacional: 3, firmaSecGeneral: 2 },
  { month: 'Nov', total: 44, pendientes: 8, enMatricula: 19, firmaSecCR: 7, firmaDecanoCR: 4, firmaDecanoNacional: 3, firmaSecGeneral: 3 },
  { month: 'Dic', total: 36, pendientes: 6, enMatricula: 16, firmaSecCR: 5, firmaDecanoCR: 4, firmaDecanoNacional: 3, firmaSecGeneral: 2 },
];

export function DecanoCRDashboard() {
  return (
    <div className="p-8 space-y-6">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm text-gray-600">
        <span className="font-medium text-gray-900">Inicio</span>
      </div>

      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">PANEL DE CONTROL - DECANO CR</h1>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <StatCard
          title="Solicitudes Pendientes"
          value={87}
          icon={<FileClock className="w-7 h-7" />}
          color="#F59E0B"
        />
        <StatCard
          title="En Matrícula"
          value={208}
          icon={<FileCheck className="w-7 h-7" />}
          color="#10B981"
        />
        <StatCard
          title="Firma Secretario CR"
          value={72}
          icon={<FileSignature className="w-7 h-7" />}
          color="#3B82F6"
        />
        <StatCard
          title="Firma Decano CR"
          value={48}
          icon={<FileSignature className="w-7 h-7" />}
          color="#8B5CF6"
        />
        <StatCard
          title="Firma Decano Nacional"
          value={35}
          icon={<UserCheck className="w-7 h-7" />}
          color="#78276B"
        />
        <StatCard
          title="Firma Secretario General"
          value={29}
          icon={<UserCheck className="w-7 h-7" />}
          color="#EC4899"
        />
      </div>

      {/* Chart */}
      <Card>
        <CardHeader>
          <CardTitle className="text-xl font-bold text-gray-900">Estadísticas de Solicitudes por Mes</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-96">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart 
                data={monthlyData} 
                margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                id="dashboard-chart"
              >
                <CartesianGrid key="grid" strokeDasharray="3 3" stroke="#e5e7eb" />
                <XAxis 
                  key="xaxis"
                  dataKey="month"
                  tick={{ fill: '#6b7280', fontSize: 12 }}
                />
                <YAxis 
                  key="yaxis"
                  tick={{ fill: '#6b7280', fontSize: 12 }}
                />
                <Tooltip 
                  key="tooltip"
                  contentStyle={{ 
                    backgroundColor: '#fff', 
                    border: '1px solid #e5e7eb',
                    borderRadius: '8px',
                    padding: '12px'
                  }}
                />
                <Legend 
                  key="legend"
                  wrapperStyle={{ paddingTop: '20px' }}
                  iconType="line"
                />
                <Line 
                  key="total"
                  type="monotone"
                  dataKey="total"
                  name="Total"
                  stroke="#78276B" 
                  strokeWidth={3}
                  dot={{ fill: '#78276B', r: 4 }}
                  activeDot={{ r: 6 }}
                />
                <Line 
                  key="pendientes"
                  type="monotone"
                  dataKey="pendientes"
                  name="Pendientes"
                  stroke="#F59E0B" 
                  strokeWidth={2}
                  dot={{ fill: '#F59E0B', r: 3 }}
                />
                <Line 
                  key="enMatricula"
                  type="monotone"
                  dataKey="enMatricula"
                  name="En Matrícula"
                  stroke="#10B981" 
                  strokeWidth={2}
                  dot={{ fill: '#10B981', r: 3 }}
                />
                <Line 
                  key="firmaSecCR"
                  type="monotone"
                  dataKey="firmaSecCR"
                  name="Firma Sec. CR"
                  stroke="#3B82F6" 
                  strokeWidth={2}
                  dot={{ fill: '#3B82F6', r: 3 }}
                />
                <Line 
                  key="firmaDecanoCR"
                  type="monotone"
                  dataKey="firmaDecanoCR"
                  name="Firma Decano CR"
                  stroke="#8B5CF6" 
                  strokeWidth={2}
                  dot={{ fill: '#8B5CF6', r: 3 }}
                />
                <Line 
                  key="firmaDecanoNacional"
                  type="monotone"
                  dataKey="firmaDecanoNacional"
                  name="Firma Decano Nacional"
                  stroke="#EC4899" 
                  strokeWidth={2}
                  dot={{ fill: '#EC4899', r: 3 }}
                />
                <Line 
                  key="firmaSecGeneral"
                  type="monotone"
                  dataKey="firmaSecGeneral"
                  name="Firma Sec. General"
                  stroke="#EF4444" 
                  strokeWidth={2}
                  dot={{ fill: '#EF4444', r: 3 }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}