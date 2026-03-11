import { useState } from 'react';
import { Document, Page, pdfjs } from 'react-pdf';
import { X } from 'lucide-react';
import { Button } from './ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from './ui/dialog';

// Configurar el worker de PDF.js
pdfjs.GlobalWorkerOptions.workerSrc = `//unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.mjs`;

interface DiplomaViewerModalProps {
  diploma: {
    nombre: string;
    numeroColegiatura: string;
    numeroSolicitud: string;
    dni: string;
    estado: string;
    fechaSolicitud: string;
    fechaEmision: string;
  };
  onClose: () => void;
}

export function DiplomaViewerModal({ diploma, onClose }: DiplomaViewerModalProps) {
  const [numPages, setNumPages] = useState<number>(0);
  const [pageNumber] = useState<number>(1);

  // URL de un PDF de ejemplo - en producción, esto vendría del servidor
  const pdfUrl = 'https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf';

  function onDocumentLoadSuccess({ numPages }: { numPages: number }) {
    setNumPages(numPages);
  }

  // Prevenir clic derecho y selección de texto
  const preventInteraction = (e: React.MouseEvent) => {
    e.preventDefault();
    return false;
  };

  return (
    <Dialog open={true} onOpenChange={onClose}>
      <DialogContent 
        className="max-w-4xl max-h-[90vh] overflow-hidden p-0"
        onContextMenu={preventInteraction}
      >
        <DialogHeader className="px-6 pt-6 pb-4 border-b">
          <div className="flex items-center justify-between">
            <div>
              <DialogTitle className="text-xl font-bold text-gray-900">
                Vista Previa de Diploma
              </DialogTitle>
              <DialogDescription className="text-sm text-gray-600 mt-1">
                {diploma.nombre} - Solicitud #{diploma.numeroSolicitud}
              </DialogDescription>
            </div>
            <Button
              variant="ghost"
              size="sm"
              onClick={onClose}
              className="h-8 w-8 p-0"
            >
              <X className="w-4 h-4" />
            </Button>
          </div>
        </DialogHeader>

        <div 
          className="relative overflow-auto px-6 py-6"
          style={{ maxHeight: 'calc(90vh - 120px)' }}
          onContextMenu={preventInteraction}
          onSelectStart={preventInteraction}
        >
          {/* Marca de agua */}
          <div 
            className="absolute inset-0 pointer-events-none z-10 flex items-center justify-center"
            style={{
              background: 'repeating-linear-gradient(45deg, transparent, transparent 100px, rgba(239, 68, 68, 0.03) 100px, rgba(239, 68, 68, 0.03) 200px)',
            }}
          >
            <div 
              className="text-6xl font-bold opacity-20 select-none"
              style={{
                color: '#EF4444',
                transform: 'rotate(-45deg)',
                textShadow: '2px 2px 4px rgba(0,0,0,0.1)',
                letterSpacing: '0.1em',
              }}
            >
              SIN VALOR LEGAL
            </div>
          </div>

          {/* Contenedor del PDF */}
          <div 
            className="relative bg-white shadow-lg mx-auto"
            style={{ 
              userSelect: 'none',
              WebkitUserSelect: 'none',
              MozUserSelect: 'none',
              msUserSelect: 'none',
            }}
          >
            <Document
              file={pdfUrl}
              onLoadSuccess={onDocumentLoadSuccess}
              loading={
                <div className="flex items-center justify-center h-96">
                  <div className="text-gray-500">Cargando diploma...</div>
                </div>
              }
              error={
                <div className="flex items-center justify-center h-96">
                  <div className="text-red-500">Error al cargar el documento</div>
                </div>
              }
            >
              <Page 
                pageNumber={pageNumber} 
                renderTextLayer={false}
                renderAnnotationLayer={false}
                className="mx-auto"
                width={700}
              />
            </Document>
          </div>

          {/* Información adicional */}
          <div className="mt-4 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
            <p className="text-sm text-yellow-800 font-medium">
              ⚠️ Esta es una vista previa sin valor legal. El documento oficial será emitido tras la aprobación final.
            </p>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}