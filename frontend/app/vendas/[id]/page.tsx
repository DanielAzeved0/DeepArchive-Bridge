'use client'

import React, { useState, useEffect } from 'react'
import Link from 'next/link'
import { useParams, useRouter } from 'next/navigation'
import { vendaService } from '@/lib/api'
import { Venda, VendaItem } from '@/types'
import { formatCurrency, formatDate, formatDateTime } from '@/lib/formatters'

interface VendaDetalhes extends Venda {
  items?: VendaItem[]
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

export default function VendaDetalhesPage() {
  const params = useParams()
  const router = useRouter()
  const vendaId = params.id as string

  const [venda, setVenda] = useState<VendaDetalhes | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)

  useEffect(() => {
    const carregarVenda = async () => {
      try {
        setCarregando(true)
        setErro(null)

        const response = await vendaService.obterPorId(parseInt(vendaId))

        if (response.sucesso && response.dados) {
          setVenda(response.dados)
        } else {
          setErro(response.mensagem || 'Erro ao carregar venda')
        }
      } catch (error) {
        setErro('Falha ao conectar com o servidor')
        console.error(error)
      } finally {
        setCarregando(false)
      }
    }

    if (vendaId) {
      carregarVenda()
    }
  }, [vendaId])

  const getStatusColor = (status: number | string) => {
    const statusStr = getStatusString(status).toLowerCase()
    const colors: Record<string, { bg: string; badge: string; icon: string }> = {
      entregue: { bg: 'bg-green-50', badge: 'bg-green-100 text-green-800', icon: '✅' },
      confirmada: { bg: 'bg-green-50', badge: 'bg-green-100 text-green-800', icon: '✅' },
      pendente: { bg: 'bg-yellow-50', badge: 'bg-yellow-100 text-yellow-800', icon: '⏳' },
      cancelada: { bg: 'bg-red-50', badge: 'bg-red-100 text-red-800', icon: '❌' },
      arquivada: { bg: 'bg-blue-50', badge: 'bg-blue-100 text-blue-800', icon: '🗂️' },
      processando: { bg: 'bg-purple-50', badge: 'bg-purple-100 text-purple-800', icon: '⚙️' },
    }
    return colors[statusStr] || { bg: 'bg-gray-50', badge: 'bg-gray-100 text-gray-800', icon: '❓' }
  }

  if (carregando) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 md:p-8 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin text-4xl">⏳</div>
          <p className="text-gray-600 mt-4">Carregando detalhes da venda...</p>
        </div>
      </div>
    )
  }

  if (erro || !venda) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 md:p-8">
        <div className="card">
          <div className="p-8 text-center">
            <p className="text-red-600 font-semibold mb-4">
              ❌ {erro || 'Venda não encontrada'}
            </p>
            <Link href="/vendas" className="btn-primary">
              ← Voltar para Vendas
            </Link>
          </div>
        </div>
      </div>
    )
  }

  const statusCol = getStatusColor(venda.status)
  const items = venda.itens || []
  const totalItens = items.length
  const totalItensValor = items.reduce((acc, item) => acc + (item.precoUnitario * item.quantidade), 0)

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-8">
      {/* Header com Breadcrumb */}
      <div className="mb-8">
        <div className="flex items-center gap-2 text-sm text-gray-600 mb-4">
          <Link href="/vendas" className="hover:text-blue-600">
            📋 Vendas
          </Link>
          <span>/</span>
          <span className="text-gray-900 font-semibold">#{venda.id} - {venda.clienteNome}</span>
        </div>

        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">
              Venda #{venda.id}
            </h1>
            <p className="text-gray-600 mt-1">
              Cliente: {venda.clienteNome}
            </p>
          </div>
          <div className="flex gap-2">
            <Link
              href={`/vendas/${venda.id}/editar`}
              className="btn-primary flex items-center justify-center"
            >
              ✏️ Editar
            </Link>
            <Link
              href="/vendas"
              className="btn-secondary flex items-center justify-center"
            >
              ← Voltar
            </Link>
          </div>
        </div>
      </div>

      {/* Main Content - 2 Colunas */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column - Info Principal */}
        <div className="lg:col-span-2 space-y-6">
          {/* Card - Informações Principais */}
          <div className={`card ${statusCol.bg}`}>
            <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-6 pb-6 border-b border-gray-200">
              <div>
                <h2 className="text-xl font-bold text-gray-900">📋 Informações da Venda</h2>
              </div>
              <span className={`px-4 py-2 rounded-full font-semibold inline-flex items-center gap-2 ${statusCol.badge}`}>
                {statusCol.icon} {getStatusString(venda.status).toUpperCase()}
              </span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* ID */}
              <div>
                <p className="text-sm text-gray-600 font-medium">ID da Venda</p>
                <p className="text-2xl font-bold text-gray-900">#{venda.id}</p>
              </div>

              {/* Cliente */}
              <div>
                <p className="text-sm text-gray-600 font-medium">Cliente</p>
                <p className="text-2xl font-bold text-gray-900">{venda.clienteNome}</p>
              </div>

              {/* Data */}
              <div>
                <p className="text-sm text-gray-600 font-medium">Data da Venda</p>
                <p className="text-lg font-semibold text-gray-900">
                  {formatDate(venda.dataVenda)}
                </p>
                <p className="text-xs text-gray-500 mt-1">
                  {formatDateTime(venda.dataVenda)}
                </p>
              </div>

              {/* Valor Total */}
              <div>
                <p className="text-sm text-gray-600 font-medium">Valor Total</p>
                <p className="text-2xl font-bold text-green-600">
                  {formatCurrency(venda.valor)}
                </p>
              </div>
            </div>
          </div>

          {/* Card - Itens da Venda */}
          <div className="card">
            <h2 className="text-xl font-bold text-gray-900 mb-4">📦 Itens ({totalItens})</h2>

            {items.length === 0 ? (
              <div className="p-8 text-center bg-gray-50 rounded-lg">
                <p className="text-gray-600">Nenhum item nesta venda</p>
              </div>
            ) : (
              <>
                {/* Desktop Table */}
                <div className="hidden md:block overflow-x-auto">
                  <table className="w-full">
                    <thead className="bg-gray-100 border-b border-gray-200">
                      <tr>
                        <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                          Descrição
                        </th>
                        <th className="px-4 py-3 text-right text-sm font-semibold text-gray-700">
                          Preço Unit.
                        </th>
                        <th className="px-4 py-3 text-center text-sm font-semibold text-gray-700">
                          Qtd.
                        </th>
                        <th className="px-4 py-3 text-right text-sm font-semibold text-gray-700">
                          Subtotal
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {items.map((item, index) => (
                        <tr key={index} className="border-b border-gray-200 hover:bg-blue-50">
                          <td className="px-4 py-3 text-sm text-gray-900">
                            {item.produto || `Item ${index + 1}`}
                          </td>
                          <td className="px-4 py-3 text-sm text-right text-gray-900 font-semibold">
                            {formatCurrency(item.precoUnitario)}
                          </td>
                          <td className="px-4 py-3 text-sm text-center text-gray-900">
                            {item.quantidade}x
                          </td>
                          <td className="px-4 py-3 text-sm text-right text-gray-900 font-bold">
                            {formatCurrency(item.precoUnitario * item.quantidade)}
                          </td>
                        </tr>
                      ))}
                      <tr className="bg-gray-100 font-bold">
                        <td colSpan={3} className="px-4 py-3 text-right text-gray-900">
                          Total de Itens:
                        </td>
                        <td className="px-4 py-3 text-right text-gray-900">
                          {formatCurrency(totalItensValor)}
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>

                {/* Mobile Cards */}
                <div className="md:hidden space-y-3">
                  {items.map((item, index) => (
                    <div
                      key={index}
                      className="bg-white border border-gray-200 rounded-lg p-4"
                    >
                      <p className="font-semibold text-gray-900 mb-2">
                        {item.produto || `Item ${index + 1}`}
                      </p>
                      <div className="grid grid-cols-3 gap-2 text-sm">
                        <div>
                          <p className="text-gray-600 text-xs">Preço</p>
                          <p className="font-semibold text-gray-900">
                            {formatCurrency(item.precoUnitario)}
                          </p>
                        </div>
                        <div>
                          <p className="text-gray-600 text-xs">Qtd.</p>
                          <p className="font-semibold text-gray-900">{item.quantidade}x</p>
                        </div>
                        <div>
                          <p className="text-gray-600 text-xs">Subtotal</p>
                          <p className="font-semibold text-gray-900">
                            {formatCurrency(item.precoUnitario * item.quantidade)}
                          </p>
                        </div>
                      </div>
                    </div>
                  ))}
                  
                  <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mt-4">
                    <div className="flex justify-between items-center">
                      <span className="font-semibold text-gray-900">Total de Itens:</span>
                      <span className="font-bold text-lg text-blue-600">
                        {formatCurrency(totalItensValor)}
                      </span>
                    </div>
                  </div>
                </div>
              </>
            )}
          </div>
        </div>

        {/* Right Column - Summary Cards */}
        <div className="space-y-6">
          {/* Card - Resumo Financeiro */}
          <div className="card bg-blue-50 border-l-4 border-blue-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">💰 Resumo Financeiro</h3>
            
            <div className="space-y-3">
              <div className="flex justify-between items-center pb-2 border-b border-gray-200">
                <span className="text-gray-700">Subtotal</span>
                <span className="font-semibold text-gray-900">
                  {formatCurrency(venda.valor)}
                </span>
              </div>
              
              <div className="flex justify-between items-center pb-2 border-b border-gray-200">
                <span className="text-gray-700">Desconto</span>
                <span className="font-semibold text-gray-900">-</span>
              </div>

              <div className="flex justify-between items-center bg-blue-100 px-3 py-2 rounded-lg">
                <span className="font-bold text-gray-900">Total</span>
                <span className="text-2xl font-bold text-blue-600">
                  {formatCurrency(venda.valor)}
                </span>
              </div>

              <div className="text-xs text-gray-600 text-center mt-3 pt-3 border-t border-gray-200">
                {totalItens} item{totalItens !== 1 ? 's' : ''} no total
              </div>
            </div>
          </div>

          {/* Card - Status & Ações */}
          <div className="card">
            <h3 className="text-lg font-bold text-gray-900 mb-4">⚙️ Ações</h3>

            <div className="space-y-2">
              <Link
                href={`/vendas/${venda.id}/editar`}
                className="w-full block text-center px-4 py-2 bg-yellow-500 hover:bg-yellow-600 text-white rounded-lg font-semibold transition"
              >
                ✏️ Editar Venda
              </Link>

              <button
                onClick={() => {
                  if (confirm('Tem certeza que deseja deletar esta venda?')) {
                    // TODO: Implementar delete
                    alert('Delete será implementado em breve')
                  }
                }}
                className="w-full px-4 py-2 bg-red-500 hover:bg-red-600 text-white rounded-lg font-semibold transition"
              >
                🗑️ Deletar
              </button>

              {venda.status === 'Pendente' && (
                <button
                  onClick={() => {
                    // TODO: Implementar aprovação
                    alert('Aprovação será implementada em breve')
                  }}
                  className="w-full px-4 py-2 bg-green-500 hover:bg-green-600 text-white rounded-lg font-semibold transition"
                >
                  ✅ Aprovar
                </button>
              )}

              <button
                onClick={() => {
                  // TODO: Implementar print
                  window.print()
                }}
                className="w-full px-4 py-2 bg-gray-500 hover:bg-gray-600 text-white rounded-lg font-semibold transition"
              >
                🖨️ Imprimir
              </button>
            </div>
          </div>

          {/* Card - Informações Adicionais */}
          <div className="card bg-gray-50">
            <h3 className="text-lg font-bold text-gray-900 mb-4">ℹ️ Info. Adicionais</h3>

            <div className="space-y-3 text-sm">
              <div>
                <p className="text-gray-600 font-medium">Status Atual</p>
                <p className={`font-semibold inline-block px-2 py-1 rounded-full ${statusCol.badge} mt-1`}>
                  {getStatusString(venda.status)}
                </p>
              </div>

              <div className="pt-2 border-t border-gray-200">
                <p className="text-gray-600 font-medium">Criado em</p>
                <p className="text-gray-900">
                  {formatDateTime(venda.dataCriacao)}
                </p>
              </div>

              {/* Próxima ação - não disponível na API */}
            </div>
          </div>
        </div>
      </div>

      {/* Footer Navigation */}
      <div className="mt-8 flex gap-3 justify-center">
        <Link
          href="/vendas"
          className="px-6 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-lg font-semibold transition"
        >
          ← Voltar para Listagem
        </Link>

        {/* Navegação entre vendas */}
        <div className="flex gap-2">
          <button
            onClick={() => {
              // TODO: Implementar navegação
              alert('Navegação será implementada em breve')
            }}
            className="px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-lg font-semibold"
          >
            ← Venda Anterior
          </button>
          <button
            onClick={() => {
              // TODO: Implementar navegação
              alert('Navegação será implementada em breve')
            }}
            className="px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-lg font-semibold"
          >
            Próxima Venda →
          </button>
        </div>
      </div>
    </div>
  )
}
