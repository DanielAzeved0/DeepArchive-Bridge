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
    <div className="flex flex-col min-h-screen bg-gray-900">
      <div className="flex flex-1">
        {/* Sidebar */}
        <aside className="w-64 bg-gray-800 shadow-lg p-6 border-r border-gray-700">
          <div className="flex items-center gap-3 mb-8">
            <span className="text-3xl">📦</span>
            <div>
              <h1 className="text-xl font-bold text-white">DeepArchive</h1>
              <p className="text-xs text-gray-400">Bridge</p>
            </div>
          </div>

          <nav className="space-y-2 mb-8">
            <a href="/" className="flex items-center gap-3 px-4 py-3 rounded-lg hover:bg-gray-700 text-gray-100 transition">
              🏠 <span>Dashboard</span>
            </a>
            <a href="/vendas" className="flex items-center gap-3 px-4 py-3 rounded-lg hover:bg-gray-700 text-gray-100 transition">
              📋 <span>Vendas</span>
            </a>
            <a href="/arquivamento" className="flex items-center gap-3 px-4 py-3 rounded-lg hover:bg-gray-700 text-gray-100 transition">
              🗂️ <span>Arquivamento</span>
            </a>
          </nav>

          <hr className="border-gray-700 mb-6" />

          <div className="space-y-2">
            <p className="px-4 py-2 text-gray-400 font-semibold text-xs uppercase tracking-wider">⚙️ Admin</p>
            <a href="/admin/health" className="flex items-center gap-3 px-4 py-3 rounded-lg hover:bg-gray-700 text-gray-100 transition">
              🏥 <span>Status da API</span>
            </a>
            <a href="/config" className="flex items-center gap-3 px-4 py-3 rounded-lg hover:bg-gray-700 text-gray-100 transition">
              ⚙️ <span>Configuração</span>
            </a>
          </div>

          {/* Health Status */}
          <div className="mt-10 pt-6 border-t border-gray-700">
            <div className="flex items-center gap-2 px-4 py-2 bg-gray-700 rounded-lg">
              <span className={`inline-block w-2 h-2 rounded-full ${isHealthy ? 'bg-green-400' : 'bg-red-400'} animate-pulse`}></span>
              <span className="text-xs text-gray-300">
                {isHealthy ? '✅ Online' : '⚠️ Offline'}
              </span>
            </div>
          </div>
        </aside>

        {/* Main Content */}
        <main className="flex-1 overflow-auto">
          <div className="bg-gradient-to-br from-gray-800 to-gray-900 min-h-screen p-8">
            {children}
          </div>
        </main>
      </div>

      {/* Footer */}
      <footer className="bg-gray-800 border-t border-gray-700 px-8 py-4 flex items-center justify-between">
        <p className="text-xs text-gray-400">© 2026 DeepArchive-Bridge • v1.0.0</p>
        <div className="flex items-center gap-2">
          <span className={`inline-block w-2 h-2 rounded-full ${isHealthy ? 'bg-green-400' : 'bg-red-400'}`}></span>
          <span className="text-xs text-gray-300">
            {isHealthy ? '✅ API Operacional' : '⚠️ API Indisponível'}
          </span>
        </div>
      </footer>
    </div>
  )
}
