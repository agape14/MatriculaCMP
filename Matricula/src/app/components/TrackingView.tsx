import { CheckCircle2, Circle, Search, Eye, History, ChevronRight, User, FileText, MapPin, Calendar, Phone, Mail, GraduationCap, CreditCard, Shield, Clock } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { Badge } from './ui/badge';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from './ui/dialog';
import { ImageWithFallback } from './figma/ImageWithFallback';
import { useState } from 'react';

interface TrackingRecord {
  id: number;
  nombre: string;
  numeroSolicitud: string;
  fechaSolicitud: string;
  estado: string;
  area: string;
  observaciones: string;
}

interface HistoryEvent {
  id: number;
  estado: string;
  fecha: string;
  hora: string;
  responsable: string;
  area: string;
  completado: boolean;
}

const trackingRecords: TrackingRecord[] = [
  {
    id: 1,
    nombre: 'JUAN FRANCISCO OBREGON MORI',
    numeroSolicitud: '00000001',
    fechaSolicitud: '05/03/2026',
    estado: 'Curso completado, pendiente de documentos',
    area: 'OFICINA DE MATRICULA',
    observaciones: 'Firma de Consejo Nacional'
  }
];

const historyEvents: HistoryEvent[] = [
  {
    id: 1,
    estado: 'Solicitud Recibida',
    fecha: '01/03/2026',
    hora: '09:15 AM',
    responsable: 'Sistema Automatizado',
    area: 'OFICINA DE MATRÍCULA',
    completado: true
  },
  {
    id: 2,
    estado: 'Validación de Documentos',
    fecha: '01/03/2026',
    hora: '11:30 AM',
    responsable: 'María González Torres',
    area: 'OFICINA DE MATRÍCULA',
    completado: true
  },
  {
    id: 3,
    estado: 'Aprobado por Of. Matrícula',
    fecha: '02/03/2026',
    hora: '02:45 PM',
    responsable: 'Carlos Ramírez Silva',
    area: 'OFICINA DE MATRÍCULA',
    completado: true
  },
  {
    id: 4,
    estado: 'Pendiente Firma Secretario CR',
    fecha: '03/03/2026',
    hora: '10:20 AM',
    responsable: 'Ana López Mendoza',
    area: 'CONSEJO REGIONAL',
    completado: true
  },
  {
    id: 5,
    estado: 'Pendiente Firma Decano CR',
    fecha: '04/03/2026',
    hora: '03:00 PM',
    responsable: 'Dr. Roberto Castillo',
    area: 'CONSEJO REGIONAL',
    completado: true
  },
  {
    id: 6,
    estado: 'Curso completado, pendiente de documentos',
    fecha: '05/03/2026',
    hora: '09:00 AM',
    responsable: 'Oficina de Matrícula',
    area: 'OFICINA DE MATRÍCULA',
    completado: false
  }
];

export function TrackingView() {
  const [open, setOpen] = useState(false);
  const [historyOpen, setHistoryOpen] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState<TrackingRecord | null>(null);

  const handleOpen = (record: TrackingRecord) => {
    setSelectedRecord(record);
    setOpen(true);
  };

  const handleClose = () => {
    setSelectedRecord(null);
    setOpen(false);
  };

  const handleHistoryOpen = (record: TrackingRecord) => {
    setSelectedRecord(record);
    setHistoryOpen(true);
  };

  const handleHistoryClose = () => {
    setSelectedRecord(null);
    setHistoryOpen(false);
  };

  return (
    <div className="p-8 space-y-6">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm text-gray-600">
        <span className="hover:text-gray-900 cursor-pointer">Inicio</span>
        <ChevronRight className="w-4 h-4" />
        <span className="font-medium text-gray-900">Seguimiento Trámite</span>
      </div>

      {/* Header */}
      <div>
        <h1 className="text-3xl font-semibold text-gray-900 mb-2">Seguimiento Trámite</h1>
        <p className="text-gray-600">Revisa el estado actual de tu proceso de matrícula</p>
      </div>

      {/* Table Card */}
      <Card>
        <CardContent className="p-6">
          {/* Search */}
          <div className="mb-6 relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
            <Input
              type="text"
              placeholder="Buscar por nombre, número de solicitud..."
              className="pl-10"
            />
          </div>

          {/* Table */}
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="text-white" style={{ backgroundColor: '#78276B' }}>
                  <th className="px-4 py-3 text-left text-sm font-semibold">Nombre</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold">N° Solicitud</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold">Fecha Solicitud</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold">Estado</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold">Área</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold">Observaciones</th>
                  <th className="px-4 py-3 text-center text-sm font-semibold">Acciones</th>
                </tr>
              </thead>
              <tbody>
                {trackingRecords.map((record) => (
                  <tr
                    key={record.id}
                    className="border-b border-gray-200 hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-4 py-4 text-sm font-medium text-gray-900">
                      {record.nombre}
                    </td>
                    <td className="px-4 py-4 text-sm text-gray-700">
                      {record.numeroSolicitud}
                    </td>
                    <td className="px-4 py-4 text-sm text-gray-700">
                      {record.fechaSolicitud}
                    </td>
                    <td className="px-4 py-4">
                      <Badge 
                        variant="default" 
                        className="text-white font-medium"
                        style={{ backgroundColor: '#3B82F6' }}
                      >
                        {record.estado}
                      </Badge>
                    </td>
                    <td className="px-4 py-4 text-sm text-gray-700">
                      {record.area}
                    </td>
                    <td className="px-4 py-4 text-sm text-gray-700">
                      {record.observaciones}
                    </td>
                    <td className="px-4 py-4">
                      <div className="flex items-center justify-center gap-2">
                        <Button
                          size="sm"
                          variant="outline"
                          className="h-8 px-3 text-xs"
                          onClick={() => handleOpen(record)}
                        >
                          <Eye className="w-3.5 h-3.5 mr-1" />
                          Ver
                        </Button>
                        <Button
                          size="sm"
                          variant="outline"
                          className="h-8 px-3 text-xs"
                          onClick={() => handleHistoryOpen(record)}
                        >
                          <History className="w-3.5 h-3.5 mr-1" />
                          Historial
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {/* Dialog */}
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="w-[90vw] max-w-none h-[85vh] max-h-[85vh] overflow-y-auto p-0">
          <DialogHeader className="px-6 pt-6 pb-4">
            <div className="flex items-start gap-4">
              <div className="relative">
                <ImageWithFallback
                  src="https://images.unsplash.com/photo-1629507208649-70919ca33793?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxwcm9mZXNzaW9uYWwlMjBtYW4lMjBwb3J0cmFpdCUyMGJ1c2luZXNzfGVufDF8fHx8MTc3MjY4OTA4MXww&ixlib=rb-4.1.0&q=80&w=1080"
                  alt={selectedRecord?.nombre || "Usuario"}
                  className="w-20 h-20 rounded-full object-cover border-4"
                  style={{ borderColor: '#78276B' }}
                />
                <div className="absolute -bottom-1 -right-1 w-6 h-6 rounded-full bg-green-500 border-2 border-white"></div>
              </div>
              <div className="flex-1">
                <DialogTitle className="text-2xl mb-1">Detalles de Solicitud</DialogTitle>
                <DialogDescription className="text-base">
                  N° {selectedRecord?.numeroSolicitud} - {selectedRecord?.fechaSolicitud}
                </DialogDescription>
                <p className="text-sm font-semibold text-gray-700 mt-2">{selectedRecord?.nombre}</p>
              </div>
            </div>
          </DialogHeader>

          {selectedRecord && (
            <div className="space-y-6 px-6">
              {/* Estado Actual */}
              <div className="p-4 rounded-lg border-2" style={{ borderColor: '#3B82F6', backgroundColor: '#EFF6FF' }}>
                <div className="flex items-center gap-3">
                  <Shield className="w-6 h-6 text-blue-600" />
                  <div>
                    <p className="text-sm font-semibold text-gray-900">Estado Actual</p>
                    <p className="text-lg font-bold text-blue-600">{selectedRecord.estado}</p>
                  </div>
                </div>
              </div>

              {/* Información Personal */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-4 py-3 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <User className="w-5 h-5" />
                  <h3 className="font-semibold">Datos Personales</h3>
                </div>
                <div className="p-4">
                  <div className="flex flex-col md:flex-row gap-6">
                    {/* Foto del Usuario */}
                    <div className="flex-shrink-0 flex justify-center md:justify-start">
                      <div className="text-center">
                        <ImageWithFallback
                          src="https://images.unsplash.com/photo-1629507208649-70919ca33793?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxwcm9mZXNzaW9uYWwlMjBtYW4lMjBwb3J0cmFpdCUyMGJ1c2luZXNzfGVufDF8fHx8MTc3MjY4OTA4MXww&ixlib=rb-4.1.0&q=80&w=1080"
                          alt="Foto del usuario"
                          className="w-32 h-40 rounded-lg object-cover border-2"
                          style={{ borderColor: '#78276B' }}
                        />
                        <p className="text-xs text-gray-600 mt-2">Foto tamaño pasaporte</p>
                      </div>
                    </div>
                    {/* Datos */}
                    <div className="flex-1 space-y-4">
                      <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-4">
                        <div>
                          <p className="text-xs text-gray-600 mb-1">Nombre Completo</p>
                          <p className="font-semibold text-gray-900">{selectedRecord.nombre}</p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-600 mb-1">DNI / Documento</p>
                          <p className="font-semibold text-gray-900">72845123</p>
                        </div>
                      </div>
                      <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-4">
                        <div>
                          <p className="text-xs text-gray-600 mb-1">Correo Electrónico</p>
                          <p className="font-semibold text-gray-900">juan.obregon@example.com</p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-600 mb-1">Teléfono</p>
                          <p className="font-semibold text-gray-900">+51 987 654 321</p>
                        </div>
                      </div>
                      <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-4">
                        <div>
                          <p className="text-xs text-gray-600 mb-1">Fecha de Nacimiento</p>
                          <p className="font-semibold text-gray-900">15/03/1995</p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-600 mb-1">Nacionalidad</p>
                          <p className="font-semibold text-gray-900">Peruana</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Información Académica */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-4 py-3 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <GraduationCap className="w-5 h-5" />
                  <h3 className="font-semibold">Información Académica</h3>
                </div>
                <div className="p-4 grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <p className="text-xs text-gray-600 mb-1">Perfil</p>
                    <p className="font-semibold text-gray-900">Ingeniero Civil</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-600 mb-1">Universidad de Origen</p>
                    <p className="font-semibold text-gray-900">Universidad Nacional de Ingeniería</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-600 mb-1">Año de Graduación</p>
                    <p className="font-semibold text-gray-900">2020</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-600 mb-1">Tipo de Validación</p>
                    <p className="font-semibold text-gray-900">Título Nacional</p>
                  </div>
                </div>
              </div>

              {/* Información del Trámite */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-4 py-3 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <FileText className="w-5 h-5" />
                  <h3 className="font-semibold">Información del Trámite</h3>
                </div>
                <div className="p-4 grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <p className="text-xs text-gray-600 mb-1">Número de Solicitud</p>
                    <p className="font-semibold text-gray-900">{selectedRecord.numeroSolicitud}</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-600 mb-1">Fecha de Solicitud</p>
                    <p className="font-semibold text-gray-900">{selectedRecord.fechaSolicitud}</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-600 mb-1">Área Responsable</p>
                    <p className="font-semibold text-gray-900">{selectedRecord.area}</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-600 mb-1">Observaciones</p>
                    <p className="font-semibold text-gray-900">{selectedRecord.observaciones}</p>
                  </div>
                </div>
              </div>

              {/* Información de Pago */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-4 py-3 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <CreditCard className="w-5 h-5" />
                  <h3 className="font-semibold">Información de Pago</h3>
                </div>
                <div className="p-4">
                  <div className="flex items-center gap-3 p-3 rounded-lg" style={{ backgroundColor: '#22C55E20' }}>
                    <CheckCircle2 className="w-6 h-6 text-green-600" />
                    <div>
                      <p className="font-semibold text-gray-900">Pago Verificado</p>
                      <p className="text-sm text-gray-600">Total pagado: S/ 1,035.00</p>
                      <p className="text-xs text-gray-500 mt-1">Fecha: 05/03/2026 - Comprobante: 00000123456</p>
                    </div>
                  </div>
                </div>
              </div>

              {/* Documentos Presentados */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-4 py-3 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <FileText className="w-5 h-5" />
                  <h3 className="font-semibold">Documentos Presentados</h3>
                </div>
                <div className="p-4 space-y-2">
                  {[
                    'DNI o Documento de Identidad',
                    'Certificado de Estudios',
                    'Título Profesional',
                    'Constancia de No Inhabilitación',
                    'Foto tamaño pasaporte'
                  ].map((doc, index) => (
                    <div key={index} className="flex items-center gap-3 p-2 hover:bg-gray-50 rounded">
                      <CheckCircle2 className="w-4 h-4 text-green-600" />
                      <span className="text-sm text-gray-700">{doc}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}

          <div className="mt-6 flex justify-end gap-3 pt-4 border-t px-6 pb-6">
            <Button
              variant="outline"
              onClick={handleClose}
            >
              Cerrar
            </Button>
            <Button
              className="text-white"
              style={{ backgroundColor: '#78276B' }}
              onClick={handleClose}
            >
              Imprimir Solicitud
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* History Dialog */}
      <Dialog open={historyOpen} onOpenChange={setHistoryOpen}>
        <DialogContent className="w-[90vw] max-w-none h-[85vh] max-h-[85vh] overflow-y-auto p-0">
          <DialogHeader className="px-6 pt-6 pb-4">
            <div className="flex items-start gap-4">
              <div className="relative">
                <ImageWithFallback
                  src="https://images.unsplash.com/photo-1629507208649-70919ca33793?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxwcm9mZXNzaW9uYWwlMjBtYW4lMjBwb3J0cmFpdCUyMGJ1c2luZXNzfGVufDF8fHx8MTc3MjY4OTA4MXww&ixlib=rb-4.1.0&q=80&w=1080"
                  alt={selectedRecord?.nombre || "Usuario"}
                  className="w-20 h-20 rounded-full object-cover border-4"
                  style={{ borderColor: '#78276B' }}
                />
                <div className="absolute -bottom-1 -right-1 w-6 h-6 rounded-full bg-green-500 border-2 border-white"></div>
              </div>
              <div className="flex-1">
                <DialogTitle className="text-2xl mb-1">Historial de Solicitud</DialogTitle>
                <DialogDescription className="text-base">
                  N° {selectedRecord?.numeroSolicitud} - {selectedRecord?.fechaSolicitud}
                </DialogDescription>
                <p className="text-sm font-semibold text-gray-700 mt-2">{selectedRecord?.nombre}</p>
              </div>
            </div>
          </DialogHeader>

          {selectedRecord && (
            <div className="space-y-6 px-6">
              {/* Estado Actual */}
              <div className="p-4 rounded-lg border-2" style={{ borderColor: '#3B82F6', backgroundColor: '#EFF6FF' }}>
                <div className="flex items-center gap-3">
                  <Shield className="w-6 h-6 text-blue-600" />
                  <div>
                    <p className="text-sm font-semibold text-gray-900">Estado Actual</p>
                    <p className="text-lg font-bold text-blue-600">{selectedRecord.estado}</p>
                  </div>
                </div>
              </div>

              {/* Historial de Eventos */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-4 py-3 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <History className="w-5 h-5" />
                  <h3 className="font-semibold">Historial de Eventos</h3>
                </div>
                <div className="p-6">
                  <div className="space-y-6 relative">
                    {/* Timeline Line */}
                    <div className="absolute left-6 top-8 bottom-8 w-0.5" style={{ backgroundColor: '#78276B40' }}></div>
                    
                    {historyEvents.map((event, index) => (
                      <div key={event.id} className="relative flex gap-6">
                        {/* Icon */}
                        <div className="flex-shrink-0 relative z-10">
                          <div 
                            className="w-12 h-12 rounded-full flex items-center justify-center border-4 border-white shadow-md"
                            style={{ 
                              backgroundColor: event.completado ? '#22C55E' : '#F8AD1D'
                            }}
                          >
                            {event.completado ? (
                              <CheckCircle2 className="w-6 h-6 text-white" />
                            ) : (
                              <Clock className="w-6 h-6 text-white" />
                            )}
                          </div>
                        </div>
                        
                        {/* Content Card */}
                        <div 
                          className="flex-1 rounded-lg border-2 p-4 shadow-sm hover:shadow-md transition-shadow"
                          style={{ 
                            borderColor: event.completado ? '#22C55E40' : '#F8AD1D40',
                            backgroundColor: event.completado ? '#F0FDF4' : '#FFFBEB'
                          }}
                        >
                          <div className="flex items-start justify-between gap-4 mb-2">
                            <h4 className="font-semibold text-gray-900">{event.estado}</h4>
                            <Badge 
                              variant="default"
                              className="text-xs font-medium text-white"
                              style={{ 
                                backgroundColor: event.completado ? '#22C55E' : '#F8AD1D' 
                              }}
                            >
                              {event.completado ? 'Completado' : 'En Proceso'}
                            </Badge>
                          </div>
                          
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-3 text-sm">
                            <div className="flex items-center gap-2 text-gray-700">
                              <Calendar className="w-4 h-4 text-gray-500" />
                              <span>{event.fecha}</span>
                            </div>
                            <div className="flex items-center gap-2 text-gray-700">
                              <Clock className="w-4 h-4 text-gray-500" />
                              <span>{event.hora}</span>
                            </div>
                            <div className="flex items-center gap-2 text-gray-700">
                              <User className="w-4 h-4 text-gray-500" />
                              <span>{event.responsable}</span>
                            </div>
                            <div className="flex items-center gap-2 text-gray-700">
                              <MapPin className="w-4 h-4 text-gray-500" />
                              <span>{event.area}</span>
                            </div>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          )}

          <div className="mt-6 flex justify-end gap-3 pt-4 border-t px-6 pb-6">
            <Button
              variant="outline"
              onClick={handleHistoryClose}
            >
              Cerrar
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}