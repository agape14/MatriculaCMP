import { ChevronRight, Search, Eye, Star, FileCheck } from 'lucide-react';
import { Card } from './ui/card';
import { Badge } from './ui/badge';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Checkbox } from './ui/checkbox';
import { useState } from 'react';
import { DiplomaViewerModal } from './DiplomaViewerModal';

interface DiplomaRecord {
  id: number;
  numeroSolicitud: string;
  fechaSolicitud: string;
  estado: string;
  dni: string;
  nombre: string;
  numeroColegiatura: string;
  fechaEmision: string;
}

interface DiplomasViewProps {
  userRole?: string;
}

const diplomaRecords: DiplomaRecord[] = [
  {
    id: 113,
    numeroSolicitud: '113',
    fechaSolicitud: '05/02/2026',
    estado: 'Pendiente Firma Secretario CR',
    dni: '20260216',
    nombre: 'Nombre12 Apellido1P12 Apellido1M12',
    numeroColegiatura: 'CMP-2026-001049',
    fechaEmision: '05/02/2026'
  },
  {
    id: 114,
    numeroSolicitud: '114',
    fechaSolicitud: '05/02/2026',
    estado: 'Pendiente Firma Secretario CR',
    dni: '20260217',
    nombre: 'Nombre13 Apellido1P13 Apellido1M13',
    numeroColegiatura: 'CMP-2026-001050',
    fechaEmision: '05/02/2026'
  },
  {
    id: 115,
    numeroSolicitud: '115',
    fechaSolicitud: '05/02/2026',
    estado: 'Pendiente Firma Secretario CR',
    dni: '20260218',
    nombre: 'Nombre14 Apellido1P14 Apellido1M14',
    numeroColegiatura: 'CMP-2026-001051',
    fechaEmision: '05/02/2026'
  },
  {
    id: 116,
    numeroSolicitud: '116',
    fechaSolicitud: '05/02/2026',
    estado: 'Pendiente Firma Secretario CR',
    dni: '20260219',
    nombre: 'Nombre15 Apellido1P15 Apellido1M15',
    numeroColegiatura: 'CMP-2026-001052',
    fechaEmision: '05/02/2026'
  },
  {
    id: 117,
    numeroSolicitud: '117',
    fechaSolicitud: '05/02/2026',
    estado: 'Pendiente Firma Secretario CR',
    dni: '20260220',
    nombre: 'Nombre16 Apellido1P16 Apellido1M16',
    numeroColegiatura: 'CMP-2026-001053',
    fechaEmision: '05/02/2026'
  },
  {
    id: 118,
    numeroSolicitud: '118',
    fechaSolicitud: '05/02/2026',
    estado: 'Pendiente Firma Secretario CR',
    dni: '20260221',
    nombre: 'Nombre17 Apellido1P17 Apellido1M17',
    numeroColegiatura: 'CMP-2026-001054',
    fechaEmision: '05/02/2026'
  },
  {
    id: 119,
    numeroSolicitud: '119',
    fechaSolicitud: '05/02/2026',
    estado: 'Pendiente Firma Decano CR',
    dni: '20260222',
    nombre: 'Nombre18 Apellido1P18 Apellido1M18',
    numeroColegiatura: 'CMP-2026-001055',
    fechaEmision: '05/02/2026'
  },
  {
    id: 120,
    numeroSolicitud: '120',
    fechaSolicitud: '05/02/2026',
    estado: 'Pendiente Firma Decano CR',
    dni: '20260223',
    nombre: 'Nombre19 Apellido1P19 Apellido1M19',
    numeroColegiatura: 'CMP-2026-001056',
    fechaEmision: '05/02/2026'
  },
  {
    id: 121,
    numeroSolicitud: '121',
    fechaSolicitud: '05/02/2026',
    estado: 'Pendiente Firma Decano CR',
    dni: '20260224',
    nombre: 'Nombre20 Apellido1P20 Apellido1M20',
    numeroColegiatura: 'CMP-2026-001057',
    fechaEmision: '05/02/2026'
  },
];

export function DiplomasView({ userRole }: DiplomasViewProps) {
  // Debug: verificar el valor de userRole
  console.log('DiplomasView userRole:', userRole);
  
  // Determine initial filter based on user role
  const getInitialFilter = () => {
    if (userRole === 'decano-cr') return 'decano';
    if (userRole === 'secretario-cr') return 'secretario';
    return 'secretario';
  };

  const [selectedAll, setSelectedAll] = useState(false);
  const [selectedItems, setSelectedItems] = useState<number[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [activeFilter, setActiveFilter] = useState<'secretario' | 'decano' | 'todos'>(getInitialFilter());
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedDiploma, setSelectedDiploma] = useState<DiplomaRecord | null>(null);

  const handleViewDiploma = (record: DiplomaRecord) => {
    setSelectedDiploma(record);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedDiploma(null);
  };

  const handleSelectAll = () => {
    if (selectedAll) {
      setSelectedItems([]);
    } else {
      setSelectedItems(diplomaRecords.map(record => record.id));
    }
    setSelectedAll(!selectedAll);
  };

  const handleSelectItem = (id: number) => {
    if (selectedItems.includes(id)) {
      setSelectedItems(selectedItems.filter(item => item !== id));
    } else {
      setSelectedItems([...selectedItems, id]);
    }
  };

  const secCRCount = 25;
  const decanoCRCount = 15;

  return (
    <div className="p-8 space-y-6">
      {/* Breadcrumb */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 text-sm text-gray-600">
          <span className="hover:text-gray-900 cursor-pointer">Inicio</span>
          <ChevronRight className="w-4 h-4" />
          <span className="font-medium text-gray-900">Listado Diplomas</span>
        </div>
      </div>

      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">
          {userRole === 'decano-nacional' 
            ? 'DECANATO NACIONAL - LISTADO DIPLOMAS' 
            : userRole === 'secretario-nacional'
            ? 'SECRETARÍA NACIONAL - LISTADO DIPLOMAS'
            : 'CONSEJO REGIONAL - LISTADO DIPLOMAS'}
        </h1>
      </div>

      {/* Filters and Actions */}
      <Card>
        <div className="p-6 space-y-4">
          {/* Top Row - Filters */}
          <div className="flex flex-wrap items-center gap-3">
            {/* Sec. CR Button - Only for Secretario CR */}
            {userRole === 'secretario-cr' && (
              <Button
                variant={activeFilter === 'secretario' ? 'default' : 'outline'}
                className="gap-2"
                style={activeFilter === 'secretario' ? { backgroundColor: '#78276B' } : {}}
                onClick={() => setActiveFilter('secretario')}
              >
                <FileCheck className="w-4 h-4" />
                Sec. CR
                <Badge 
                  variant="secondary" 
                  className="ml-1 text-white"
                  style={{ backgroundColor: '#9333AB' }}
                >
                  {secCRCount}
                </Badge>
              </Button>
            )}

            {/* Decano CR Button - Only for Decano CR */}
            {userRole === 'decano-cr' && (
              <Button
                variant={activeFilter === 'decano' ? 'default' : 'outline'}
                className="gap-2"
                style={activeFilter === 'decano' ? { backgroundColor: '#78276B' } : {}}
                onClick={() => setActiveFilter('decano')}
              >
                <FileCheck className="w-4 h-4" />
                Decano CR
                <Badge 
                  variant="secondary" 
                  className="ml-1 text-white"
                  style={{ backgroundColor: '#9333AB' }}
                >
                  {decanoCRCount}
                </Badge>
              </Button>
            )}

            {/* Decano Nacional Button - Only for Decano Nacional */}
            {userRole === 'decano-nacional' && (
              <Button
                variant={activeFilter === 'decano' ? 'default' : 'outline'}
                className="gap-2"
                style={activeFilter === 'decano' ? { backgroundColor: '#78276B' } : {}}
                onClick={() => setActiveFilter('decano')}
              >
                <FileCheck className="w-4 h-4" />
                Decano
                <Badge 
                  variant="secondary" 
                  className="ml-1 text-white"
                  style={{ backgroundColor: '#9333AB' }}
                >
                  {decanoCRCount}
                </Badge>
              </Button>
            )}

            {/* Secretario General Nacional Button - Only for Secretario Nacional */}
            {userRole === 'secretario-nacional' && (
              <Button
                variant={activeFilter === 'secretario' ? 'default' : 'outline'}
                className="gap-2"
                style={activeFilter === 'secretario' ? { backgroundColor: '#78276B' } : {}}
                onClick={() => setActiveFilter('secretario')}
              >
                <FileCheck className="w-4 h-4" />
                Secretario General
                <Badge 
                  variant="secondary" 
                  className="ml-1 text-white"
                  style={{ backgroundColor: '#9333AB' }}
                >
                  {secCRCount}
                </Badge>
              </Button>
            )}

            {/* Seleccionar todos */}
            <div className="flex items-center gap-2 ml-4">
              <Checkbox 
                id="select-all" 
                checked={selectedAll}
                onCheckedChange={handleSelectAll}
              />
              <label htmlFor="select-all" className="text-sm font-medium cursor-pointer">
                Seleccionar todos
              </label>
            </div>

            {/* Firmar seleccionados */}
            <Button
              className="gap-2 text-white font-semibold ml-2"
              style={{ backgroundColor: '#78276B' }}
              disabled={selectedItems.length === 0}
            >
              <FileCheck className="w-4 h-4" />
              Firmar seleccionados
              {selectedItems.length > 0 && (
                <Badge 
                  variant="secondary" 
                  className="ml-1 bg-white"
                  style={{ color: '#78276B' }}
                >
                  {selectedItems.length}
                </Badge>
              )}
            </Button>

            {/* Search */}
            <div className="ml-auto flex items-center gap-2 min-w-[300px]">
              <Search className="w-5 h-5 text-gray-400" />
              <Input
                type="text"
                placeholder="Buscar DNI, nombre o N° solicitud..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="flex-1"
              />
            </div>
          </div>
        </div>
      </Card>

      {/* Table */}
      <Card>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="text-white" style={{ backgroundColor: '#78276B' }}>
                <th className="px-4 py-3 text-left font-semibold text-sm w-12"></th>
                <th className="px-4 py-3 text-left font-semibold text-sm">N° Solicitud</th>
                <th className="px-4 py-3 text-left font-semibold text-sm">Fecha Solicitud</th>
                <th className="px-4 py-3 text-left font-semibold text-sm">Estado</th>
                <th className="px-4 py-3 text-left font-semibold text-sm">DNI</th>
                <th className="px-4 py-3 text-left font-semibold text-sm">Nombre</th>
                <th className="px-4 py-3 text-left font-semibold text-sm">N° Colegiatura</th>
                <th className="px-4 py-3 text-left font-semibold text-sm">Fecha Emisión</th>
                <th className="px-4 py-3 text-center font-semibold text-sm">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {diplomaRecords
                .filter(record => {
                  const search = searchTerm.toLowerCase();
                  return (
                    record.dni.toLowerCase().includes(search) ||
                    record.nombre.toLowerCase().includes(search) ||
                    record.numeroSolicitud.toLowerCase().includes(search)
                  );
                })
                .map((record, index) => (
                  <tr 
                    key={record.id}
                    className={`border-b border-gray-200 hover:bg-gray-50 transition-colors ${
                      index % 2 === 0 ? 'bg-white' : 'bg-gray-50/50'
                    }`}
                  >
                    <td className="px-4 py-3">
                      <Checkbox
                        checked={selectedItems.includes(record.id)}
                        onCheckedChange={() => handleSelectItem(record.id)}
                      />
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">{record.numeroSolicitud}</td>
                    <td className="px-4 py-3 text-sm text-gray-700">{record.fechaSolicitud}</td>
                    <td className="px-4 py-3">
                      <Badge 
                        variant="default"
                        className="text-xs font-medium text-white"
                        style={{ backgroundColor: '#3B82F6' }}
                      >
                        {record.estado}
                      </Badge>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-700">{record.dni}</td>
                    <td className="px-4 py-3 text-sm text-gray-900">{record.nombre}</td>
                    <td className="px-4 py-3 text-sm text-gray-700">{record.numeroColegiatura}</td>
                    <td className="px-4 py-3 text-sm text-gray-700">{record.fechaEmision}</td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-center gap-2">
                        <Button 
                          variant="ghost" 
                          size="sm"
                          className="h-8 w-8 p-0 hover:bg-blue-50"
                          onClick={() => handleViewDiploma(record)}
                        >
                          <Eye className="w-4 h-4" style={{ color: '#3B82F6' }} />
                        </Button>
                        <Button 
                          variant="ghost" 
                          size="sm"
                          className="h-8 w-8 p-0 hover:bg-yellow-50"
                        >
                          <Star className="w-4 h-4" style={{ color: '#F8AD1D' }} />
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Modal */}
      {isModalOpen && selectedDiploma && (
        <DiplomaViewerModal
          diploma={selectedDiploma}
          onClose={handleCloseModal}
        />
      )}
    </div>
  );
}