'use client'

import React, { useState, useEffect } from 'react'
import Link from 'next/link'
import { healthService } from '@/lib/api'
import { HealthStatus } from '@/types'

export default function AdminHealthPage() {
  const [saude, setSaude] = useState<HealthStatus | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [autoRefresh, setAutoRefresh] = useState(true)

  const carregarHealth = async () => {
    try {
      setCarregando(true)
      setErro(null)

      const response = await healthService.status()

      if (response.sucesso) {
        setSaude(response.dados)
      } else {
        setErro(response.mensagem || 'Erro ao carregar health status')
      }
    } catch (error) {
      setErro('Falha ao conectar com o servidor')
      console.error(error)
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    carregarHealth()

    if (!autoRefresh) return

    const intervalo = setInterval(carregarHealth, 5000)
    return () => clearInterval(intervalo)
  }, [autoRefresh])

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Healthy':
        return { bg: 'bg-green-50', text: 'text-green-900', border: 'border-green-500', badge: 'bg-green-100 text-green-800' }
      case 'Degraded':
        return { bg: 'bg-yellow-50', text: 'text-yellow-900', border: 'border-yellow-500', badge: 'bg-yellow-100 text-yellow-800' }
      default:
        return { bg: 'bg-red-50', text: 'text-red-900', border: 'border-red-500', badge: 'bg-red-100 text-red-800' }
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Healthy':
        return '✅'
      case 'Degraded':
        return '⚠️'
      default:
        return '❌'
    }
  }

  if (carregando && !saude) {
    return (
      <div className="min-h-screen bg-gray-50 p-8 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin text-4xl">⏳</div>
          <p className="text-gray-600 mt-4">Buscando status da API...</p>
        </div>
      </div>
    )
  }

  const color = saude ? getStatusColor(saude.status) : getStatusColor('Unhealthy')

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-8">
      {/* Header */}
      <div className="mb-8 flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">🏥 Status da API</h1>
          <p className="text-gray-600 mt-1">Monitoramento em tempo real - Apenas para Administradores</p>
        </div>
        <Link
          href="/"
          className="px-4 py-2 bg-gray-300 hover:bg-gray-400 text-gray-800 rounded-lg font-semibold"
        >
          ← Voltar
        </Link>
      </div>

      {/* Alert */}
      {erro && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
          <span className="text-red-800">❌ {erro}</span>
        </div>
      )}

      {/* Status Principal */}
      {saude && (
        <div className={`bg-white rounded-lg shadow-md border-l-4 ${color.border} p-8 mb-8`}>
          <div className="flex items-center justify-between mb-6">
            <div>
              <h2 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                {getStatusIcon(saude.status)} API Status
              </h2>
              <p className="text-gray-600 mt-1">
                Último check: {new Date(saude.timestamp).toLocaleString('pt-BR')}
              </p>
            </div>
            <div className="text-right">
              <span className={`px-4 py-2 rounded-full font-bold text-lg inline-block ${color.badge}`}>
                {getStatusIcon(saude.status)} {saude.status}
              </span>
            </div>
          </div>

          {/* Grid de Stats */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6 pb-6 border-b">
            <div className="bg-blue-50 p-4 rounded-lg">
              <p className="text-sm text-gray-600 font-medium">Versão da API</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{saude.apiVersion}</p>
            </div>
            <div className="bg-purple-50 p-4 rounded-lg">
              <p className="text-sm text-gray-600 font-medium">Tempo de Atividade</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{Math.floor(saude.uptime / 60)}m</p>
            </div>
            <div className="bg-indigo-50 p-4 rounded-lg">
              <p className="text-sm text-gray-600 font-medium">Memória em Uso</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{saude.memoryMB} MB</p>
            </div>
            <div className="bg-cyan-50 p-4 rounded-lg">
              <p className="text-sm text-gray-600 font-medium">Status</p>
              <p className={`text-2xl font-bold mt-1 ${color.text}`}>{saude.status}</p>
            </div>
          </div>

          {/* Controles */}
          <div className="flex gap-3 flex-wrap">
            <button
              onClick={carregarHealth}
              disabled={carregando}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white rounded-lg font-semibold"
            >
              🔄 Atualizar Agora
            </button>
            <button
              onClick={() => setAutoRefresh(!autoRefresh)}
              className={`px-4 py-2 rounded-lg font-semibold ${
                autoRefresh
                  ? 'bg-green-600 hover:bg-green-700 text-white'
                  : 'bg-gray-300 hover:bg-gray-400 text-gray-800'
              }`}
            >
              {autoRefresh ? '⏸️ Pausar Auto-atualização' : '▶️ Retomar Auto-atualização'}
            </button>
          </div>
        </div>
      )}

      {/* Info */}
      <div className="bg-white rounded-lg shadow-md p-8">
        <h3 className="text-lg font-bold text-gray-900 mb-4">ℹ️ Informações</h3>
        <ul className="text-sm text-gray-700 space-y-2">
          <li>• Esta página é apenas para administradores</li>
          <li>• A página se atualiza automaticamente a cada 5 segundos</li>
          <li>• Você pode pausar a atualização automática usando o botão acima</li>
          <li>• O indicador no rodapé mostra o status em tempo real</li>
        </ul>
      </div>
    </div>
  )
}
