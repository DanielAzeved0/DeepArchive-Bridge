'use client'

import React, { useState, useEffect } from 'react'
import { archivingService } from '@/lib/api'
import { ArquivamentoInfo, ArquivamentoLog } from '@/types'
import { formatCurrency, formatDate, formatDateTime } from '@/lib/formatters'

export default function ArquivamentoPage() {
  const [info, setInfo] = useState<ArquivamentoInfo | null>(null)
  const [logs, setLogs] = useState<ArquivamentoLog[]>([])
  const [ultimoLog, setUltimoLog] = useState<ArquivamentoLog | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [executando, setExecutando] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const [sucesso, setSucesso] = useState(false)
  const [mostrarConfirmacao, setMostrarConfirmacao] = useState(false)

  const carregarDados = async () => {
    try {
      setCarregando(true)
      setErro(null)

      const [infoResponse, logsResponse, ultimoResponse] = await Promise.all([
        archivingService.obterInfo(),
        archivingService.listarLogs(0, 20),
        archivingService.obterUltimo(),
      ])

      if (infoResponse.sucesso && infoResponse.dados) {
        setInfo(infoResponse.dados)
      } else {
        setErro(infoResponse.mensagem || 'Erro ao carregar informacoes de arquivamento')
      }

      if (logsResponse.sucesso && logsResponse.dados) {
        setLogs(logsResponse.dados)
      }

      if (ultimoResponse.sucesso) {
        setUltimoLog(ultimoResponse.dados ?? null)
      }
    } catch {
      setErro('Falha ao conectar com o servidor')
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    carregarDados()
    const intervalo = setInterval(carregarDados, 60000)
    return () => clearInterval(intervalo)
  }, [])

  const executarArquivamento = async () => {
    try {
      setExecutando(true)
      setErro(null)
      setSucesso(false)

      const response = await archivingService.executar()

      if (response.sucesso) {
        setSucesso(true)
        setMostrarConfirmacao(false)
        await carregarDados()
        setTimeout(() => setSucesso(false), 2500)
      } else {
        setErro(response.mensagem || 'Erro ao executar arquivamento')
      }
    } catch {
      setErro('Falha ao conectar com o servidor')
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

  if (carregando && !info) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 md:p-8 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin text-4xl">...</div>
          <p className="text-gray-600 mt-4">Carregando informacoes de arquivamento...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Gerenciador de Arquivamento</h1>
        <p className="text-gray-600 mt-1">
          Valide vendas antigas e acompanhe o historico real gravado no SQLite
        </p>
      </div>

      {sucesso && (
        <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg">
          <p className="text-green-800">Arquivamento executado com sucesso.</p>
        </div>
      )}

      {erro && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex justify-between items-center">
          <span className="text-red-800">{erro}</span>
          <button onClick={() => setErro(null)} className="text-red-800 hover:text-red-900 font-bold">
            x
          </button>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
        <div className="card bg-yellow-50 border-l-4 border-yellow-500">
          <p className="text-sm text-gray-600 font-medium">Vendas a validar</p>
          <p className="text-3xl font-bold text-gray-900 mt-2">{info?.vendasParaArquivar || 0}</p>
          <p className="text-xs text-gray-500 mt-2">Registros com mais de 90 dias</p>
        </div>

        <div className="card bg-blue-50 border-l-4 border-blue-500">
          <p className="text-sm text-gray-600 font-medium">Valor elegivel</p>
          <p className="text-2xl font-bold text-gray-900 mt-2">{formatCurrency(info?.valorAArquivar || 0)}</p>
          <p className="text-xs text-gray-500 mt-2">Somado a partir da API</p>
        </div>

        <div className="card bg-purple-50 border-l-4 border-purple-500">
          <p className="text-sm text-gray-600 font-medium">Data limite</p>
          <p className="text-2xl font-bold text-gray-900 mt-2">
            {info?.dataLimite ? formatDate(info.dataLimite) : 'N/A'}
          </p>
          <p className="text-xs text-gray-500 mt-2">Retencao logica atual</p>
        </div>

        <div className="card bg-green-50 border-l-4 border-green-500">
          <p className="text-sm text-gray-600 font-medium">Ultimo arquivamento</p>
          <p className="text-xl font-bold text-gray-900 mt-2">
            {ultimoLog ? formatDate(ultimoLog.dataExecucao) : 'Nenhum'}
          </p>
          <p className="text-xs text-gray-500 mt-2">
            {ultimoLog ? `${ultimoLog.vendasProcessadas} vendas processadas` : 'Sem logs registrados'}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <div className="card">
            <h2 className="text-xl font-bold text-gray-900 mb-4">Executar arquivamento manual</h2>
            <div className="p-4 border border-gray-200 rounded-lg hover:bg-blue-50 transition">
              <div className="flex flex-col md:flex-row md:justify-between md:items-start gap-4">
                <div>
                  <h3 className="font-bold text-gray-900">Validacao imediata</h3>
                  <p className="text-sm text-gray-600 mt-1">
                    O sistema valida os registros elegiveis e grava um log real da execucao.
                  </p>
                  <p className="text-xs text-gray-500 mt-2">
                    Esta fase nao move dados para outro banco nem agenda execucoes automaticas.
                  </p>
                </div>
                <button
                  onClick={() => setMostrarConfirmacao(true)}
                  disabled={executando}
                  className="px-4 py-2 bg-blue-500 hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-lg font-semibold transition"
                >
                  Executar agora
                </button>
              </div>
            </div>
          </div>

          <div className="card">
            <h2 className="text-xl font-bold text-gray-900 mb-4">Historico de arquivamentos</h2>
            {logs.length === 0 ? (
              <div className="p-8 text-center bg-gray-50 rounded-lg">
                <p className="text-gray-600">Nenhum arquivamento realizado ainda</p>
              </div>
            ) : (
              <div className="space-y-3">
                {logs.map((log) => (
                  <div key={log.id} className={`p-4 border rounded-lg ${getStatusColor(log.status)}`}>
                    <div className="flex justify-between items-start mb-2">
                      <div>
                        <p className="font-bold text-gray-900">{log.mensagem}</p>
                        <p className="text-sm text-gray-600 mt-1">{formatDateTime(log.dataExecucao)}</p>
                      </div>
                      <span className="text-xs font-semibold uppercase">{log.status}</span>
                    </div>

                    <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm mt-3">
                      <div>
                        <p className="text-gray-600">Vendas</p>
                        <p className="font-bold text-gray-900">{log.vendasProcessadas}</p>
                      </div>
                      <div>
                        <p className="text-gray-600">Itens</p>
                        <p className="font-bold text-gray-900">{log.itensProcessados}</p>
                      </div>
                      <div>
                        <p className="text-gray-600">Valor</p>
                        <p className="font-bold text-gray-900">{formatCurrency(log.valorProcessado)}</p>
                      </div>
                      <div>
                        <p className="text-gray-600">Duracao</p>
                        <p className="font-bold text-gray-900">{log.duracaoMs} ms</p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        <div className="space-y-6">
          <div className="card bg-gradient-to-br from-blue-50 to-purple-50 border-l-4 border-purple-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">Status geral</h3>
            <div className="space-y-3 text-sm">
              <div>
                <p className="text-gray-600">Total de vendas na base</p>
                <p className="text-2xl font-bold text-gray-900">{info?.totalVendas || 0}</p>
              </div>
              <div className="pt-3 border-t border-gray-200">
                <p className="text-gray-600">Status atual</p>
                <div className="flex items-center gap-2 mt-1">
                  <span className="inline-block w-3 h-3 bg-green-500 rounded-full"></span>
                  <span className="font-semibold text-gray-900">Operacional</span>
                </div>
              </div>
            </div>
          </div>

          <div className="card bg-yellow-50 border-l-4 border-yellow-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">Observacoes</h3>
            <div className="text-sm text-gray-700 space-y-2">
              <p>O arquivamento atual e logico e usa SQLite como armazenamento unificado.</p>
              <p>Execucoes automaticas ficam para uma fase futura com background worker.</p>
              <p>O historico desta tela vem da API e fica salvo no banco.</p>
            </div>
          </div>
        </div>
      </div>

      {mostrarConfirmacao && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg p-6 max-w-sm w-full">
            <h3 className="text-xl font-bold text-gray-900 mb-4">Confirmar arquivamento</h3>
            <p className="text-gray-600 mb-2">Deseja executar a validacao de arquivamento agora?</p>
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 mb-4 text-sm">
              <p className="text-blue-900">
                <span className="font-bold">{info?.vendasParaArquivar || 0}</span>{' '}
                vendas serao processadas ({formatCurrency(info?.valorAArquivar || 0)})
              </p>
            </div>
            <div className="flex gap-3">
              <button
                onClick={() => setMostrarConfirmacao(false)}
                disabled={executando}
                className="flex-1 px-4 py-2 bg-gray-300 hover:bg-gray-400 text-gray-800 rounded-lg font-semibold disabled:opacity-50"
              >
                Cancelar
              </button>
              <button
                onClick={executarArquivamento}
                disabled={executando}
                className="flex-1 px-4 py-2 bg-green-500 hover:bg-green-600 text-white rounded-lg font-semibold disabled:opacity-50"
              >
                {executando ? 'Processando...' : 'Confirmar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
