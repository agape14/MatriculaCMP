import { useState } from 'react';
import { CheckCircle, Circle, Info, User, GraduationCap, FileCheck, CreditCard, Send, AlertCircle, MapPin, Phone, Mail, Upload, FileText, Camera, Shield } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './ui/select';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from './ui/dialog';
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from './ui/breadcrumb';
import { Checkbox } from './ui/checkbox';

const steps = [
  { id: 1, name: 'Registro', icon: User },
  { id: 2, name: 'Ética', icon: FileCheck },
  { id: 3, name: 'Validación', icon: CheckCircle },
  { id: 4, name: 'Pago', icon: CreditCard },
  { id: 5, name: 'Solicitud enviada', icon: Send },
];

export function RegistrationView() {
  const [currentStep, setCurrentStep] = useState(1);
  const [showInfoModal, setShowInfoModal] = useState(true);
  const [selectedProfile, setSelectedProfile] = useState('');
  const [universityOrigin, setUniversityOrigin] = useState('nacional');
  const [selectedCountry, setSelectedCountry] = useState('peru');
  const [selectedUniversity, setSelectedUniversity] = useState('');
  const [validationType, setValidationType] = useState('');
  const [courseProgress, setCourseProgress] = useState(0);
  const [isVerifying, setIsVerifying] = useState(false);
  const [paymentVerified, setPaymentVerified] = useState(false);
  const [isVerifyingPayment, setIsVerifyingPayment] = useState(false);

  const handleVerifyProgress = () => {
    setIsVerifying(true);
    // Simular verificación del progreso
    setTimeout(() => {
      // Simular que el curso está completo (en producción esto vendría de la API)
      const newProgress = Math.random() > 0.3 ? 100 : Math.floor(Math.random() * 90);
      setCourseProgress(newProgress);
      setIsVerifying(false);
    }, 2000);
  };

  const handleGoToVirtualClassroom = () => {
    // Abrir el aula virtual en una nueva pestaña
    window.open('https://aulavirtual.example.com', '_blank');
  };

  const handleGoToPaymentGateway = () => {
    // Abrir la pasarela de pago en una nueva pestaña
    window.open('https://pagos.example.com', '_blank');
  };

  const handleVerifyPayment = () => {
    setIsVerifyingPayment(true);
    // Simular verificación del comprobante de pago según los datos ingresados
    setTimeout(() => {
      // Simular que el pago fue verificado exitosamente (en producción esto vendría de la API)
      const paymentFound = Math.random() > 0.3;
      setPaymentVerified(paymentFound);
      setIsVerifyingPayment(false);
    }, 2500);
  };

  const countries = [
    'Argentina', 'Bolivia', 'Brasil', 'Chile', 'Colombia', 'Costa Rica', 'Cuba', 'Ecuador', 
    'El Salvador', 'España', 'Estados Unidos', 'Guatemala', 'Honduras', 'México', 'Nicaragua', 
    'Panamá', 'Paraguay', 'Puerto Rico', 'República Dominicana', 'Uruguay', 'Venezuela',
    'Alemania', 'Australia', 'Austria', 'Bélgica', 'Canadá', 'China', 'Corea del Sur', 
    'Dinamarca', 'Finlandia', 'Francia', 'Grecia', 'India', 'Irlanda', 'Italia', 'Japón',
    'Noruega', 'Nueva Zelanda', 'Países Bajos', 'Polonia', 'Portugal', 'Reino Unido', 
    'Rusia', 'Suecia', 'Suiza', 'Otro'
  ].sort();

  const universitiesByCountry: Record<string, string[]> = {
    'peru': [
      'Universidad Nacional Mayor de San Marcos',
      'Pontificia Universidad Católica del Perú',
      'Universidad Nacional de Ingeniería',
      'Universidad Nacional Agraria La Molina',
      'Universidad Peruana Cayetano Heredia',
      'Universidad del Pacífico',
      'Universidad de Lima',
      'Universidad Nacional de Trujillo',
      'Universidad Nacional San Agustín de Arequipa',
      'Universidad Nacional del Altiplano',
      'Universidad Ricardo Palma',
      'Universidad San Martín de Porres',
      'Universidad Peruana de Ciencias Aplicadas',
      'Universidad ESAN',
      'Universidad Continental',
      'Otra universidad peruana'
    ],
    'argentina': [
      'Universidad de Buenos Aires',
      'Universidad Nacional de Córdoba',
      'Universidad Nacional de La Plata',
      'Universidad Tecnológica Nacional',
      'Universidad Nacional del Litoral',
      'Universidad Nacional de Rosario',
      'Otra universidad argentina'
    ],
    'brasil': [
      'Universidade de São Paulo',
      'Universidade Federal do Rio de Janeiro',
      'Universidade Estadual de Campinas',
      'Universidade Federal de Minas Gerais',
      'Universidade de Brasília',
      'Otra universidad brasileña'
    ],
    'chile': [
      'Universidad de Chile',
      'Pontificia Universidad Católica de Chile',
      'Universidad de Santiago de Chile',
      'Universidad de Concepción',
      'Universidad Técnica Federico Santa María',
      'Otra universidad chilena'
    ],
    'colombia': [
      'Universidad Nacional de Colombia',
      'Universidad de los Andes',
      'Universidad de Antioquia',
      'Universidad del Valle',
      'Pontificia Universidad Javeriana',
      'Otra universidad colombiana'
    ],
    'mexico': ['méxico'],
    'españa': [
      'Universidad Complutense de Madrid',
      'Universidad de Barcelona',
      'Universidad Autónoma de Madrid',
      'Universidad de Valencia',
      'Universidad Politécnica de Madrid',
      'Universidad de Sevilla',
      'Universidad Autónoma de Barcelona',
      'Otra universidad española'
    ],
    'estados-unidos': [
      'Harvard University',
      'Stanford University',
      'Massachusetts Institute of Technology',
      'University of California',
      'Yale University',
      'Princeton University',
      'Columbia University',
      'Otra universidad estadounidense'
    ],
    'reino-unido': [
      'University of Oxford',
      'University of Cambridge',
      'Imperial College London',
      'University College London',
      'London School of Economics',
      'Otra universidad británica'
    ],
    'francia': [
      'Université Paris-Sorbonne',
      'École Polytechnique',
      'Sciences Po',
      'Université Paris-Saclay',
      'Otra universidad francesa'
    ],
    'alemania': [
      'Technische Universität München',
      'Ludwig-Maximilians-Universität München',
      'Ruprecht-Karls-Universität Heidelberg',
      'Humboldt-Universität zu Berlin',
      'Otra universidad alemana'
    ]
  };

  const getUniversitiesForCountry = (country: string): string[] => {
    const universities = universitiesByCountry[country];
    if (universities && universities.length > 0) {
      return universities;
    }
    return ['Especifique manualmente', 'Otra universidad'];
  };

  return (
    <div className="p-8 space-y-6 bg-gradient-to-b from-purple-50/30 to-white min-h-screen">
      {/* Breadcrumb */}
      <Breadcrumb>
        <BreadcrumbList>
          <BreadcrumbItem>
            <BreadcrumbLink href="#" onClick={() => {}}>Inicio</BreadcrumbLink>
          </BreadcrumbItem>
          <BreadcrumbSeparator />
          <BreadcrumbItem>
            <BreadcrumbPage>Ficha Pre-matrícula</BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      {/* Header */}
      <div className="flex items-center gap-4">
        <div className="w-12 h-12 rounded-lg flex items-center justify-center" style={{ backgroundColor: '#78276B' }}>
          <GraduationCap className="w-6 h-6 text-white" />
        </div>
        <div>
          <h1 className="text-3xl font-semibold text-gray-900">FICHA PRE-MATRÍCULA</h1>
          <p className="text-sm text-gray-600 mt-1">Complete los datos requeridos para su inscripción</p>
        </div>
      </div>

      {/* Progress Steps */}
      <Card className="max-w-5xl mx-auto shadow-lg border-2" style={{ borderColor: '#78276B20' }}>
        <CardContent className="p-8">
          <div className="flex items-center justify-between">
            {steps.map((step, index) => {
              const Icon = step.icon;
              return (
                <div key={step.id} className="flex items-center flex-1">
                  <div className="flex flex-col items-center flex-1">
                    <div
                      className={`w-16 h-16 rounded-full flex items-center justify-center border-3 transition-all shadow-md relative ${
                        step.id < currentStep
                          ? 'border-[#78276B]'
                          : step.id === currentStep
                          ? 'border-[#F8AD1D]'
                          : 'bg-white border-gray-300'
                      }`}
                      style={
                        step.id < currentStep
                          ? { backgroundColor: '#78276B', borderWidth: '3px' }
                          : step.id === currentStep
                          ? { backgroundColor: '#F8AD1D', borderWidth: '3px' }
                          : { borderWidth: '3px' }
                      }
                    >
                      {step.id < currentStep ? (
                        <CheckCircle className="w-8 h-8 text-white" />
                      ) : step.id === currentStep ? (
                        <Icon className="w-8 h-8 text-white" />
                      ) : (
                        <Icon className="w-8 h-8 text-gray-400" />
                      )}
                      {step.id === currentStep && (
                        <div className="absolute -inset-1 rounded-full animate-pulse" style={{ backgroundColor: '#F8AD1D40' }} />
                      )}
                    </div>
                    <span
                      className={`mt-3 text-sm text-center font-medium ${
                        step.id <= currentStep ? 'text-gray-900' : 'text-gray-500'
                      }`}
                    >
                      {step.name}
                    </span>
                    {step.id === currentStep && (
                      <span className="mt-1 text-xs px-3 py-1 rounded-full text-white font-medium" style={{ backgroundColor: '#F8AD1D' }}>
                        Paso {step.id}/5
                      </span>
                    )}
                  </div>
                  {index < steps.length - 1 && (
                    <div
                      className="h-1 flex-1 transition-all rounded-full mx-2"
                      style={{ backgroundColor: step.id < currentStep ? '#78276B' : '#e5e7eb' }}
                    />
                  )}
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>

      {/* Alert Card */}
      <Card className="max-w-6xl mx-auto border-l-4" style={{ borderLeftColor: '#F8AD1D', backgroundColor: '#FFF9E6' }}>
        <CardContent className="p-4 flex items-start gap-3">
          <AlertCircle className="w-5 h-5 mt-0.5 flex-shrink-0" style={{ color: '#F8AD1D' }} />
          <div>
            <p className="text-sm text-gray-800">
              <strong>Importante:</strong> Los campos marcados con <span className="text-red-600">*</span> son obligatorios. 
              Su perfil profesional determinará los requisitos específicos del curso de Ética.
            </p>
          </div>
        </CardContent>
      </Card>

      {/* Profile Selection - Destacado */}
      {currentStep === 1 && (
        <Card className="max-w-6xl mx-auto shadow-xl border-2" style={{ borderColor: '#78276B' }}>
          <div className="text-white px-6 py-4 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
            <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center">
              <User className="w-5 h-5 text-white" />
            </div>
            <div>
              <h3 className="font-semibold text-lg">Paso 1: Selección de Perfil</h3>
              <p className="text-sm text-white/90">Seleccione su perfil profesional y consejo regional</p>
            </div>
          </div>
          <CardContent className="p-6 space-y-6">
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <div className="space-y-2">
                <Label className="text-sm font-semibold text-gray-900 flex items-center gap-2">
                  Seleccione su perfil <span className="text-red-600">*</span>
                  {selectedProfile && (
                    <span className="text-xs px-2 py-0.5 rounded text-white ml-auto" style={{ backgroundColor: '#78276B' }}>
                      Seleccionado
                    </span>
                  )}
                </Label>
                <Select value={selectedProfile} onValueChange={setSelectedProfile}>
                  <SelectTrigger className="h-12 border-2 hover:border-[#78276B] transition-colors" style={selectedProfile ? { borderColor: '#78276B' } : {}}>
                    <SelectValue placeholder="Seleccione su perfil profesional" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="peruano-titulo-peruano">Profesional peruano con título de universidad peruana</SelectItem>
                    <SelectItem value="peruano-titulo-extranjero">Profesional peruano con título de universidad extranjera</SelectItem>
                    <SelectItem value="extranjero-titulo-peru">Profesional extranjero con título otorgado en el Perú</SelectItem>
                    <SelectItem value="extranjero-titulo-extranjero">Profesional extranjero con título otorgado en el extranjero</SelectItem>
                  </SelectContent>
                </Select>
                <p className="text-xs text-gray-600 flex items-start gap-1">
                  <Info className="w-3 h-3 mt-0.5 flex-shrink-0" />
                  El perfil guía los requisitos del curso de Ética y la documentación necesaria
                </p>
              </div>

              <div className="space-y-2">
                <Label className="text-sm font-semibold text-gray-900">
                  Consejo Regional <span className="text-red-600">*</span>
                </Label>
                <Select defaultValue="">
                  <SelectTrigger className="h-12 border-2 hover:border-[#78276B] transition-colors">
                    <SelectValue placeholder="Seleccione su consejo regional" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="lima">Lima</SelectItem>
                    <SelectItem value="arequipa">Arequipa</SelectItem>
                    <SelectItem value="cusco">Cusco</SelectItem>
                    <SelectItem value="trujillo">Trujillo</SelectItem>
                    <SelectItem value="piura">Piura</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Form */}
      <Card className="max-w-6xl mx-auto shadow-lg">
        <CardContent className="p-0">
          {currentStep === 1 && (
            <>
              {/* Datos Personales */}
              <div>
                <div className="text-white px-6 py-4 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center">
                    <User className="w-4 h-4 text-white" />
                  </div>
                  <h3 className="font-semibold text-lg">Paso 2: Datos Personales</h3>
                </div>

                <div className="p-6 grid grid-cols-1 md:grid-cols-3 gap-6">
                  <div className="space-y-2">
                    <Label htmlFor="docType" className="text-sm font-medium text-gray-900">
                      Tipo de Documento <span className="text-red-600">*</span>
                    </Label>
                    <Select defaultValue="dni">
                      <SelectTrigger id="docType" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="dni">DNI</SelectItem>
                        <SelectItem value="ce">Carnet de Extranjería</SelectItem>
                        <SelectItem value="pasaporte">Pasaporte</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="docNumber" className="text-sm font-medium text-gray-900">
                      Número de Documento <span className="text-red-600">*</span>
                    </Label>
                    <Input id="docNumber" type="text" className="h-11 border-2 hover:border-[#78276B] transition-colors" placeholder="Ej: 12345678" />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="bloodType" className="text-sm font-medium text-gray-900">
                      Grupo Sanguíneo <span className="text-red-600">*</span>
                    </Label>
                    <Select defaultValue="o+">
                      <SelectTrigger id="bloodType" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="o+">O+</SelectItem>
                        <SelectItem value="o-">O-</SelectItem>
                        <SelectItem value="a+">A+</SelectItem>
                        <SelectItem value="a-">A-</SelectItem>
                        <SelectItem value="b+">B+</SelectItem>
                        <SelectItem value="b-">B-</SelectItem>
                        <SelectItem value="ab+">AB+</SelectItem>
                        <SelectItem value="ab-">AB-</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="firstName" className="text-sm font-medium text-gray-900">
                      Nombres Completos <span className="text-red-600">*</span>
                    </Label>
                    <Input id="firstName" type="text" className="h-11 border-2 hover:border-[#78276B] transition-colors" placeholder="Ej: Juan Carlos" />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="lastName" className="text-sm font-medium text-gray-900">
                      Apellido Paterno <span className="text-red-600">*</span>
                    </Label>
                    <Input id="lastName" type="text" className="h-11 border-2 hover:border-[#78276B] transition-colors" placeholder="Ej: Pérez" />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="motherName" className="text-sm font-medium text-gray-900">
                      Apellido Materno <span className="text-red-600">*</span>
                    </Label>
                    <Input id="motherName" type="text" className="h-11 border-2 hover:border-[#78276B] transition-colors" placeholder="Ej: García" />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="gender" className="text-sm font-medium text-gray-900">
                      Sexo <span className="text-red-600">*</span>
                    </Label>
                    <Select defaultValue="masculino">
                      <SelectTrigger id="gender" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="masculino">Masculino</SelectItem>
                        <SelectItem value="femenino">Femenino</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="birthDate" className="text-sm font-medium text-gray-900">
                      Fecha de Nacimiento <span className="text-red-600">*</span>
                    </Label>
                    <Input id="birthDate" type="date" className="h-11 border-2 hover:border-[#78276B] transition-colors" />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="nationality" className="text-sm font-medium text-gray-900">
                      Nacionalidad <span className="text-red-600">*</span>
                    </Label>
                    <Input id="nationality" type="text" className="h-11 border-2 hover:border-[#78276B] transition-colors" defaultValue="Peruana" />
                  </div>
                </div>
              </div>

              {/* Datos de Universidad */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-6 py-4 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center">
                    <GraduationCap className="w-4 h-4 text-white" />
                  </div>
                  <h3 className="font-semibold text-lg">Paso 3: Datos de Universidad</h3>
                </div>

                <div className="p-6 grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-2">
                    <Label htmlFor="universityOrigin" className="text-sm font-medium text-gray-900">
                      Origen de Universidad <span className="text-red-600">*</span>
                    </Label>
                    <Select value={universityOrigin} onValueChange={setUniversityOrigin}>
                      <SelectTrigger id="universityOrigin" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="nacional">Nacional</SelectItem>
                        <SelectItem value="internacional">Internacional</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="universityCountry" className="text-sm font-medium text-gray-900">
                      País de universidad <span className="text-red-600">*</span>
                    </Label>
                    <Select value={universityOrigin === 'nacional' ? 'peru' : undefined} disabled={universityOrigin === 'nacional'} onValueChange={setSelectedCountry}>
                      <SelectTrigger 
                        id="universityCountry" 
                        className="h-11 border-2 hover:border-[#78276B] transition-colors"
                        style={universityOrigin === 'nacional' ? { opacity: 0.6, cursor: 'not-allowed' } : {}}
                      >
                        <SelectValue placeholder="Seleccione un país" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="peru">Perú</SelectItem>
                        {universityOrigin === 'internacional' && countries.map(country => (
                          <SelectItem key={country} value={country.toLowerCase().replace(/\s+/g, '-')}>{country}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    {universityOrigin === 'nacional' && (
                      <p className="text-xs text-gray-600 flex items-start gap-1">
                        <Info className="w-3 h-3 mt-0.5 flex-shrink-0" />
                        Campo bloqueado - Universidad de origen nacional
                      </p>
                    )}
                  </div>

                  <div className="md:col-span-2 space-y-2">
                    <Label htmlFor="university" className="text-sm font-medium text-gray-900">
                      Universidad <span className="text-red-600">*</span>
                    </Label>
                    <Select value={selectedUniversity} onValueChange={setSelectedUniversity}>
                      <SelectTrigger id="university" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                        <SelectValue placeholder="Seleccione una universidad" />
                      </SelectTrigger>
                      <SelectContent>
                        {getUniversitiesForCountry(selectedCountry).map(university => (
                          <SelectItem key={university} value={university.toLowerCase().replace(/\s+/g, '-')}>{university}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <p className="text-xs text-gray-600 flex items-start gap-1">
                      <GraduationCap className="w-3 h-3 mt-0.5 flex-shrink-0" />
                      Universidades disponibles para {universityOrigin === 'nacional' ? 'Perú' : 'el país seleccionado'}
                    </p>
                  </div>

                  <div className="md:col-span-2 space-y-2">
                    <Label htmlFor="validationType" className="text-sm font-medium text-gray-900">
                      Tipo de validación <span className="text-red-600">*</span>
                    </Label>
                    <Select value={validationType} onValueChange={setValidationType}>
                      <SelectTrigger id="validationType" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                        <SelectValue placeholder="Seleccione el tipo de validación" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="reconocimiento">Reconocimiento de SUNEDU</SelectItem>
                        <SelectItem value="revalidacion">Revalidación</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  {/* Campos condicionales para Reconocimiento de SUNEDU */}
                  {validationType === 'reconocimiento' && (
                    <>
                      <div className="space-y-2">
                        <Label htmlFor="resolucionReconocimiento" className="text-sm font-medium text-gray-900">
                          Resolución de Reconocimiento Universidad Nacional <span className="text-red-600">*</span>
                        </Label>
                        <Input 
                          id="resolucionReconocimiento" 
                          type="text" 
                          className="h-11 border-2 hover:border-[#78276B] transition-colors" 
                          placeholder="Ej: R-123-2024-SUNEDU" 
                        />
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor="fechaReconocimiento" className="text-sm font-medium text-gray-900">
                          Fecha de Reconocimiento <span className="text-red-600">*</span>
                        </Label>
                        <Input 
                          id="fechaReconocimiento" 
                          type="date" 
                          className="h-11 border-2 hover:border-[#78276B] transition-colors" 
                        />
                      </div>
                    </>
                  )}

                  {/* Campos condicionales para Revalidación */}
                  {validationType === 'revalidacion' && (
                    <>
                      <div className="md:col-span-2 space-y-2">
                        <Label htmlFor="resolucionRevalidacion" className="text-sm font-medium text-gray-900">
                          Resolución de Revalidación Universidad Nacional <span className="text-red-600">*</span>
                        </Label>
                        <Input 
                          id="resolucionRevalidacion" 
                          type="text" 
                          className="h-11 border-2 hover:border-[#78276B] transition-colors" 
                          placeholder="Ej: RR-456-2024" 
                        />
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor="universidadPeruana" className="text-sm font-medium text-gray-900">
                          Universidad Peruana <span className="text-red-600">*</span>
                        </Label>
                        <Select>
                          <SelectTrigger id="universidadPeruana" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                            <SelectValue placeholder="Seleccione universidad peruana" />
                          </SelectTrigger>
                          <SelectContent>
                            {universitiesByCountry['peru'].map(university => (
                              <SelectItem key={university} value={university.toLowerCase().replace(/\s+/g, '-')}>{university}</SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor="fechaRevalidacion" className="text-sm font-medium text-gray-900">
                          Fecha de Revalidación <span className="text-red-600">*</span>
                        </Label>
                        <Input 
                          id="fechaRevalidacion" 
                          type="date" 
                          className="h-11 border-2 hover:border-[#78276B] transition-colors" 
                        />
                      </div>
                    </>
                  )}
                </div>
              </div>

              {/* Action Buttons */}
              <div className="px-6 py-6 bg-gray-50 border-t flex justify-between items-center">
                <Button variant="outline" size="lg" className="border-2 hover:bg-gray-100">
                  Cancelar
                </Button>
                <Button 
                  className="text-white hover:opacity-90 shadow-lg px-8" 
                  style={{ backgroundColor: '#78276B' }}
                  size="lg"
                  onClick={() => setCurrentStep(Math.min(currentStep + 1, steps.length))}
                >
                  Siguiente
                  <CheckCircle className="w-4 h-4 ml-2" />
                </Button>
              </div>
            </>
          )}

          {currentStep === 2 && (
            <>
              {/* Curso de Ética */}
              <div>
                <div className="text-white px-6 py-4 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center">
                    <FileCheck className="w-4 h-4 text-white" />
                  </div>
                  <h3 className="font-semibold text-lg">Curso de Ética</h3>
                </div>

                <div className="p-8 space-y-6">
                  {/* Mensaje explicativo */}
                  <div className="text-center space-y-3 max-w-2xl mx-auto">
                    <div className="w-20 h-20 rounded-full mx-auto flex items-center justify-center" style={{ backgroundColor: '#F8AD1D20' }}>
                      <FileCheck className="w-10 h-10" style={{ color: '#F8AD1D' }} />
                    </div>
                    <h4 className="text-2xl font-semibold text-gray-900">Complete el Curso de Ética</h4>
                    <p className="text-gray-600">
                      Para continuar con el proceso de pre-matrícula, debe completar el curso de ética en nuestra plataforma de Aula Virtual. 
                      Una vez que haya finalizado el curso al 100%, podrá avanzar al siguiente paso.
                    </p>
                  </div>

                  {/* Indicador de progreso */}
                  {courseProgress > 0 && (
                    <div className="max-w-md mx-auto space-y-3">
                      <div className="flex justify-between items-center">
                        <span className="text-sm font-medium text-gray-700">Progreso del curso</span>
                        <span className="text-sm font-bold" style={{ color: courseProgress === 100 ? '#22C55E' : '#F8AD1D' }}>
                          {courseProgress}%
                        </span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-3 overflow-hidden">
                        <div 
                          className="h-full rounded-full transition-all duration-500"
                          style={{ 
                            width: `${courseProgress}%`, 
                            backgroundColor: courseProgress === 100 ? '#22C55E' : '#F8AD1D'
                          }}
                        />
                      </div>
                      {courseProgress === 100 && (
                        <div className="flex items-center justify-center gap-2 text-green-600 font-medium">
                          <CheckCircle className="w-5 h-5" />
                          <span>¡Curso completado! Puede continuar al siguiente paso</span>
                        </div>
                      )}
                      {courseProgress < 100 && courseProgress > 0 && (
                        <p className="text-sm text-center text-gray-600">
                          Debe completar el curso al 100% para continuar
                        </p>
                      )}
                    </div>
                  )}

                  {/* Botones de acción */}
                  <div className="flex flex-col sm:flex-row items-center justify-center gap-4 max-w-md mx-auto">
                    <Button 
                      className="w-full sm:w-auto text-white hover:opacity-90 shadow-lg px-8" 
                      style={{ backgroundColor: '#78276B' }}
                      size="lg"
                      onClick={handleGoToVirtualClassroom}
                    >
                      <GraduationCap className="w-5 h-5 mr-2" />
                      Ir al aula virtual
                    </Button>

                    <Button 
                      className="w-full sm:w-auto border-2 hover:bg-gray-50 px-8" 
                      style={{ borderColor: '#78276B', color: '#78276B' }}
                      size="lg"
                      variant="outline"
                      onClick={handleVerifyProgress}
                      disabled={isVerifying}
                    >
                      {isVerifying ? (
                        <>
                          <Circle className="w-5 h-5 mr-2 animate-spin" />
                          Verificando...
                        </>
                      ) : (
                        <>
                          <CheckCircle className="w-5 h-5 mr-2" />
                          Verificar Avance
                        </>
                      )}
                    </Button>
                  </div>

                  {/* Información adicional */}
                  <div className="max-w-2xl mx-auto mt-8 p-4 rounded-lg" style={{ backgroundColor: '#F8AD1D20' }}>
                    <div className="flex items-start gap-3">
                      <Info className="w-5 h-5 mt-0.5 flex-shrink-0" style={{ color: '#F8AD1D' }} />
                      <div className="text-sm text-gray-700">
                        <p className="font-medium mb-1">Instrucciones importantes:</p>
                        <ul className="space-y-1 list-disc list-inside">
                          <li>El curso se genera automáticamente con sus datos de registro</li>
                          <li>Debe completar todas las lecciones y aprobar las evaluaciones</li>
                          <li>Una vez completado el 100%, use el botón "Verificar Avance" para continuar</li>
                          <li>El progreso se guarda automáticamente en la plataforma</li>
                        </ul>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="px-6 py-6 bg-gray-50 border-t flex justify-between items-center">
                <Button 
                  variant="outline" 
                  size="lg" 
                  className="border-2 hover:bg-gray-100"
                  onClick={() => setCurrentStep(Math.max(currentStep - 1, 1))}
                >
                  Volver
                </Button>
                <Button 
                  className="text-white shadow-lg px-8" 
                  style={{ backgroundColor: courseProgress === 100 ? '#78276B' : '#9CA3AF' }}
                  size="lg"
                  onClick={() => setCurrentStep(Math.min(currentStep + 1, steps.length))}
                  disabled={courseProgress < 100}
                >
                  Siguiente
                  <CheckCircle className="w-4 h-4 ml-2" />
                </Button>
              </div>
            </>
          )}

          {currentStep === 3 && (
            <>
              {/* Resumen de Registro Inicial */}
              <div>
                <div className="text-white px-6 py-4 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center">
                    <Info className="w-4 h-4 text-white" />
                  </div>
                  <h3 className="font-semibold text-lg">Resumen de Registro Inicial</h3>
                </div>

                <div className="p-6 bg-gray-50">
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
                    <div>
                      <p className="text-gray-600">Nombres Completos</p>
                      <p className="font-semibold text-gray-900">Juan Carlos Pérez García</p>
                    </div>
                    <div>
                      <p className="text-gray-600">Documento</p>
                      <p className="font-semibold text-gray-900">DNI: 12345678</p>
                    </div>
                    <div>
                      <p className="text-gray-600">Perfil Seleccionado</p>
                      <p className="font-semibold text-gray-900">Profesional peruano con título de universidad peruana</p>
                    </div>
                  </div>
                </div>
              </div>

              {/* Datos de Nacimiento */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-6 py-4 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center">
                    <User className="w-4 h-4 text-white" />
                  </div>
                  <h3 className="font-semibold text-lg">Datos de Nacimiento</h3>
                </div>

                <div className="p-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                  <div className="space-y-2">
                    <Label htmlFor="birthCountry" className="text-sm font-medium text-gray-900">
                      País <span className="text-red-600">*</span>
                    </Label>
                    <Select defaultValue="peru">
                      <SelectTrigger id="birthCountry" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="peru">Perú</SelectItem>
                        {countries.map(country => (
                          <SelectItem key={country} value={country.toLowerCase().replace(/\s+/g, '-')}>{country}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="birthDepartment" className="text-sm font-medium text-gray-900">
                      Departamento <span className="text-red-600">*</span>
                    </Label>
                    <Select>
                      <SelectTrigger id="birthDepartment" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                        <SelectValue placeholder="Seleccione" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="lima">Lima</SelectItem>
                        <SelectItem value="arequipa">Arequipa</SelectItem>
                        <SelectItem value="cusco">Cusco</SelectItem>
                        <SelectItem value="trujillo">La Libertad</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="birthProvince" className="text-sm font-medium text-gray-900">
                      Provincia <span className="text-red-600">*</span>
                    </Label>
                    <Select>
                      <SelectTrigger id="birthProvince" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                        <SelectValue placeholder="Seleccione" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="lima">Lima</SelectItem>
                        <SelectItem value="callao">Callao</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="birthDistrict" className="text-sm font-medium text-gray-900">
                      Distrito <span className="text-red-600">*</span>
                    </Label>
                    <Select>
                      <SelectTrigger id="birthDistrict" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                        <SelectValue placeholder="Seleccione" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="miraflores">Miraflores</SelectItem>
                        <SelectItem value="san-isidro">San Isidro</SelectItem>
                        <SelectItem value="surco">Surco</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="birthDateValidation" className="text-sm font-medium text-gray-900">
                      Fecha de Nacimiento <span className="text-red-600">*</span>
                    </Label>
                    <Input id="birthDateValidation" type="date" className="h-11 border-2 hover:border-[#78276B] transition-colors" />
                  </div>
                </div>
              </div>

              {/* Datos de Domicilio */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-6 py-4 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center">
                    <MapPin className="w-4 h-4 text-white" />
                  </div>
                  <h3 className="font-semibold text-lg">Datos de Domicilio</h3>
                </div>

                <div className="p-6 space-y-6">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div className="space-y-2">
                      <Label htmlFor="zone" className="text-sm font-medium text-gray-900">
                        Zona <span className="text-red-600">*</span>
                      </Label>
                      <Select>
                        <SelectTrigger id="zone" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                          <SelectValue placeholder="Seleccione" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="urbanizacion">Urbanización</SelectItem>
                          <SelectItem value="asentamiento">Asentamiento Humano</SelectItem>
                          <SelectItem value="pueblo-joven">Pueblo Joven</SelectItem>
                          <SelectItem value="conjunto-habitacional">Conjunto Habitacional</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="zoneDescription" className="text-sm font-medium text-gray-900">
                        Descripción Zona <span className="text-red-600">*</span>
                      </Label>
                      <Input id="zoneDescription" type="text" className="h-11 border-2 hover:border-[#78276B] transition-colors" placeholder="Ej: Santa Patricia" />
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="via" className="text-sm font-medium text-gray-900">
                        Vía <span className="text-red-600">*</span>
                      </Label>
                      <Select>
                        <SelectTrigger id="via" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                          <SelectValue placeholder="Seleccione" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="avenida">Avenida</SelectItem>
                          <SelectItem value="calle">Calle</SelectItem>
                          <SelectItem value="jiron">Jirón</SelectItem>
                          <SelectItem value="pasaje">Pasaje</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="viaDescription" className="text-sm font-medium text-gray-900">
                        Descripción Vía <span className="text-red-600">*</span>
                      </Label>
                      <Input id="viaDescription" type="text" className="h-11 border-2 hover:border-[#78276B] transition-colors" placeholder="Ej: Los Pinos" />
                    </div>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    <div className="space-y-2">
                      <Label htmlFor="addressDepartment" className="text-sm font-medium text-gray-900">
                        Departamento <span className="text-red-600">*</span>
                      </Label>
                      <Select>
                        <SelectTrigger id="addressDepartment" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                          <SelectValue placeholder="Seleccione" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="lima">Lima</SelectItem>
                          <SelectItem value="arequipa">Arequipa</SelectItem>
                          <SelectItem value="cusco">Cusco</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="addressProvince" className="text-sm font-medium text-gray-900">
                        Provincia <span className="text-red-600">*</span>
                      </Label>
                      <Select>
                        <SelectTrigger id="addressProvince" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                          <SelectValue placeholder="Seleccione" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="lima">Lima</SelectItem>
                          <SelectItem value="callao">Callao</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="addressDistrict" className="text-sm font-medium text-gray-900">
                        Distrito <span className="text-red-600">*</span>
                      </Label>
                      <Select>
                        <SelectTrigger id="addressDistrict" className="h-11 border-2 hover:border-[#78276B] transition-colors">
                          <SelectValue placeholder="Seleccione" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="miraflores">Miraflores</SelectItem>
                          <SelectItem value="san-isidro">San Isidro</SelectItem>
                          <SelectItem value="surco">Surco</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    <div className="space-y-2">
                      <Label htmlFor="phoneFixed" className="text-sm font-medium text-gray-900">
                        Teléfono Fijo
                      </Label>
                      <div className="flex gap-2">
                        <Input id="phoneFixed" type="text" className="h-11 border-2 hover:border-[#78276B] transition-colors" placeholder="01 234 5678" />
                        <Phone className="w-10 h-10 p-2 rounded border-2" style={{ borderColor: '#78276B', color: '#78276B' }} />
                      </div>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="phoneMobile" className="text-sm font-medium text-gray-900">
                        Teléfono Celular <span className="text-red-600">*</span>
                      </Label>
                      <div className="flex gap-2">
                        <Input id="phoneMobile" type="text" className="h-11 border-2 hover:border-[#78276B] transition-colors" placeholder="987 654 321" />
                        <Phone className="w-10 h-10 p-2 rounded border-2" style={{ borderColor: '#78276B', color: '#78276B' }} />
                      </div>
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="email" className="text-sm font-medium text-gray-900">
                        Correo Electrónico <span className="text-red-600">*</span>
                      </Label>
                      <div className="flex gap-2">
                        <Input id="email" type="email" className="h-11 border-2 hover:border-[#78276B] transition-colors" placeholder="ejemplo@email.com" />
                        <Mail className="w-10 h-10 p-2 rounded border-2" style={{ borderColor: '#78276B', color: '#78276B' }} />
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Documentos a Adjuntar */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-6 py-4 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center">
                    <FileText className="w-4 h-4 text-white" />
                  </div>
                  <h3 className="font-semibold text-lg">Documentos a Adjuntar</h3>
                </div>

                <div className="p-6 space-y-6">
                  <div className="space-y-2">
                    <Label htmlFor="titleDate" className="text-sm font-medium text-gray-900">
                      Fecha de Titulación <span className="text-red-600">*</span>
                    </Label>
                    <Input id="titleDate" type="date" className="h-11 border-2 hover:border-[#78276B] transition-colors max-w-xs" />
                  </div>

                  <div className="space-y-4">
                    <div className="p-4 border-2 border-dashed rounded-lg hover:border-[#78276B] transition-colors">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 rounded-lg flex items-center justify-center" style={{ backgroundColor: '#78276B20' }}>
                          <Upload className="w-6 h-6" style={{ color: '#78276B' }} />
                        </div>
                        <div className="flex-1">
                          <p className="font-medium text-gray-900">Título Médico Cirujano (ambas caras) <span className="text-red-600">*</span></p>
                          <p className="text-sm text-gray-600">Formato PDF, máximo 10 MB</p>
                        </div>
                        <Button className="text-white" style={{ backgroundColor: '#78276B' }}>
                          <Upload className="w-4 h-4 mr-2" />
                          Adjuntar
                        </Button>
                      </div>
                    </div>

                    <div className="p-4 border-2 border-dashed rounded-lg hover:border-[#78276B] transition-colors">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 rounded-lg flex items-center justify-center" style={{ backgroundColor: '#78276B20' }}>
                          <Upload className="w-6 h-6" style={{ color: '#78276B' }} />
                        </div>
                        <div className="flex-1">
                          <p className="font-medium text-gray-900">Certificado Antecedentes Penales o Certificado Único Laboral <span className="text-red-600">*</span></p>
                          <p className="text-sm text-gray-600">Formato PDF, máximo 10 MB</p>
                        </div>
                        <Button className="text-white" style={{ backgroundColor: '#78276B' }}>
                          <Upload className="w-4 h-4 mr-2" />
                          Adjuntar
                        </Button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Foto a Adjuntar */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-6 py-4 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center">
                    <Camera className="w-4 h-4 text-white" />
                  </div>
                  <h3 className="font-semibold text-lg">Foto a Adjuntar</h3>
                </div>

                <div className="p-6">
                  <div className="max-w-2xl">
                    <div className="p-6 border-2 border-dashed rounded-lg hover:border-[#78276B] transition-colors">
                      <div className="flex flex-col md:flex-row items-start gap-6">
                        <div className="w-32 h-40 bg-gray-100 rounded-lg flex items-center justify-center flex-shrink-0">
                          <Camera className="w-12 h-12 text-gray-400" />
                        </div>
                        <div className="flex-1 space-y-4">
                          <div>
                            <p className="font-medium text-gray-900 mb-2">Foto tamaño pasaporte <span className="text-red-600">*</span></p>
                            <ul className="text-sm text-gray-600 space-y-1">
                              <li>• Formato: JPG o PNG</li>
                              <li>• Tamaño máximo: 4 MB</li>
                              <li>• Fondo blanco</li>
                              <li>• Foto reciente, sin lentes oscuros</li>
                            </ul>
                          </div>
                          <Button className="text-white" style={{ backgroundColor: '#78276B' }}>
                            <Camera className="w-4 h-4 mr-2" />
                            Subir Foto
                          </Button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Validación DDJJ */}
              <div className="border-t-4" style={{ borderColor: '#78276B20' }}>
                <div className="text-white px-6 py-4 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center">
                    <Shield className="w-4 h-4 text-white" />
                  </div>
                  <h3 className="font-semibold text-lg">Validación y Declaración Jurada</h3>
                </div>

                <div className="p-6 space-y-6">
                  <div className="p-4 rounded-lg border-2" style={{ borderColor: '#22C55E', backgroundColor: '#F0FDF4' }}>
                    <div className="flex items-center gap-3">
                      <CheckCircle className="w-6 h-6 text-green-600" />
                      <div>
                        <p className="font-semibold text-gray-900">DDJJ validada con ID PERÚ</p>
                        <p className="text-sm text-gray-600">Su declaración jurada ha sido verificada correctamente</p>
                      </div>
                    </div>
                  </div>

                  <div className="space-y-4">
                    <div className="flex items-start gap-3">
                      <Checkbox id="privacy" className="mt-1" />
                      <Label htmlFor="privacy" className="text-sm text-gray-700 cursor-pointer">
                        Acepto la <span className="font-semibold" style={{ color: '#78276B' }}>Política de Privacidad</span> y el tratamiento de mis datos personales según la Ley de Protección de Datos Personales.
                      </Label>
                    </div>

                    <div className="flex items-start gap-3">
                      <Checkbox id="declaration" className="mt-1" />
                      <Label htmlFor="declaration" className="text-sm text-gray-700 cursor-pointer">
                        Declaro bajo juramento que toda la información proporcionada es verídica y me comprometo a presentar la documentación original cuando sea requerida.
                      </Label>
                    </div>
                  </div>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="px-6 py-6 bg-gray-50 border-t flex justify-between items-center">
                <Button 
                  variant="outline" 
                  size="lg" 
                  className="border-2 hover:bg-gray-100"
                  onClick={() => setCurrentStep(Math.max(currentStep - 1, 1))}
                >
                  Volver
                </Button>
                <Button 
                  className="text-white hover:opacity-90 shadow-lg px-8" 
                  style={{ backgroundColor: '#78276B' }}
                  size="lg"
                  onClick={() => setCurrentStep(Math.min(currentStep + 1, steps.length))}
                >
                  Continuar al Pago
                  <CreditCard className="w-4 h-4 ml-2" />
                </Button>
              </div>
            </>
          )}

          {currentStep === 4 && (
            <>
              {/* Pago */}
              <div>
                <div className="text-white px-6 py-4 flex items-center gap-3" style={{ background: 'linear-gradient(135deg, #78276B 0%, #9333AB 100%)' }}>
                  <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center">
                    <CreditCard className="w-4 h-4 text-white" />
                  </div>
                  <h3 className="font-semibold text-lg">Paso 4: Pago</h3>
                </div>

                <div className="p-8 space-y-6">
                  {/* Mensaje explicativo */}
                  <div className="text-center space-y-3 max-w-2xl mx-auto">
                    <div className="w-20 h-20 rounded-full mx-auto flex items-center justify-center" style={{ backgroundColor: '#F8AD1D20' }}>
                      <CreditCard className="w-10 h-10" style={{ color: '#F8AD1D' }} />
                    </div>
                    <h4 className="text-2xl font-semibold text-gray-900">Realice el Pago de Pre-matrícula</h4>
                    <p className="text-gray-600">
                      Para completar su proceso de pre-matrícula, debe realizar el pago correspondiente a través de nuestra pasarela de pagos segura. 
                      Una vez efectuado el pago, use el botón "Verificar Comprobante" para validar su transacción.
                    </p>
                  </div>

                  {/* Resumen de pago */}
                  <div className="max-w-md mx-auto">
                    <div className="p-6 rounded-lg border-2" style={{ borderColor: '#78276B', backgroundColor: '#78276B10' }}>
                      <div className="space-y-3">
                        <div className="flex justify-between items-center">
                          <span className="text-gray-700">Colegiatura</span>
                          <span className="font-semibold text-gray-900">S/ 1,000.00</span>
                        </div>
                        <div className="flex justify-between items-center">
                          <span className="text-gray-700">1era Cuota</span>
                          <span className="font-semibold text-gray-900">S/ 35.00</span>
                        </div>
                        <div className="border-t-2 pt-3 flex justify-between items-center">
                          <span className="text-lg font-bold text-gray-900">Total a Pagar</span>
                          <span className="text-2xl font-bold" style={{ color: '#78276B' }}>S/ 1,035.00</span>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Estado de verificación */}
                  {paymentVerified && (
                    <div className="max-w-md mx-auto">
                      <div className="p-4 rounded-lg border-2" style={{ borderColor: '#22C55E', backgroundColor: '#F0FDF4' }}>
                        <div className="flex items-center gap-3">
                          <CheckCircle className="w-6 h-6 text-green-600" />
                          <div>
                            <p className="font-semibold text-gray-900">¡Pago verificado exitosamente!</p>
                            <p className="text-sm text-gray-600">Su comprobante ha sido encontrado y validado</p>
                          </div>
                        </div>
                      </div>
                    </div>
                  )}

                  {paymentVerified === false && isVerifyingPayment === false && (
                    <div className="max-w-md mx-auto">
                      <div className="p-4 rounded-lg border-2" style={{ borderColor: '#EF4444', backgroundColor: '#FEF2F2' }}>
                        <div className="flex items-center gap-3">
                          <AlertCircle className="w-6 h-6 text-red-600" />
                          <div>
                            <p className="font-semibold text-gray-900">No se encontró el comprobante</p>
                            <p className="text-sm text-gray-600">Intente nuevamente en unos minutos o verifique que realizó el pago</p>
                          </div>
                        </div>
                      </div>
                    </div>
                  )}

                  {/* Botones de acción */}
                  <div className="flex flex-col sm:flex-row items-center justify-center gap-4 max-w-md mx-auto">
                    <Button 
                      className="w-full sm:w-auto text-white hover:opacity-90 shadow-lg px-8" 
                      style={{ backgroundColor: '#78276B' }}
                      size="lg"
                      onClick={handleGoToPaymentGateway}
                    >
                      <CreditCard className="w-5 h-5 mr-2" />
                      Ir a Pasarela de Pago
                    </Button>

                    <Button 
                      className="w-full sm:w-auto border-2 hover:bg-gray-50 px-8" 
                      style={{ borderColor: '#78276B', color: '#78276B' }}
                      size="lg"
                      variant="outline"
                      onClick={handleVerifyPayment}
                      disabled={isVerifyingPayment}
                    >
                      {isVerifyingPayment ? (
                        <>
                          <Circle className="w-5 h-5 mr-2 animate-spin" />
                          Verificando...
                        </>
                      ) : (
                        <>
                          <CheckCircle className="w-5 h-5 mr-2" />
                          Verificar Comprobante
                        </>
                      )}
                    </Button>
                  </div>

                  {/* Información adicional */}
                  <div className="max-w-2xl mx-auto mt-8 p-4 rounded-lg" style={{ backgroundColor: '#F8AD1D20' }}>
                    <div className="flex items-start gap-3">
                      <Info className="w-5 h-5 mt-0.5 flex-shrink-0" style={{ color: '#F8AD1D' }} />
                      <div className="text-sm text-gray-700">
                        <p className="font-medium mb-1">Instrucciones importantes:</p>
                        <ul className="space-y-1 list-disc list-inside">
                          <li>El pago se procesa automáticamente según sus datos de registro (DNI/Documento)</li>
                          <li>Puede pagar mediante tarjeta de crédito, débito o transferencia bancaria</li>
                          <li>Una vez realizado el pago, use "Verificar Comprobante" para validar la transacción</li>
                          <li>La verificación puede tardar hasta 5 minutos después del pago</li>
                          <li>Guarde su comprobante de pago para futuras consultas</li>
                        </ul>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="px-6 py-6 bg-gray-50 border-t flex justify-between items-center">
                <Button 
                  variant="outline" 
                  size="lg" 
                  className="border-2 hover:bg-gray-100"
                  onClick={() => setCurrentStep(Math.max(currentStep - 1, 1))}
                >
                  Volver
                </Button>
                <Button 
                  className="text-white shadow-lg px-8" 
                  style={{ backgroundColor: paymentVerified ? '#78276B' : '#9CA3AF' }}
                  size="lg"
                  onClick={() => setCurrentStep(Math.min(currentStep + 1, steps.length))}
                  disabled={!paymentVerified}
                >
                  Enviar Solicitud
                  <Send className="w-4 h-4 ml-2" />
                </Button>
              </div>
            </>
          )}
        </CardContent>
      </Card>

      {/* Info Modal */}
      <Dialog open={showInfoModal} onOpenChange={setShowInfoModal}>
        <DialogContent className="sm:max-w-[600px]">
          <DialogHeader>
            <div className="flex items-center justify-center mb-4">
              <div className="w-16 h-16 rounded-full flex items-center justify-center" style={{ backgroundColor: '#F8AD1D20' }}>
                <Info className="w-8 h-8" style={{ color: '#F8AD1D' }} />
              </div>
            </div>
            <DialogTitle className="text-center text-2xl">
              Requisitos previos para el registro
            </DialogTitle>
            <DialogDescription className="sr-only">
              Información sobre los requisitos necesarios para completar el proceso de pre-matrícula
            </DialogDescription>
          </DialogHeader>
          
          <div className="text-left space-y-3">
            <p className="text-gray-700">Antes de iniciar el registro, asegúrese de tener:</p>
            <ul className="space-y-2 text-gray-600">
              <li className="flex gap-2">
                <span className="font-bold" style={{ color: '#78276B' }}>•</span>
                <span><strong>DNI electrónico:</strong> Necesario para validar su DDJJ mediante ID PERU.</span>
              </li>
              <li className="flex gap-2">
                <span className="font-bold" style={{ color: '#78276B' }}>•</span>
                <span><strong>Paso 2 - Curso de Ética:</strong> Debe completar el curso en el Aula Virtual antes de continuar al paso 3.</span>
              </li>
              <li className="flex gap-2">
                <span className="font-bold" style={{ color: '#78276B' }}>•</span>
                <span><strong>Paso 4 - Pago:</strong> Debe realizar el pago correspondiente para enviar su solicitud.</span>
              </li>
              <li className="flex gap-2">
                <span className="font-bold" style={{ color: '#78276B' }}>•</span>
                <span><strong>Documentos:</strong> Tenga a mano su título profesional, constancias y demás documentos escaneados en formato PDF.</span>
              </li>
              <li className="flex gap-2">
                <span className="font-bold" style={{ color: '#78276B' }}>•</span>
                <span><strong>Foto reciente:</strong> Para cargar en el formulario (formato válido, buena calidad).</span>
              </li>
            </ul>
            <p className="text-sm text-gray-500 italic pt-4">
              Esta información es solo de carácter informativo.
            </p>
          </div>

          <DialogFooter>
            <Button 
              onClick={() => setShowInfoModal(false)}
              className="w-full text-white hover:opacity-90"
              style={{ backgroundColor: '#78276B' }}
            >
              Entendido
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}