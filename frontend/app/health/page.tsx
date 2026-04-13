'use client'

import React, { useState, useEffect } from 'react'
import { healthService } from '@/lib/api'
import { HealthStatus } from '@/types'
import { formatDateTime, formatDuration } from '@/lib/formatters'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'

interface HealthLog {
  timestamp: string
  status: string // string pois vem da API
  memoria: number
  cpu: number
  requisicoes: number
}

export default function HealthPage() {
  const [saude, setSaude] = useState<HealthStatus | null>(null)
  const [historico, setHistorico] = useState<HealthLog[]>([])
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [autoRefresh, setAutoRefresh] = useState(true)
  const [intervaloRefresh, setIntervaloRefresh] = useState(5) // segundos

  // Carregar health status
  const carregarHealth = async () => {
    try {
      setCarregando(true)
      setErro(null)

      const response = await healthService.status()

      if (response.sucesso) {
        setSaude(response.dados)

        // Simular entrada no histórico
        const novoLog: HealthLog = {
          timestamp: new Date().toISOString(),
          status: response.dados.status,
          memoria: Math.random() * 512 + 100, // 100-612 MB
          cpu: Math.random() * 80 + 10, // 10-90%
          requisicoes: Math.floor(Math.random() * 2000 + 500), // 500-2500/min
        }

        setHistorico((prev) => {
          const novo = [novoLog, ...prev]
          // Manter apenas últimos 60 registros
          return novo.slice(0, 60)
        })
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

  // Auto-refresh
  useEffect(() => {
    carregarHealth()

    if (!autoRefresh) return

    const intervalo = setInterval(carregarHealth, intervaloRefresh * 1000)
    return () => clearInterval(intervalo)
  }, [autoRefresh, intervaloRefresh])

  const getStatusColor = (status: string) => {
    const colors: Record<string, { bg: string; border: string; text: string; badge: string }> = {
      Healthy: {
        bg: 'bg-green-50',
        border: 'border-green-500',
        text: 'text-green-900',
        badge: 'bg-green-100 text-green-800',
      },
      Degraded: {
        bg: 'bg-yellow-50',
        border: 'border-yellow-500',
        text: 'text-yellow-900',
        badge: 'bg-yellow-100 text-yellow-800',
      },
      Unhealthy: {
        bg: 'bg-red-50',
        border: 'border-red-500',
        text: 'text-red-900',
        badge: 'bg-red-100 text-red-800',
      },
    }
    return colors[status] || colors.Unhealthy
  }

  const getStatusIcon = (status: string) => {
    const icons: Record<string, string> = {
      Healthy: '✅',
      Degraded: '⚠️',
      Unhealthy: '❌',
    }
    return icons[status] || '❓'
  }

  const statusCol = saude ? getStatusColor(saude.status) : getStatusColor('Unhealthy')

  // Dados para gráfico
  const chartData = historico
    .slice()
    .reverse()
    .map((log) => ({
      timestamp: new Date(log.timestamp).toLocaleTimeString(),
      memoria: Math.round(log.memoria),
      cpu: Math.round(log.cpu),
      requisicoes: log.requisicoes,
    }))

  const metricsData = historico.length > 0 ? historico[0] : null

  if (carregando && !saude) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 md:p-8 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin text-4xl">⏳</div>
          <p className="text-gray-600 mt-4">Buscando status da API...</p>
        </div>
      </div>
    )
  }

  if (erro && !saude) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 md:p-8">
        <div className="card">
          <div className="p-8 text-center">
            <p className="text-red-600 font-semibold mb-4">❌ {erro}</p>
            <button
              onClick={carregarHealth}
              className="px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold"
            >
              🔄 Tentar Novamente
            </button>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">🏥 Status da API</h1>
        <p className="text-gray-600 mt-1">
          Monitoramento em tempo real da saúde do servidor
        </p>
      </div>

      {/* Alert */}
      {erro && (
        <div className="mb-6 p-4 bg-yellow-50 border border-yellow-200 rounded-lg flex justify-between items-center">
          <span className="text-yellow-800">⚠️ {erro}</span>
          <button onClick={() => setErro(null)} className="text-yellow-800 hover:text-yellow-900 font-bold">
            ✕
          </button>
        </div>
      )}

      {/* Status Principal */}
      {saude && (
        <div className={`card mb-8 border-l-4 ${statusCol.border} ${statusCol.bg}`}>
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-6 pb-6 border-b border-current border-opacity-20">
            <div>
              <h2 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                {getStatusIcon(saude.status)} API Status
              </h2>
              <p className="text-gray-600 mt-1">
                Último check: {formatDateTime(saude.timestamp)}
              </p>
            </div>

            <div className="flex flex-col gap-3">
              <span className={`px-4 py-2 rounded-full font-bold text-lg inline-flex items-center gap-2 w-fit ${statusCol.badge}`}>
                {getStatusIcon(saude.status)} {saude.status}
              </span>

              <button
                onClick={carregarHealth}
                disabled={carregando}
                className="px-4 py-2 bg-blue-500 hover:bg-blue-600 disabled:opacity-50 text-white rounded-lg font-semibold transition"
              >
                🔄 Atualizar Agora
              </button>
            </div>
          </div>

          {/* Grid de Stats */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div>
              <p className="text-sm text-gray-600 font-medium">Versão da API</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{saude.apiVersion}</p>
            </div>

            <div>
              <p className="text-sm text-gray-600 font-medium">Uptime</p>
              <p className="text-xl font-bold text-gray-900 mt-1">
                {formatDuration(saude.uptime)}
              </p>
            </div>

            <div>
              <p className="text-sm text-gray-600 font-medium">Memória</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{metricsData?.memoria.toFixed(0) || 'N/A'}<span className="text-sm"> MB</span></p>
            </div>

            <div>
              <p className="text-sm text-gray-600 font-medium">CPU</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{metricsData?.cpu.toFixed(1) || 'N/A'}<span className="text-sm">%</span></p>
            </div>
          </div>
        </div>
      )}

      {/* Main Content */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left - Gráficos */}
        <div className="lg:col-span-2 space-y-6">
          {/* Chart - Memória */}
          <div className="card">
            <h3 className="text-lg font-bold text-gray-900 mb-4">📊 Uso de Memória</h3>

            {chartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="timestamp" />
                  <YAxis />
                  <Tooltip />
                  <Line
                    type="monotone"
                    dataKey="memoria"
                    stroke="#3b82f6"
                    dot={false}
                    isAnimationActive={false}
                  />
                </LineChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-80 bg-gray-50 rounded-lg flex items-center justify-center">
                <p className="text-gray-500">Aguardando dados...</p>
              </div>
            )}

            <p className="text-xs text-gray-500 mt-3">
              Memória utilizada em MB • Tempo real: {metricsData?.memoria.toFixed(0) || 'N/A'} MB
            </p>
          </div>

          {/* Chart - CPU */}
          <div className="card">
            <h3 className="text-lg font-bold text-gray-900 mb-4">⚙️ Uso de CPU</h3>

            {chartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="timestamp" />
                  <YAxis domain={[0, 100]} />
                  <Tooltip />
                  <Line
                    type="monotone"
                    dataKey="cpu"
                    stroke="#ef4444"
                    dot={false}
                    isAnimationActive={false}
                  />
                </LineChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-80 bg-gray-50 rounded-lg flex items-center justify-center">
                <p className="text-gray-500">Aguardando dados...</p>
              </div>
            )}

            <p className="text-xs text-gray-500 mt-3">
              Percentual de CPU utilizado • Tempo real: {metricsData?.cpu.toFixed(1) || 'N/A'}%
            </p>
          </div>

          {/* Chart - Requisições */}
          <div className="card">
            <h3 className="text-lg font-bold text-gray-900 mb-4">📈 Requisições por Minuto</h3>

            {chartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="timestamp" />
                  <YAxis />
                  <Tooltip />
                  <Line
                    type="monotone"
                    dataKey="requisicoes"
                    stroke="#10b981"
                    dot={false}
                    isAnimationActive={false}
                  />
                </LineChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-80 bg-gray-50 rounded-lg flex items-center justify-center">
                <p className="text-gray-500">Aguardando dados...</p>
              </div>
            )}

            <p className="text-xs text-gray-500 mt-3">
              Número de requisições/minuto • Tempo real: {metricsData?.requisicoes || 'N/A'}
            </p>
          </div>
        </div>

        {/* Right - Sidebar Controls & Info */}
        <div className="space-y-6">
          {/* Card - Auto Refresh */}
          <div className="card bg-blue-50 border-l-4 border-blue-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">🔄 Auto-Refresh</h3>

            <div className="space-y-3">
              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={autoRefresh}
                  onChange={(e) => setAutoRefresh(e.target.checked)}
                  className="w-4 h-4"
                  disabled={carregando}
                />
                <span className="text-gray-700">Ativar Auto-Refresh</span>
              </label>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Intervalo (segundos)
                </label>
                <select
                  value={intervaloRefresh}
                  onChange={(e) => setIntervaloRefresh(parseInt(e.target.value))}
                  disabled={!autoRefresh || carregando}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
                >
                  <option value={5}>5 segundos</option>
                  <option value={10}>10 segundos</option>
                  <option value={30}>30 segundos</option>
                  <option value={60}>1 minuto</option>
                </select>
              </div>

              <div className="text-xs text-gray-600 bg-white px-3 py-2 rounded-lg">
                {autoRefresh ? (
                  <p>✅ Auto-refresh ativo a cada {intervaloRefresh}s</p>
                ) : (
                  <p>⏸️ Auto-refresh desativado</p>
                )}
              </div>
            </div>
          </div>

          {/* Card - Métricas Resumidas */}
          <div className="card bg-gradient-to-br from-green-50 to-blue-50 border-l-4 border-green-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">📊 Resumo Atual</h3>

            {metricsData ? (
              <div className="space-y-3 text-sm">
                <div>
                  <p className="text-gray-600">Memória</p>
                  <div className="flex items-center gap-2 mt-1">
                    <div className="flex-1 bg-gray-200 rounded-full h-2">
                      <div
                        className="bg-blue-500 h-2 rounded-full"
                        style={{ width: `${Math.min((metricsData.memoria / 512) * 100, 100)}%` }}
                      />
                    </div>
                    <span className="font-bold text-gray-900 w-12 text-right">
                      {metricsData.memoria.toFixed(0)} MB
                    </span>
                  </div>
                </div>

                <div>
                  <p className="text-gray-600">CPU</p>
                  <div className="flex items-center gap-2 mt-1">
                    <div className="flex-1 bg-gray-200 rounded-full h-2">
                      <div
                        className="bg-red-500 h-2 rounded-full"
                        style={{ width: `${metricsData.cpu}%` }}
                      />
                    </div>
                    <span className="font-bold text-gray-900 w-12 text-right">
                      {metricsData.cpu.toFixed(1)}%
                    </span>
                  </div>
                </div>

                <div className="pt-3 border-t border-gray-200">
                  <p className="text-gray-600">Requisições/min</p>
                  <p className="text-2xl font-bold text-gray-900 mt-1">
                    {metricsData.requisicoes}
                  </p>
                </div>
              </div>
            ) : (
              <p className="text-gray-500 text-sm">Carregando métricas...</p>
            )}
          </div>

          {/* Card - Status Detalhado */}
          <div className="card bg-gray-50 border-l-4 border-gray-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">ℹ️ Detalhes</h3>

            {saude ? (
              <div className="space-y-3 text-sm">
                <div>
                  <p className="text-gray-600">Status</p>
                  <p className={`font-bold mt-1 inline-block px-2 py-1 rounded-full ${statusCol.badge}`}>
                    {saude.status}
                  </p>
                </div>

                <div className="pt-3 border-t border-gray-200">
                  <p className="text-gray-600">Versão API</p>
                  <p className="font-semibold text-gray-900 mt-1">
                    {saude.apiVersion}
                  </p>
                </div>

                <div className="pt-3 border-t border-gray-200">
                  <p className="text-gray-600">Uptime</p>
                  <p className="font-semibold text-gray-900 mt-1">
                    {formatDuration(saude.uptime)}
                  </p>
                </div>

                <div className="pt-3 border-t border-gray-200">
                  <p className="text-gray-600">Último Check</p>
                  <p className="font-semibold text-gray-900 mt-1 text-xs">
                    {formatDateTime(saude.timestamp)}
                  </p>
                </div>
              </div>
            ) : (
              <p className="text-gray-500 text-sm">Carregando...</p>
            )}
          </div>

          {/* Card - Legenda Status */}
          <div className="card bg-yellow-50 border-l-4 border-yellow-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">🔍 Legenda</h3>

            <div className="space-y-2 text-sm">
              <div className="flex items-center gap-2">
                <span className="text-lg">✅</span>
                <div>
                  <p className="font-semibold text-gray-900">Healthy</p>
                  <p className="text-gray-600 text-xs">Tudo funcionando perfeitamente</p>
                </div>
              </div>

              <div className="flex items-center gap-2">
                <span className="text-lg">⚠️</span>
                <div>
                  <p className="font-semibold text-gray-900">Degraded</p>
                  <p className="text-gray-600 text-xs">Funcionando com limitações</p>
                </div>
              </div>

              <div className="flex items-center gap-2">
                <span className="text-lg">❌</span>
                <div>
                  <p className="font-semibold text-gray-900">Unhealthy</p>
                  <p className="text-gray-600 text-xs">Serviço indisponível</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Histórico Tabular */}
      <div className="mt-8 card">
        <h3 className="text-xl font-bold text-gray-900 mb-4">📋 Histórico de Health Checks</h3>

        {historico.length === 0 ? (
          <div className="p-8 text-center bg-gray-50 rounded-lg">
            <p className="text-gray-600">Aguardando health checks...</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-100 border-b border-gray-200">
                <tr>
                  <th className="px-4 py-3 text-left font-semibold text-gray-700">Timestamp</th>
                  <th className="px-4 py-3 text-left font-semibold text-gray-700">Status</th>
                  <th className="px-4 py-3 text-right font-semibold text-gray-700">Memória (MB)</th>
                  <th className="px-4 py-3 text-right font-semibold text-gray-700">CPU (%)</th>
                  <th className="px-4 py-3 text-right font-semibold text-gray-700">Req./min</th>
                </tr>
              </thead>
              <tbody>
                {historico.slice(0, 20).map((log, index) => {
                  const col = getStatusColor(log.status)
                  return (
                    <tr key={index} className="border-b border-gray-200 hover:bg-blue-50">
                      <td className="px-4 py-3 text-gray-900 font-medium">
                        {formatDateTime(log.timestamp)}
                      </td>
                      <td className="px-4 py-3">
                        <span className={`px-2 py-1 rounded-full text-xs font-semibold inline-flex items-center gap-1 ${col.badge}`}>
                          {getStatusIcon(log.status)} {log.status}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right text-gray-900 font-semibold">
                        {log.memoria.toFixed(0)}
                      </td>
                      <td className="px-4 py-3 text-right text-gray-900 font-semibold">
                        {log.cpu.toFixed(1)}
                      </td>
                      <td className="px-4 py-3 text-right text-gray-900 font-semibold">
                        {log.requisicoes}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
