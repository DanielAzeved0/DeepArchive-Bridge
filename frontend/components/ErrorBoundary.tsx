'use client'

import React, { ReactNode } from 'react'

interface ErrorBoundaryProps {
  children: ReactNode
}

interface ErrorBoundaryState {
  hasError: boolean
  error: Error | null
}

/**
 * Error Boundary Component
 * Captura erros em tempo de renderização e exibe fallback UI
 * em vez de quebrar toda a página
 */
export class ErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props)
    this.state = { hasError: false, error: null }
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error }
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('ErrorBoundary capturou um erro:', error, errorInfo)
    // Aqui você poderia enviar o erro para um serviço de logging
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="min-h-screen bg-red-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-lg shadow-lg p-8 max-w-md">
            <h1 className="text-2xl font-bold text-red-600 mb-4">⚠️ Erro na Aplicação</h1>
            <p className="text-gray-700 mb-4">
              Desculpe, algo deu errado. Por favor, recarregue a página ou entre em contato com o suporte.
            </p>
            <details className="mb-4 text-sm text-gray-600 bg-gray-50 p-3 rounded border border-gray-200">
              <summary className="cursor-pointer font-semibold">Detalhes do erro</summary>
              <pre className="mt-2 overflow-auto text-xs">{this.state.error?.message}</pre>
            </details>
            <button
              onClick={() => window.location.reload()}
              className="w-full bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition"
            >
              🔄 Recarregar Página
            </button>
          </div>
        </div>
      )
    }

    return this.props.children
  }
}

export default ErrorBoundary
