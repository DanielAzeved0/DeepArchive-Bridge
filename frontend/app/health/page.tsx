'use client'

import React, { useState, useEffect } from 'react'
import { healthService } from '@/lib/api'
import { HealthStatus } from '@/types'
import { formatDateTime, formatDuration } from '@/lib/formatters'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'

interface HealthLog {
  timestamp: string
  status: string
  memoria: number
  duracao: number
  dependenciasOk: number
  dependenciasComErro: number
}

export default function HealthPage() {
  const [saude, setSaude] = useState<HealthStatus | null>(null)
  const [historico, setHistorico] = useState<HealthLog[]>([])
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [autoRefresh, setAutoRefresh] = useState(true)
  const [intervaloRefresh, setIntervaloRefresh] = useState(5)

  const carregarHealth = async () => {
    try {
      setCarregando(true)
      setErro(null)

      const response = await healthService.status()

      if (response.sucesso && response.dados) {
        const dados = response.dados
        setSaude(dados)
        setHistorico((prev) => [
          {
            timestamp: dados.timestamp,
            status: dados.status,
            memoria: dados.memoryMB,
            duracao: dados.checkDurationMs ?? 0,
            dependenciasOk: dados.dependenciesHealthy ?? 0,
            dependenciasComErro: dados.dependenciesUnhealthy ?? 0,
          },
          ...prev,
        ].slice(0, 60))
      } else {
        setErro(response.mensagem || 'Erro ao carregar status da API')
      }
    } catch {
      setErro('Falha ao conectar com o servidor')
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    carregarHealth()

    if (!autoRefresh) return

    const intervalo = setInterval(carregarHealth, intervaloRefresh * 1000)
    return () => clearInterval(intervalo)
  }, [autoRefresh, intervaloRefresh])

  const getStatusColor = (status: string) => {
    const colors: Record<string, { bg: string; border: string; badge: string }> = {
      Healthy: { bg: 'bg-green-50', border: 'border-green-500', badge: 'bg-green-100 text-green-800' },
      Degraded: { bg: 'bg-yellow-50', border: 'border-yellow-500', badge: 'bg-yellow-100 text-yellow-800' },
      Unhealthy: { bg: 'bg-red-50', border: 'border-red-500', badge: 'bg-red-100 text-red-800' },
    }
    return colors[status] || colors.Unhealthy
  }

  const chartData = historico
    .slice()
    .reverse()
    .map((log) => ({
      timestamp: new Date(log.timestamp).toLocaleTimeString('pt-BR'),
      memoria: log.memoria,
      duracao: log.duracao,
    }))

  const statusCol = saude ? getStatusColor(saude.status) : getStatusColor('Unhealthy')

  if (carregando && !saude) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 md:p-8 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin text-4xl">...</div>
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
            <p className="text-red-600 font-semibold mb-4">{erro}</p>
            <button onClick={carregarHealth} className="btn-primary">
              Tentar novamente
            </button>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Status da API</h1>
        <p className="text-gray-600 mt-1">Monitoramento com dados reais retornados pelo backend</p>
      </div>

      {erro && (
        <div className="mb-6 p-4 bg-yellow-50 border border-yellow-200 rounded-lg flex justify-between items-center">
          <span className="text-yellow-800">{erro}</span>
          <button onClick={() => setErro(null)} className="text-yellow-800 hover:text-yellow-900 font-bold">
            x
          </button>
        </div>
      )}

      {saude && (
        <div className={`card mb-8 border-l-4 ${statusCol.border} ${statusCol.bg}`}>
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-6 pb-6 border-b border-current border-opacity-20">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">API Status</h2>
              <p className="text-gray-600 mt-1">Ultimo check: {formatDateTime(saude.timestamp)}</p>
            </div>

            <div className="flex flex-col gap-3">
              <span className={`px-4 py-2 rounded-full font-bold text-lg inline-flex items-center gap-2 w-fit ${statusCol.badge}`}>
                {saude.status}
              </span>
              <button
                onClick={carregarHealth}
                disabled={carregando}
                className="px-4 py-2 bg-blue-500 hover:bg-blue-600 disabled:opacity-50 text-white rounded-lg font-semibold transition"
              >
                Atualizar agora
              </button>
            </div>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
            <div>
              <p className="text-sm text-gray-600 font-medium">Versao</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{saude.apiVersion}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Uptime</p>
              <p className="text-xl font-bold text-gray-900 mt-1">{formatDuration(saude.uptime)}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Memoria</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{saude.memoryMB}<span className="text-sm"> MB</span></p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Duracao</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{saude.checkDurationMs ?? 0}<span className="text-sm"> ms</span></p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Dependencias</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">
                {saude.dependenciesHealthy ?? 0}/{(saude.dependenciesHealthy ?? 0) + (saude.dependenciesUnhealthy ?? 0)}
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <div className="card">
            <h3 className="text-lg font-bold text-gray-900 mb-4">Uso de memoria</h3>
            {chartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="timestamp" />
                  <YAxis />
                  <Tooltip />
                  <Line type="monotone" dataKey="memoria" stroke="#3b82f6" dot={false} isAnimationActive={false} />
                </LineChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-80 bg-gray-50 rounded-lg flex items-center justify-center">
                <p className="text-gray-500">Aguardando dados...</p>
              </div>
            )}
          </div>

          <div className="card">
            <h3 className="text-lg font-bold text-gray-900 mb-4">Duracao do health check</h3>
            {chartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="timestamp" />
                  <YAxis />
                  <Tooltip />
                  <Line type="monotone" dataKey="duracao" stroke="#10b981" dot={false} isAnimationActive={false} />
                </LineChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-80 bg-gray-50 rounded-lg flex items-center justify-center">
                <p className="text-gray-500">Aguardando dados...</p>
              </div>
            )}
          </div>
        </div>

        <div className="space-y-6">
          <div className="card bg-blue-50 border-l-4 border-blue-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">Auto-refresh</h3>
            <div className="space-y-3">
              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={autoRefresh}
                  onChange={(e) => setAutoRefresh(e.target.checked)}
                  className="w-4 h-4"
                  disabled={carregando}
                />
                <span className="text-gray-700">Ativar auto-refresh</span>
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
          </div>

          <div className="card bg-gray-50 border-l-4 border-gray-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">Detalhes</h3>
            {saude ? (
              <div className="space-y-3 text-sm">
                <div>
                  <p className="text-gray-600">Status</p>
                  <p className={`font-bold mt-1 inline-block px-2 py-1 rounded-full ${statusCol.badge}`}>{saude.status}</p>
                </div>
                <div className="pt-3 border-t border-gray-200">
                  <p className="text-gray-600">Dependencias com erro</p>
                  <p className="font-semibold text-gray-900 mt-1">{saude.dependenciesUnhealthy ?? 0}</p>
                </div>
                <div className="pt-3 border-t border-gray-200">
                  <p className="text-gray-600">Ultimo check</p>
                  <p className="font-semibold text-gray-900 mt-1 text-xs">{formatDateTime(saude.timestamp)}</p>
                </div>
              </div>
            ) : (
              <p className="text-gray-500 text-sm">Carregando...</p>
            )}
          </div>
        </div>
      </div>

      <div className="mt-8 card">
        <h3 className="text-xl font-bold text-gray-900 mb-4">Historico de checks</h3>
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
                  <th className="px-4 py-3 text-right font-semibold text-gray-700">Memoria</th>
                  <th className="px-4 py-3 text-right font-semibold text-gray-700">Duracao</th>
                  <th className="px-4 py-3 text-right font-semibold text-gray-700">Deps OK</th>
                  <th className="px-4 py-3 text-right font-semibold text-gray-700">Deps erro</th>
                </tr>
              </thead>
              <tbody>
                {historico.slice(0, 20).map((log, index) => {
                  const col = getStatusColor(log.status)
                  return (
                    <tr key={`${log.timestamp}-${index}`} className="border-b border-gray-200 hover:bg-blue-50">
                      <td className="px-4 py-3 text-gray-900 font-medium">{formatDateTime(log.timestamp)}</td>
                      <td className="px-4 py-3">
                        <span className={`px-2 py-1 rounded-full text-xs font-semibold inline-flex items-center gap-1 ${col.badge}`}>
                          {log.status}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right text-gray-900 font-semibold">{log.memoria} MB</td>
                      <td className="px-4 py-3 text-right text-gray-900 font-semibold">{log.duracao} ms</td>
                      <td className="px-4 py-3 text-right text-gray-900 font-semibold">{log.dependenciasOk}</td>
                      <td className="px-4 py-3 text-right text-gray-900 font-semibold">{log.dependenciasComErro}</td>
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
