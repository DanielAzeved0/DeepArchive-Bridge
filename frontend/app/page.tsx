'use client'

import { useEffect, useState } from 'react'
import { archivingService, vendaService, healthService } from '@/lib/api'
import { ArquivamentoInfo, HealthStatus, Venda } from '@/types'

// Componente de Card de Estatística
function StatCard({ title, value, icon, color }: { title: string; value: string | number; icon: string; color: string }) {
  return (
    <div className="card border-l-4" style={{ borderLeftColor: color }}>
      <div className="flex justify-between items-start">
        <div>
          <p className="text-gray-600 text-sm font-medium">{title}</p>
          <p className="text-3xl font-bold mt-2">{value}</p>
        </div>
        <span className="text-4xl">{icon}</span>
      </div>
    </div>
  )
}

// Componente de Alerta
function Alert({ tipo, mensagem }: { tipo: 'sucesso' | 'aviso' | 'erro' | 'info'; mensagem: string }) {
  const cores = {
    sucesso: 'bg-green-50 border-green-200 text-green-800',
    aviso: 'bg-yellow-50 border-yellow-200 text-yellow-800',
    erro: 'bg-red-50 border-red-200 text-red-800',
    info: 'bg-blue-50 border-blue-200 text-blue-800',
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
          <div className="animate-spin text-blue-600 text-4xl mb-4">⏳</div>
          <p className="text-gray-600">Carregando dados...</p>
        </div>
      </div>
    )
  }

  return (
    <div>
      <h1 className="text-4xl font-bold mb-2">📊 Dashboard</h1>
      <p className="text-gray-600 mb-8">
        Bem-vindo ao DeepArchive-Bridge
      </p>

      {/* Alertas */}
      {error && <Alert tipo="erro" mensagem={`Erro: ${error}`} />}
      {healthStatus?.status !== 'Healthy' && (
        <Alert tipo="aviso" mensagem="⚠️ API não está totalmente operacional" />
      )}

      {/* Cards de Estatísticas */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <StatCard
          title="Total de Vendas"
          value={archivingInfo?.totalVendas ?? 0}
          icon="📦"
          color="#3b82f6"
        />
        <StatCard
          title="Valor Total"
          value={formatarMoeda(archivingInfo?.valor_total ?? 0)}
          icon="💰"
          color="#10b981"
        />
        <StatCard
          title="Vendas a Arquivar"
          value={archivingInfo?.vendas_para_arquivar ?? 0}
          icon="🗂️"
          color="#f59e0b"
        />
        <StatCard
          title="Valor a Arquivar"
          value={formatarMoeda(archivingInfo?.valor_para_arquivar ?? 0)}
          icon="❄️"
          color="#ef4444"
        />
      </div>

      {/* Seção de Ações */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
        {/* Info Arquivamento */}
        <div className="card">
          <h2 className="text-xl font-bold mb-4">🗂️ Arquivamento</h2>
          {archivingInfo ? (
            <div className="space-y-2 text-sm">
              <p>
                <strong>Data Limite:</strong> {formatarData(archivingInfo.data_limite)}
              </p>
              <p>
                <strong>Vendas Prontas:</strong> {archivingInfo.vendas_para_arquivar}
              </p>
              <p>
                <strong>Valor:</strong> {formatarMoeda(archivingInfo.valor_para_arquivar)}
              </p>
              <button
                onClick={() => window.location.href = '/arquivamento'}
                className="btn-primary w-full mt-4"
              >
                ▶️ Gerenciar Arquivamento
              </button>
            </div>
          ) : (
            <p className="text-gray-500">Carregando informações...</p>
          )}
        </div>

        {/* Health Status */}
        <div className="card">
          <h2 className="text-xl font-bold mb-4">🏥 Status da API</h2>
          {healthStatus ? (
            <div className="space-y-2 text-sm">
              <p>
                <strong>Status:</strong>{' '}
                <span className="badge badge-success">{healthStatus.status}</span>
              </p>
              <p>
                <strong>Versão:</strong> {healthStatus.apiVersion}
              </p>
              <p>
                <strong>Memória:</strong> {healthStatus.uptime} MB
              </p>
              <p>
                <strong>Timestamp:</strong> {formatarData(healthStatus.timestamp)}
              </p>
              <button
                onClick={() => window.location.href = '/health'}
                className="btn-secondary w-full mt-4"
              >
                🔍 Ver Detalhes
              </button>
            </div>
          ) : (
            <p className="text-gray-500">Carregando status...</p>
          )}
        </div>
      </div>

      {/* Últimas Vendas */}
      <div className="card mb-8">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-xl font-bold">📋 Últimas Vendas</h2>
          <button
            onClick={() => window.location.href = '/vendas'}
            className="btn-primary text-sm"
          >
            Ver Todas →
          </button>
        </div>

        {ultimasVendas.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b">
                  <th className="text-left py-3 px-4">ID</th>
                  <th className="text-left py-3 px-4">Cliente</th>
                  <th className="text-left py-3 px-4">Valor</th>
                  <th className="text-left py-3 px-4">Data</th>
                  <th className="text-left py-3 px-4">Status</th>
                  <th className="text-left py-3 px-4">Ação</th>
                </tr>
              </thead>
              <tbody>
                {ultimasVendas.map((venda) => (
                  <tr key={venda.id} className="border-b hover:bg-gray-50">
                    <td className="py-3 px-4">#{venda.id}</td>
                    <td className="py-3 px-4">{venda.clienteNome}</td>
                    <td className="py-3 px-4">{formatarMoeda(venda.valor)}</td>
                    <td className="py-3 px-4">{formatarData(venda.dataVenda)}</td>
                    <td className="py-3 px-4">
                      <span
                        className={`badge ${
                          venda.status === 'Confirmada'
                            ? 'badge-success'
                            : venda.status === 'Pendente'
                            ? 'badge-warning'
                            : venda.status === 'Cancelada'
                            ? 'badge-danger'
                            : 'badge-info'
                        }`}
                      >
                        {venda.status}
                      </span>
                    </td>
                    <td className="py-3 px-4">
                      <a
                        href={`/vendas/${venda.id}`}
                        className="text-blue-600 hover:underline"
                      >
                        Ver →
                      </a>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="text-gray-500 text-center py-8">Nenhuma venda encontrada</p>
        )}
      </div>

      {/* Quick Actions */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-6">
        <h2 className="text-lg font-bold mb-4">⚡ Ações Rápidas</h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <a
            href="/vendas/novo"
            className="card text-center hover:shadow-lg transition-shadow cursor-pointer"
          >
            <div className="text-3xl mb-2">📝</div>
            <p className="font-semibold">Nova Venda</p>
          </a>
          <a
            href="/vendas"
            className="card text-center hover:shadow-lg transition-shadow cursor-pointer"
          >
            <div className="text-3xl mb-2">📋</div>
            <p className="font-semibold">Ver Vendas</p>
          </a>
          <a
            href="/arquivamento"
            className="card text-center hover:shadow-lg transition-shadow cursor-pointer"
          >
            <div className="text-3xl mb-2">🗂️</div>
            <p className="font-semibold">Arquivar</p>
          </a>
          <a
            href="/health"
            className="card text-center hover:shadow-lg transition-shadow cursor-pointer"
          >
            <div className="text-3xl mb-2">🏥</div>
            <p className="font-semibold">Status API</p>
          </a>
        </div>
      </div>
    </div>
  )
}
