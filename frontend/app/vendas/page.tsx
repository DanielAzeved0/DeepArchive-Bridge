'use client'

import React, { useState, useEffect } from 'react'
import Link from 'next/link'
import { vendaService } from '@/lib/api'
import { Venda, ApiResponse } from '@/types'
import { formatCurrency, formatDate } from '@/lib/formatters'

type SortField = 'id' | 'clienteNome' | 'valor' | 'dataVenda'
type SortOrder = 'asc' | 'desc'

interface FilterOptions {
  search: string
  status: string
  dataInicio: string
  dataFim: string
}

// Mapear números de status para strings legíveis
const STATUS_MAP: Record<number | string, string> = {
  1: 'Pendente',
  2: 'Confirmada',
  3: 'Entregue',
  4: 'Cancelada',
  'pendente': 'Pendente',
  'confirmada': 'Confirmada',
  'entregue': 'Entregue',
  'cancelada': 'Cancelada',
}

function getStatusString(status: number | string): string {
  return STATUS_MAP[status] || String(status)
}

export default function VendasPage() {
  const [vendas, setVendas] = useState<Venda[]>([])
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [paginaAtual, setPaginaAtual] = useState(1)
  const [totalPaginas, setTotalPaginas] = useState(1)
  const [sortField, setSortField] = useState<SortField>('dataVenda')
  const [sortOrder, setSortOrder] = useState<SortOrder>('desc')
  const itensPorPagina = 10

  const [filtros, setFiltros] = useState<FilterOptions>({
    search: '',
    status: 'todas',
    dataInicio: '',
    dataFim: '',
  })

  // Calcular data padrão (últimos 90 dias)
  useEffect(() => {
    const hoje = new Date()
    const noventa_dias_atras = new Date(hoje.getTime() - 90 * 24 * 60 * 60 * 1000)

    setFiltros((prev) => ({
      ...prev,
      dataFim: hoje.toISOString().split('T')[0],
      dataInicio: noventa_dias_atras.toISOString().split('T')[0],
    }))
  }, [])

  // Carregar vendas
  const carregarVendas = async () => {
    try {
      setCarregando(true)
      setErro(null)

      const dataInicio = filtros.dataInicio || '2023-01-01'
      const dataFim = filtros.dataFim || new Date().toISOString().split('T')[0]

      console.log('Buscando vendas com:', { dataInicio, dataFim, skip: (paginaAtual - 1) * itensPorPagina, take: itensPorPagina })
      
      const vendas = await vendaService.buscar({
        dataInicio,
        dataFim,
        skip: (paginaAtual - 1) * itensPorPagina,
        take: itensPorPagina,
      })

      console.log('Vendas retornadas:', vendas)

      if (vendas && Array.isArray(vendas)) {
        let dadosOrdenados = vendas

        // Aplicar filtro de busca
        if (filtros.search.trim()) {
          const searchLower = filtros.search.toLowerCase()
          dadosOrdenados = dadosOrdenados.filter((venda) =>
            venda.clienteNome?.toLowerCase().includes(searchLower)
          )
        }

        // Aplicar filtro de status
        if (filtros.status !== 'todas') {
          dadosOrdenados = dadosOrdenados.filter(
            (venda) => getStatusString(venda.status).toLowerCase() === filtros.status.toLowerCase()
          )
        }

        // Aplicar ordenação
        dadosOrdenados.sort((a, b) => {
          let valorA: string | number = a[sortField] || ''
          let valorB: string | number = b[sortField] || ''

          if (sortField === 'valor') {
            valorA = parseFloat(String(valorA))
            valorB = parseFloat(String(valorB))
          } else if (sortField === 'dataVenda') {
            valorA = new Date(String(valorA)).getTime()
            valorB = new Date(String(valorB)).getTime()
          }

          if (valorA < valorB) return sortOrder === 'asc' ? -1 : 1
          if (valorA > valorB) return sortOrder === 'asc' ? 1 : -1
          return 0
        })

        setVendas(dadosOrdenados)
        setTotalPaginas(Math.ceil(dadosOrdenados.length / itensPorPagina))
      } else {
        setErro('Nenhuma venda encontrada')
      }
    } catch (error) {
      const errorMsg = error instanceof Error ? error.message : 'Falha ao conectar com o servidor'
      setErro(errorMsg)
      console.error('Erro ao buscar vendas:', error)
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    if (filtros.dataInicio && filtros.dataFim) {
      carregarVendas()
    }
  }, [paginaAtual, filtros.dataInicio, filtros.dataFim, filtros.search, filtros.status, sortField, sortOrder])

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')
    } else {
      setSortField(field)
      setSortOrder('asc')
    }
  }

  const getStatusColor = (status: number | string) => {
    const statusStr = getStatusString(status).toLowerCase()
    const colors: Record<string, string> = {
      entregue: 'bg-green-100 text-green-800',
      confirmada: 'bg-green-100 text-green-800',
      pendente: 'bg-yellow-100 text-yellow-800',
      cancelada: 'bg-red-100 text-red-800',
      arquivada: 'bg-blue-100 text-blue-800',
      processando: 'bg-purple-100 text-purple-800',
    }
    return colors[statusStr] || 'bg-gray-100 text-gray-800'
  }

  const getSortIcon = (field: SortField) => {
    if (sortField !== field) return '↕️'
    return sortOrder === 'asc' ? '↑' : '↓'
  }

  const vendasPaginadas = vendas.slice(
    (paginaAtual - 1) * itensPorPagina,
    paginaAtual * itensPorPagina
  )

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-8">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between mb-8 gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">📋 Vendas</h1>
          <p className="text-gray-600 mt-1">Gerenciar todas as vendas do sistema</p>
        </div>
        <Link
          href="/vendas/novo"
          className="btn-primary inline-flex items-center justify-center"
        >
          📝 Nova Venda
        </Link>
      </div>

      {/* Error Alert */}
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

      {/* Filters Section */}
      <div className="card mb-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">🔍 Filtros</h2>
        
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {/* Search */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Buscar Cliente
            </label>
            <input
              type="text"
              placeholder="Nome do cliente..."
              value={filtros.search}
              onChange={(e) => {
                setFiltros((prev) => ({ ...prev, search: e.target.value }))
                setPaginaAtual(1)
              }}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Status Filter */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Status
            </label>
            <select
              value={filtros.status}
              onChange={(e) => {
                setFiltros((prev) => ({ ...prev, status: e.target.value }))
                setPaginaAtual(1)
              }}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="todas">Todas</option>
              <option value="finalizada">✅ Finalizada</option>
              <option value="pendente">⏳ Pendente</option>
              <option value="cancelada">❌ Cancelada</option>
              <option value="arquivada">🗂️ Arquivada</option>
              <option value="processando">⚙️ Processando</option>
            </select>
          </div>

          {/* Data Início */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Data Início
            </label>
            <input
              type="date"
              value={filtros.dataInicio}
              onChange={(e) => {
                setFiltros((prev) => ({ ...prev, dataInicio: e.target.value }))
                setPaginaAtual(1)
              }}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Data Fim */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Data Fim
            </label>
            <input
              type="date"
              value={filtros.dataFim}
              onChange={(e) => {
                setFiltros((prev) => ({ ...prev, dataFim: e.target.value }))
                setPaginaAtual(1)
              }}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        {/* Reset Button */}
        <button
          onClick={() => {
            setPaginaAtual(1)
            carregarVendas()
          }}
          className="mt-4 px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-md transition"
        >
          🔄 Atualizar
        </button>
      </div>

      {/* Table Section */}
      <div className="card overflow-hidden">
        {carregando ? (
          <div className="p-8 text-center">
            <div className="inline-block animate-spin">⏳</div>
            <p className="text-gray-600 mt-2">Carregando vendas...</p>
          </div>
        ) : vendasPaginadas.length === 0 ? (
          <div className="p-8 text-center">
            <p className="text-gray-600">📭 Nenhuma venda encontrada</p>
          </div>
        ) : (
          <>
            {/* Desktop Table */}
            <div className="hidden md:block overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-100 border-b border-gray-200">
                  <tr>
                    <th
                      onClick={() => handleSort('id')}
                      className="px-6 py-3 text-left text-sm font-semibold text-gray-700 cursor-pointer hover:bg-gray-200"
                    >
                      ID {getSortIcon('id')}
                    </th>
                    <th
                      onClick={() => handleSort('clienteNome')}
                      className="px-6 py-3 text-left text-sm font-semibold text-gray-700 cursor-pointer hover:bg-gray-200"
                    >
                      Cliente {getSortIcon('clienteNome')}
                    </th>
                    <th
                      onClick={() => handleSort('valor')}
                      className="px-6 py-3 text-left text-sm font-semibold text-gray-700 cursor-pointer hover:bg-gray-200"
                    >
                      Valor {getSortIcon('valor')}
                    </th>
                    <th
                      onClick={() => handleSort('dataVenda')}
                      className="px-6 py-3 text-left text-sm font-semibold text-gray-700 cursor-pointer hover:bg-gray-200"
                    >
                      Data {getSortIcon('dataVenda')}
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">
                      Ações
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {vendasPaginadas.map((venda) => (
                    <tr key={venda.id} className="border-b border-gray-200 hover:bg-blue-50">
                      <td className="px-6 py-3 text-sm text-gray-900 font-medium">
                        #{venda.id}
                      </td>
                      <td className="px-6 py-3 text-sm text-gray-700">{venda.clienteNome}</td>
                      <td className="px-6 py-3 text-sm text-gray-900 font-semibold">
                        {formatCurrency(venda.valor)}
                      </td>
                      <td className="px-6 py-3 text-sm text-gray-700">
                        {formatDate(venda.dataVenda)}
                      </td>
                      <td className="px-6 py-3 text-sm">
                        <span className={`px-2 py-1 rounded-full text-xs font-semibold ${getStatusColor(venda.status)}`}>
                          {getStatusString(venda.status)}
                        </span>
                      </td>
                      <td className="px-6 py-3 text-sm">
                        <div className="flex gap-2">
                          <Link
                            href={`/vendas/${venda.id}`}
                            className="px-2 py-1 bg-blue-500 hover:bg-blue-600 text-white rounded text-xs font-semibold"
                          >
                            👁️ Ver
                          </Link>
                          <Link
                            href={`/vendas/${venda.id}/editar`}
                            className="px-2 py-1 bg-yellow-500 hover:bg-yellow-600 text-white rounded text-xs font-semibold"
                          >
                            ✏️ Editar
                          </Link>
                          <button
                            onClick={() => {
                              if (confirm('Tem certeza que deseja deletar?')) {
                                // TODO: Implementar delete
                                alert('Delete não implementado ainda')
                              }
                            }}
                            className="px-2 py-1 bg-red-500 hover:bg-red-600 text-white rounded text-xs font-semibold"
                          >
                            🗑️ Deletar
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Mobile Cards */}
            <div className="md:hidden space-y-3 p-4">
              {vendasPaginadas.map((venda) => (
                <div key={venda.id} className="bg-white border border-gray-200 rounded-lg p-4">
                  <div className="flex justify-between items-start mb-2">
                    <span className="font-semibold text-gray-900">#{venda.id}</span>
                    <span className={`px-2 py-1 rounded-full text-xs font-semibold ${getStatusColor(venda.status)}`}>
                      {getStatusString(venda.status)}
                    </span>
                  </div>
                  <p className="text-sm text-gray-600 mb-1">
                    <strong>Cliente:</strong> {venda.clienteNome}
                  </p>
                  <p className="text-sm text-gray-600 mb-1">
                    <strong>Valor:</strong> {formatCurrency(venda.valor)}
                  </p>
                  <p className="text-sm text-gray-600 mb-3">
                    <strong>Data:</strong> {formatDate(venda.dataVenda)}
                  </p>
                  <div className="flex gap-2">
                    <Link
                      href={`/vendas/${venda.id}`}
                      className="flex-1 px-2 py-1 bg-blue-500 hover:bg-blue-600 text-white rounded text-xs font-semibold text-center"
                    >
                      Ver
                    </Link>
                    <Link
                      href={`/vendas/${venda.id}/editar`}
                      className="flex-1 px-2 py-1 bg-yellow-500 hover:bg-yellow-600 text-white rounded text-xs font-semibold text-center"
                    >
                      Editar
                    </Link>
                  </div>
                </div>
              ))}
            </div>
          </>
        )}
      </div>

      {/* Pagination */}
      {!carregando && totalPaginas > 1 && (
        <div className="mt-6 flex justify-center items-center gap-2">
          <button
            onClick={() => setPaginaAtual(Math.max(1, paginaAtual - 1))}
            disabled={paginaAtual === 1}
            className="px-4 py-2 bg-gray-200 hover:bg-gray-300 disabled:opacity-50 disabled:cursor-not-allowed text-gray-800 rounded-md"
          >
            ← Anterior
          </button>

          <div className="flex gap-1">
            {Array.from({ length: totalPaginas }, (_, i) => i + 1).map((pagina) => (
              <button
                key={pagina}
                onClick={() => setPaginaAtual(pagina)}
                className={`px-3 py-2 rounded-md text-sm font-semibold ${
                  paginaAtual === pagina
                    ? 'bg-blue-500 text-white'
                    : 'bg-gray-200 hover:bg-gray-300 text-gray-800'
                }`}
              >
                {pagina}
              </button>
            ))}
          </div>

          <button
            onClick={() => setPaginaAtual(Math.min(totalPaginas, paginaAtual + 1))}
            disabled={paginaAtual === totalPaginas}
            className="px-4 py-2 bg-gray-200 hover:bg-gray-300 disabled:opacity-50 disabled:cursor-not-allowed text-gray-800 rounded-md"
          >
            Próxima →
          </button>
        </div>
      )}

      {/* Stats Footer */}
      <div className="mt-6 text-center text-sm text-gray-600">
        Mostrando {vendasPaginadas.length} de {vendas.length} vendas
        {totalPaginas > 1 && ` | Página ${paginaAtual} de ${totalPaginas}`}
      </div>
    </div>
  )
}
