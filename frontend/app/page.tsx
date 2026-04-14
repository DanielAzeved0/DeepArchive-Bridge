'use client'

import { useEffect, useState } from 'react'
import { archivingService, vendaService, healthService } from '@/lib/api'
import { ArquivamentoInfo, HealthStatus, Venda } from '@/types'

// Componente de Card de Estatística
function StatCard({ title, value, icon, color }: { title: string; value: string | number; icon: string; color: string }) {
  return (
    <div className="bg-gray-700 rounded-lg p-6 border-l-4 border-gray-600 hover:border-opacity-100 transition" style={{ borderLeftColor: color }}>
      <div className="flex justify-between items-start">
        <div className="flex-1">
          <p className="text-gray-400 text-sm font-medium uppercase tracking-wide">{title}</p>
          <p className="text-3xl font-bold mt-2 text-white">{value}</p>
        </div>
        <span className="text-5xl opacity-20">{icon}</span>
      </div>
    </div>
  )
}

// Componente de Alerta
function Alert({ tipo, mensagem }: { tipo: 'sucesso' | 'aviso' | 'erro' | 'info'; mensagem: string }) {
  const cores = {
    sucesso: 'bg-green-900/30 border-green-700 text-green-300',
    aviso: 'bg-yellow-900/30 border-yellow-700 text-yellow-300',
    erro: 'bg-red-900/30 border-red-700 text-red-300',
    info: 'bg-blue-900/30 border-blue-700 text-blue-300',
  }
  
  return (
    <div className={`${cores[tipo] || cores.info} border rounded-lg p-4 mb-4`}>
      {mensagem}
    </div>
  )
}

export default function Dashboard() {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [archivingInfo, setArchivingInfo] = useState<ArquivamentoInfo | null>(null)
  const [healthStatus, setHealthStatus] = useState<HealthStatus | null>(null)
  const [ultimasVendas, setUltimasVendas] = useState<Venda[]>([])

  useEffect(() => {
    carregarDados()
    // Refresh a cada 30 segundos
    const intervalo = setInterval(carregarDados, 30000)
    return () => clearInterval(intervalo)
  }, [])

  const carregarDados = async () => {
    try {
      setLoading(true)
      setError(null)

      // Carregar dados em paralelo
      const [archResp, healthResp, vendas] = await Promise.all([
        archivingService.obterInfo().catch(() => null),
        healthService.status().catch(() => null),
        vendaService
          .buscar({
            dataInicio: new Date(Date.now() - 90 * 24 * 60 * 60 * 1000)
              .toISOString()
              .split('T')[0],
            dataFim: new Date().toISOString().split('T')[0],
            skip: 0,
            take: 5,
          })
          .catch(() => []),
      ])

      setArchivingInfo(archResp?.dados || null)
      setHealthStatus(healthResp?.dados || null)
      setUltimasVendas(vendas)
    } catch (err: any) {
      setError(err.message || 'Erro ao carregar dados')
    } finally {
      setLoading(false)
    }
  }

  const formatarMoeda = (valor: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(valor)
  }

  const formatarData = (data: string) => {
    return new Date(data).toLocaleDateString('pt-BR')
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin text-blue-400 text-4xl mb-4">⏳</div>
          <p className="text-gray-400">Carregando dados...</p>
        </div>
      </div>
    )
  }

  return (
    <div>
      <div className="mb-10">
        <h1 className="text-5xl font-bold text-white mb-2">📊 Dashboard</h1>
        <p className="text-gray-400 text-lg">
          Bem-vindo ao DeepArchive-Bridge
        </p>
      </div>

      {/* Alertas */}
      {error && <Alert tipo="erro" mensagem={`Erro: ${error}`} />}
      {healthStatus?.status !== 'Healthy' && (
        <Alert tipo="aviso" mensagem="⚠️ API não está totalmente operacional" />
      )}

      {/* Cards de Estatísticas */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-10">
        <StatCard
          title="Total de Vendas"
          value={archivingInfo?.vendaRelativosArmazem ?? 0}
          icon="📦"
          color="#3b82f6"
        />
        <StatCard
          title="Valor Total"
          value={formatarMoeda(0)}
          icon="💰"
          color="#10b981"
        />
        <StatCard
          title="Vendas a Arquivar"
          value={archivingInfo?.vendaRelativosArmazem ?? 0}
          icon="🗂️"
          color="#f59e0b"
        />
        <StatCard
          title="Valor a Arquivar"
          value={formatarMoeda(0)}
          icon="❄️"
          color="#ef4444"
        />
      </div>

      {/* Seção de Ações */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-10">
        {/* Info Arquivamento */}
        <div className="bg-gray-700 rounded-lg p-6 border border-gray-600 hover:border-gray-500 transition">
          <h2 className="text-2xl font-bold text-white mb-6 flex items-center gap-2">
            🗂️ Arquivamento
          </h2>
          {archivingInfo ? (
            <div className="space-y-4 text-sm">
              <div className="flex justify-between items-center bg-gray-600/50 p-3 rounded">
                <span className="text-gray-400"><strong>Docs Cold:</strong></span>
                <span className="text-white font-semibold">{archivingInfo.vendaRelativosArquivo}</span>
              </div>
              <div className="flex justify-between items-center bg-gray-600/50 p-3 rounded">
                <span className="text-gray-400"><strong>Docs Hot:</strong></span>
                <span className="text-white font-semibold">{archivingInfo.vendaRelativosArmazem}</span>
              </div>
              <button
                onClick={() => window.location.href = '/arquivamento'}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-lg transition mt-4"
              >
                ▶️ Gerenciar Arquivamento
              </button>
            </div>
          ) : (
            <p className="text-gray-400">Carregando informações...</p>
          )}
        </div>

        {/* Health Status */}
        <div className="bg-gray-700 rounded-lg p-6 border border-gray-600 hover:border-gray-500 transition">
          <h2 className="text-2xl font-bold text-white mb-6 flex items-center gap-2">
            🏥 Status da API
          </h2>
          {healthStatus ? (
            <div className="space-y-4 text-sm">
              <div className="flex justify-between items-center bg-gray-600/50 p-3 rounded">
                <span className="text-gray-400"><strong>Status:</strong></span>
                <span className={`font-semibold ${healthStatus.status === 'Healthy' ? 'text-green-400' : 'text-yellow-400'}`}>
                  {healthStatus.status}
                </span>
              </div>
              <div className="flex justify-between items-center bg-gray-600/50 p-3 rounded">
                <span className="text-gray-400"><strong>Versão:</strong></span>
                <span className="text-white">{healthStatus.apiVersion}</span>
              </div>
              <div className="flex justify-between items-center bg-gray-600/50 p-3 rounded">
                <span className="text-gray-400"><strong>Memória:</strong></span>
                <span className="text-white">{healthStatus.memoryMB} MB</span>
              </div>
              <div className="flex justify-between items-center bg-gray-600/50 p-3 rounded">
                <span className="text-gray-400"><strong>Uptime:</strong></span>
                <span className="text-white">{Math.floor(healthStatus.uptime / 3600)}h</span>
              </div>
              <button
                onClick={() => window.location.href = '/health'}
                className="w-full bg-green-600 hover:bg-green-700 text-white font-semibold py-2 px-4 rounded-lg transition mt-4"
              >
                🔍 Ver Detalhes
              </button>
            </div>
          ) : (
            <p className="text-gray-400">Carregando status...</p>
          )}
        </div>
      </div>

      {/* Últimas Vendas */}
      <div className="bg-gray-700 rounded-lg p-6 border border-gray-600">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold text-white">📋 Últimas Vendas</h2>
          <button
            onClick={() => window.location.href = '/vendas'}
            className="bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-lg transition text-sm"
          >
            Ver Todas →
          </button>
        </div>

        {ultimasVendas.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-gray-300">
              <thead className="border-b border-gray-600">
                <tr>
                  <th className="text-left py-4 px-4 text-white font-semibold">ID</th>
                  <th className="text-left py-4 px-4 text-white font-semibold">Cliente</th>
                  <th className="text-left py-4 px-4 text-white font-semibold">Valor</th>
                  <th className="text-left py-4 px-4 text-white font-semibold">Data</th>
                  <th className="text-left py-4 px-4 text-white font-semibold">Status</th>
                </tr>
              </thead>
              <tbody>
                {ultimasVendas.map((venda) => (
                  <tr key={venda.id} className="border-b border-gray-600 hover:bg-gray-600/50 transition">
                    <td className="py-4 px-4 text-blue-400">#{venda.id}</td>
                    <td className="py-4 px-4">{venda.clienteNome}</td>
                    <td className="py-4 px-4 text-green-400 font-semibold">{formatarMoeda(venda.valor)}</td>
                    <td className="py-4 px-4 text-gray-400">{formatarData(venda.dataCriacao)}</td>
                    <td className="py-4 px-4">
                      <span className={`px-3 py-1 rounded-full text-xs font-semibold ${
                        venda.status === 'Processada'
                          ? 'bg-green-900/50 text-green-300'
                          : venda.status === 'Pendente'
                          ? 'bg-yellow-900/50 text-yellow-300'
                          : 'bg-gray-600/50 text-gray-300'
                      }`}>
                        {venda.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="text-center text-gray-400 py-8">Nenhuma venda encontrada</p>
        )}
      </div>
    </div>
  )
}
