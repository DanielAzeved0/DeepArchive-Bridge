'use client'

import React, { useState, useEffect } from 'react'
import { archivingService } from '@/lib/api'
import { ArquivamentoInfo, ScheduleConfig } from '@/types'
import { formatCurrency, formatDate, formatDateTime } from '@/lib/formatters'

interface ArquivamentoLog {
  id: number
  timestamp: string
  vendasProcessadas: number
  valorProcessado: number
  status: 'sucesso' | 'erro' | 'processando'
  mensagem: string
}

export default function ArquivamentoPage() {
  const [info, setInfo] = useState<ArquivamentoInfo | null>(null)
  const [logs, setLogs] = useState<ArquivamentoLog[]>([])
  const [carregando, setCarregando] = useState(true)
  const [executando, setExecutando] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const [sucesso, setSucesso] = useState(false)
  const [mostrarConfirmacao, setMostrarConfirmacao] = useState(false)
  const [mostrarConfiguracaoAgendamento, setMostrarConfiguracaoAgendamento] = useState(false)
  const [tipoExecucao, setTipoExecucao] = useState<'manual' | 'automatico'>('manual')
  
  // Estados para agendamento
  const [scheduleConfig, setScheduleConfig] = useState<ScheduleConfig>({
    enabled: false,
    hour: 2,
    minute: 0,
    daysOfWeek: [0, 1, 2, 3, 4, 5, 6] // Todos os dias
  })
  const [proximaExecucao, setProximaExecucao] = useState<string>('Amanhã às 02:00 AM')

  // Carregar informações de arquivamento
  const carregarInfo = async () => {
    try {
      setCarregando(true)
      setErro(null)

      const response = await archivingService.obterInfo()

      if (response.sucesso && response.dados) {
        setInfo(response.dados)
        
        // Simular logs (em produção viria do backend)
        const logsSimulados: ArquivamentoLog[] = [
          {
            id: 1,
            timestamp: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
            vendasProcessadas: 342,
            valorProcessado: 145000,
            status: 'sucesso',
            mensagem: 'Arquivamento concluído com sucesso',
          },
          {
            id: 2,
            timestamp: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
            vendasProcessadas: 298,
            valorProcessado: 120500,
            status: 'sucesso',
            mensagem: 'Arquivamento concluído com sucesso',
          },
          {
            id: 3,
            timestamp: new Date(Date.now() - 8 * 24 * 60 * 60 * 1000).toISOString(),
            vendasProcessadas: 215,
            valorProcessado: 89300,
            status: 'sucesso',
            mensagem: 'Arquivamento concluído com sucesso',
          },
        ]
        setLogs(logsSimulados)
      } else {
        setErro(response.mensagem || 'Erro ao carregar informações')
      }
    } catch (error) {
      setErro('Falha ao conectar com o servidor')
      console.error(error)
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    carregarInfo()
    
    // Recarregar a cada 1 minuto
    const intervalo = setInterval(carregarInfo, 60000)
    
    // Carregar configuração de agendamento do localStorage
    const savedConfig = window.localStorage.getItem('archiving_schedule_config')
    if (savedConfig) {
      try {
        const config = JSON.parse(savedConfig)
        setScheduleConfig(config)
        calcularProximaExecucao(config)
      } catch (error) {
        console.error('Erro ao carregar configuração de agendamento:', error)
      }
    }
    
    return () => clearInterval(intervalo)
  }, [])

  // Função para calcular próxima execução
  const calcularProximaExecucao = (config: ScheduleConfig) => {
    const now = new Date()
    const nextExecution = new Date()
    nextExecution.setHours(config.hour, config.minute, 0, 0)
    
    // Se o horário já passou hoje, define para amanhã
    if (nextExecution <= now) {
      nextExecution.setDate(nextExecution.getDate() + 1)
    }
    
    // Formatar data
    const diasSemana = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado']
    const horaFormatada = `${String(config.hour).padStart(2, '0')}:${String(config.minute).padStart(2, '0')}`
    const proximaDia = diasSemana[nextExecution.getDay()]
    setProximaExecucao(`${proximaDia} às ${horaFormatada}`)
  }

  // Executar arquivamento
  const executarArquivamento = async (tipo: 'manual' | 'automatico') => {
    try {
      setExecutando(true)
      setErro(null)
      setSucesso(false)

      const response = tipo === 'manual' 
        ? await archivingService.executar()
        : await archivingService.executarAutomatico()

      if (response.sucesso) {
        setSucesso(true)
        setMostrarConfirmacao(false)
        
        // Criar novo log
        const novoLog: ArquivamentoLog = {
          id: (logs[0]?.id || 0) + 1,
          timestamp: new Date().toISOString(),
          vendasProcessadas: (response.dados as any)?.vendasProcessadas || 0,
          valorProcessado: (response.dados as any)?.valorProcessado || 0,
          status: 'sucesso',
          mensagem: (response.dados as any)?.mensagem || 'Arquivamento concluído com sucesso',
        }
        
        setLogs([novoLog, ...logs])
        
        // Recarregar info
        setTimeout(() => {
          carregarInfo()
          setSucesso(false)
        }, 2000)
      } else {
        setErro(response.mensagem || 'Erro ao executar arquivamento')
      }
    } catch (error) {
      setErro('Falha ao conectar com o servidor')
      console.error(error)
    } finally {
      setExecutando(false)
    }
  }

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      sucesso: 'bg-green-50 border-green-200 text-green-800',
      erro: 'bg-red-50 border-red-200 text-red-800',
      processando: 'bg-yellow-50 border-yellow-200 text-yellow-800',
    }
    return colors[status] || 'bg-gray-50 border-gray-200 text-gray-800'
  }

  const getStatusIcon = (status: string) => {
    const icons: Record<string, string> = {
      sucesso: '✅',
      erro: '❌',
      processando: '⚙️',
    }
    return icons[status] || '❓'
  }

  if (carregando) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 md:p-8 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin text-4xl">⏳</div>
          <p className="text-gray-600 mt-4">Carregando informações de arquivamento...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">🗂️ Gerenciador de Arquivamento</h1>
        <p className="text-gray-600 mt-1">
          Gerencie o arquivamento de vendas antigas para Cold Storage
        </p>
      </div>

      {/* Alert de Sucesso */}
      {sucesso && (
        <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg">
          <p className="text-green-800">✅ Arquivamento executado com sucesso!</p>
        </div>
      )}

      {/* Alert de Erro */}
      {erro && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex justify-between items-center">
          <span className="text-red-800">❌ {erro}</span>
          <button
            onClick={() => setErro(null)}
            className="text-red-800 hover:text-red-900 font-bold"
          >
            ✕
          </button>
        </div>
      )}

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
        {/* Total Vendas a Arquivar */}
        <div className="card bg-yellow-50 border-l-4 border-yellow-500">
          <p className="text-sm text-gray-600 font-medium">📦 Vendas a Arquivar</p>
          <p className="text-3xl font-bold text-gray-900 mt-2">
            {info?.vendasParaArquivar || 0}
          </p>
          <p className="text-xs text-gray-500 mt-2">
            Prontas para Cold Storage
          </p>
        </div>

        {/* Valor Total */}
        <div className="card bg-blue-50 border-l-4 border-blue-500">
          <p className="text-sm text-gray-600 font-medium">💰 Valor Total</p>
          <p className="text-2xl font-bold text-gray-900 mt-2">
            {formatCurrency(info?.valorAArquivar || 0)}
          </p>
          <p className="text-xs text-gray-500 mt-2">
            Em conversão
          </p>
        </div>

        {/* Data Limite */}
        <div className="card bg-purple-50 border-l-4 border-purple-500">
          <p className="text-sm text-gray-600 font-medium">📅 Data Limite</p>
          <p className="text-2xl font-bold text-gray-900 mt-2">
            {info?.dataLimite ? formatDate(info.dataLimite) : 'N/A'}
          </p>
          <p className="text-xs text-gray-500 mt-2">
            Retenção Hot Storage
          </p>
        </div>

        {/* Último Arquivamento - Removido: não disponível na API */}
      </div>

      {/* Main Content */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left - Ações */}
        <div className="lg:col-span-2 space-y-6">
          {/* Card - Executar Arquivamento */}
          <div className="card">
            <h2 className="text-xl font-bold text-gray-900 mb-4">⚙️ Executar Arquivamento</h2>

            <div className="space-y-4">
              {/* Execução Manual */}
              <div className="p-4 border border-gray-200 rounded-lg hover:bg-blue-50 transition">
                <div className="flex justify-between items-start">
                  <div>
                    <h3 className="font-bold text-gray-900">🔧 Execução Manual</h3>
                    <p className="text-sm text-gray-600 mt-1">
                      Execute o arquivamento imediatamente
                    </p>
                    <p className="text-xs text-gray-500 mt-2">
                      ⏱️ Tempo estimado: 2-5 minutos
                    </p>
                  </div>
                  <button
                    onClick={() => {
                      setTipoExecucao('manual')
                      setMostrarConfirmacao(true)
                    }}
                    disabled={executando || (info?.vendasParaArquivar || 0) === 0}
                    className="px-4 py-2 bg-blue-500 hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-lg font-semibold transition"
                  >
                    ▶️ Executar Agora
                  </button>
                </div>
              </div>

              {/* Execução Automática */}
              <div className="p-4 border border-gray-200 rounded-lg hover:bg-green-50 transition">
                <div className="flex justify-between items-start">
                  <div>
                    <h3 className="font-bold text-gray-900">🤖 Agendamento Automático</h3>
                    <p className="text-sm text-gray-600 mt-1">
                      Agende para rodar diariamente em horário específico
                    </p>
                    <p className="text-xs text-gray-500 mt-2">
                      ⏰ Próxima: Amanhã às 02:00 AM
                    </p>
                  </div>
                  <button
                    onClick={() => {
                      setMostrarConfiguracaoAgendamento(true)
                    }}
                    disabled={executando}
                    className="px-4 py-2 bg-green-500 hover:bg-green-600 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-lg font-semibold transition"
                  >
                    🕐 Agendar
                  </button>
                </div>
              </div>

              {/* Info */}
              {(info?.vendasParaArquivar || 0) === 0 && (
                <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
                  <p className="text-sm text-blue-800">
                    ℹ️ Nenhuma venda pronta para arquivamento no momento
                  </p>
                </div>
              )}
            </div>
          </div>

          {/* Card - Histórico */}
          <div className="card">
            <h2 className="text-xl font-bold text-gray-900 mb-4">📊 Histórico de Arquivamentos</h2>

            {logs.length === 0 ? (
              <div className="p-8 text-center bg-gray-50 rounded-lg">
                <p className="text-gray-600">📭 Nenhum arquivamento realizado ainda</p>
              </div>
            ) : (
              <div className="space-y-3">
                {logs.map((log) => (
                  <div
                    key={log.id}
                    className={`p-4 border rounded-lg ${getStatusColor(log.status)}`}
                  >
                    <div className="flex justify-between items-start mb-2">
                      <div>
                        <p className="font-bold text-gray-900 flex items-center gap-2">
                          <span>{getStatusIcon(log.status)}</span>
                          {log.mensagem}
                        </p>
                        <p className="text-sm text-gray-600 mt-1">
                          {formatDateTime(log.timestamp)}
                        </p>
                      </div>
                    </div>

                    <div className="grid grid-cols-2 gap-3 text-sm mt-3">
                      <div>
                        <p className="text-gray-600">Vendas Processadas</p>
                        <p className="font-bold text-gray-900">{log.vendasProcessadas}</p>
                      </div>
                      <div>
                        <p className="text-gray-600">Valor Processado</p>
                        <p className="font-bold text-gray-900">
                          {formatCurrency(log.valorProcessado)}
                        </p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Right - Info Sidebar */}
        <div className="space-y-6">
          {/* Card - Status Geral */}
          <div className="card bg-gradient-to-br from-blue-50 to-purple-50 border-l-4 border-purple-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">🔍 Status Geral</h3>

            <div className="space-y-3 text-sm">
              <div>
                <p className="text-gray-600">Total Arquivado</p>
                <p className="text-2xl font-bold text-gray-900">
                  {info?.totalVendas || 0}
                </p>
              </div>

              <div className="pt-3 border-t border-gray-200">
                <p className="text-gray-600">Status Atual</p>
                <div className="flex items-center gap-2 mt-1">
                  <span className="inline-block w-3 h-3 bg-green-500 rounded-full"></span>
                  <span className="font-semibold text-gray-900">Operacional</span>
                </div>
              </div>

              <div className="pt-3 border-t border-gray-200">
                <p className="text-gray-600">Próxima Execução</p>
                <p className="font-semibold text-gray-900 mt-1">
                  {proximaExecucao}
                </p>
              </div>
            </div>
          </div>

          {/* Card - Dicas */}
          <div className="card bg-yellow-50 border-l-4 border-yellow-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">💡 Dicas</h3>

            <div className="text-sm text-gray-700 space-y-2">
              <p>
                • Arquivamento manual é ideal para testes e ajustes rápidos
              </p>
              <p>
                • Automático roda todos os dias em horário de baixo uso
              </p>
              <p>
                • Dados arquivados ainda são consultáveis (Cold Storage)
              </p>
              <p>
                • Monitore o histórico regularmente
              </p>
            </div>
          </div>

          {/* Card - Stats */}
          <div className="card bg-gray-50 border-l-4 border-gray-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">📈 Estatísticas</h3>

            <div className="space-y-3 text-sm">
              {logs.length > 0 && (
                <>
                  <div>
                    <p className="text-gray-600">Última Execução</p>
                    <p className="font-semibold text-gray-900">
                      {logs[0].vendasProcessadas} vendas
                    </p>
                  </div>

                  <div className="pt-3 border-t border-gray-200">
                    <p className="text-gray-600">Média por Execução</p>
                    <p className="font-semibold text-gray-900">
                      {Math.round(logs.reduce((a, l) => a + l.vendasProcessadas, 0) / logs.length)} vendas
                    </p>
                  </div>

                  <div className="pt-3 border-t border-gray-200">
                    <p className="text-gray-600">Total Arquivado</p>
                    <p className="font-semibold text-gray-900">
                      {formatCurrency(
                        logs.reduce((a, l) => a + l.valorProcessado, 0)
                      )}
                    </p>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Modal de Confirmação */}
      {mostrarConfiguracaoAgendamento && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-xl font-bold text-gray-900 mb-4">
              🕐 Configurar Agendamento Automático
            </h3>

            <div className="space-y-4">
              {/* Seletor de Hora */}
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  Horário: {String(scheduleConfig.hour).padStart(2, '0')}:{String(scheduleConfig.minute).padStart(2, '0')}
                </label>
                <div className="flex gap-3">
                  <div className="flex-1">
                    <label className="block text-xs text-gray-600 mb-1">Hora</label>
                    <input
                      type="range"
                      min="0"
                      max="23"
                      value={scheduleConfig.hour}
                      onChange={(e) => setScheduleConfig({ ...scheduleConfig, hour: parseInt(e.target.value) })}
                      className="w-full"
                    />
                    <div className="flex justify-between text-xs text-gray-500 mt-1">
                      <span>00</span>
                      <span>23</span>
                    </div>
                  </div>
                  <div className="flex-1">
                    <label className="block text-xs text-gray-600 mb-1">Minuto</label>
                    <select
                      value={scheduleConfig.minute}
                      onChange={(e) => setScheduleConfig({ ...scheduleConfig, minute: parseInt(e.target.value) })}
                      className="w-full px-2 py-1 border border-gray-300 rounded"
                    >
                      <option value={0}>00</option>
                      <option value={15}>15</option>
                      <option value={30}>30</option>
                      <option value={45}>45</option>
                    </select>
                  </div>
                </div>
              </div>

              {/* Dias da Semana */}
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Dias da Semana</label>
                <div className="grid grid-cols-7 gap-2">
                  {['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sab'].map((day, index) => (
                    <label key={index} className="flex items-center justify-center">
                      <input
                        type="checkbox"
                        checked={scheduleConfig.daysOfWeek.includes(index)}
                        onChange={(e) => {
                          const newDays = e.target.checked
                            ? [...scheduleConfig.daysOfWeek, index]
                            : scheduleConfig.daysOfWeek.filter(d => d !== index)
                          setScheduleConfig({ ...scheduleConfig, daysOfWeek: newDays.sort() })
                        }}
                        className="mr-1"
                      />
                      <span className="text-xs text-gray-700">{day}</span>
                    </label>
                  ))}
                </div>
              </div>

              {/* Info */}
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 text-sm">
                <p className="text-blue-900">
                  ℹ️ O arquivamento automático rodará diariamente nos dias selecionados
                </p>
              </div>
            </div>

            <div className="flex gap-3 mt-6">
              <button
                onClick={() => {
                  setMostrarConfiguracaoAgendamento(false)
                }}
                disabled={executando}
                className="flex-1 px-4 py-2 bg-gray-300 hover:bg-gray-400 text-gray-800 rounded-lg font-semibold disabled:opacity-50"
              >
                ← Cancelar
              </button>
              <button
                onClick={() => {
                  // Salvar configuração no localStorage
                  window.localStorage.setItem('archiving_schedule_config', JSON.stringify(scheduleConfig))
                  calcularProximaExecucao(scheduleConfig)
                  setScheduleConfig({ ...scheduleConfig, enabled: true })
                  setMostrarConfiguracaoAgendamento(false)
                  
                  // Mostrar confirmação
                  setTipoExecucao('automatico')
                  setMostrarConfirmacao(true)
                }}
                disabled={executando || scheduleConfig.daysOfWeek.length === 0}
                className="flex-1 px-4 py-2 bg-green-500 hover:bg-green-600 text-white rounded-lg font-semibold disabled:opacity-50"
              >
                ✅ Confirmar Agendamento
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal de Confirmação */}
      {mostrarConfirmacao && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg p-6 max-w-sm w-full">
            <h3 className="text-xl font-bold text-gray-900 mb-4">
              ⚠️ Confirmar Arquivamento
            </h3>

            <p className="text-gray-600 mb-2">
              Tem certeza que deseja executar o{' '}
              <span className="font-bold">
                {tipoExecucao === 'manual'
                  ? 'arquivamento manual'
                  : 'arquivamento automático'}
              </span>
              ?
            </p>

            <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 mb-4 text-sm">
              <p className="text-blue-900">
                <span className="font-bold">
                  {info?.vendasParaArquivar}
                </span>{' '}
                vendas serão processadas ({formatCurrency(info?.valorAArquivar || 0)})
              </p>
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => {
                  setMostrarConfirmacao(false)
                }}
                disabled={executando}
                className="flex-1 px-4 py-2 bg-gray-300 hover:bg-gray-400 text-gray-800 rounded-lg font-semibold disabled:opacity-50"
              >
                ← Cancelar
              </button>
              <button
                onClick={() => {
                  executarArquivamento(tipoExecucao)
                }}
                disabled={executando}
                className="flex-1 px-4 py-2 bg-green-500 hover:bg-green-600 text-white rounded-lg font-semibold disabled:opacity-50"
              >
                {executando ? '⏳ Processando...' : '✅ Confirmar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
