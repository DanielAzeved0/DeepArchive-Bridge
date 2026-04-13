'use client'

import { useState, useEffect, ReactNode } from 'react'
import { healthService } from '@/lib/api'

export default function ClientLayout({ children }: { children: ReactNode }) {
  const [isHealthy, setIsHealthy] = useState(true)

  useEffect(() => {
    const checkHealth = async () => {
      try {
        const response = await healthService.status()
        setIsHealthy(response.sucesso && response.dados?.status === 'Healthy')
      } catch {
        setIsHealthy(false)
      }
    }

    checkHealth()
    const interval = setInterval(checkHealth, 30000)
    return () => clearInterval(interval)
  }, [])

  return (
    <div className="flex flex-col min-h-screen bg-gray-50">
      <div className="flex flex-1">
        {/* Sidebar */}
        <aside className="w-64 bg-white shadow-md p-6">
          <h1 className="text-2xl font-bold text-blue-600 mb-8">
            📦 DeepArchive
          </h1>
          <nav className="space-y-4 mb-8">
            <a href="/" className="block px-4 py-2 rounded hover:bg-blue-50">
              🏠 Dashboard
            </a>
            <a href="/vendas" className="block px-4 py-2 rounded hover:bg-blue-50">
              📋 Vendas
            </a>
            <a href="/arquivamento" className="block px-4 py-2 rounded hover:bg-blue-50">
              🗂️ Arquivamento
            </a>
          </nav>

          <hr className="mb-4" />
          <nav className="space-y-2 text-sm">
            <p className="px-4 py-2 text-gray-600 font-semibold">⚙️ Admin</p>
            <a href="/admin/health" className="block px-4 py-2 rounded hover:bg-blue-50 text-gray-700">
              🏥 Status
            </a>
            <a href="/config" className="block px-4 py-2 rounded hover:bg-blue-50 text-gray-700">
              ⚙️ Configuração
            </a>
          </nav>
        </aside>

        {/* Main Content */}
        <main className="flex-1 p-8">
          {children}
        </main>
      </div>

      {/* Footer */}
      <footer className="bg-white border-t border-gray-200 px-8 py-4 flex items-center justify-between">
        <p className="text-sm text-gray-600">© 2026 DeepArchive-Bridge</p>
        <div className="flex items-center gap-2">
          <span className={`inline-block w-3 h-3 rounded-full ${isHealthy ? 'bg-green-500' : 'bg-red-500'}`}></span>
          <span className="text-sm text-gray-700">
            {isHealthy ? '✅ API Operacional' : '⚠️ API Indisponível'}
          </span>
        </div>
      </footer>
    </div>
  )
}
