'use client'

import React, { useState, useEffect } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { vendaService } from '@/lib/api'
import { Venda, VendaItem, ApiResponse, VendaRequest, VendaItemRequest } from '@/types'
import { formatCurrency } from '@/lib/formatters'

interface FormVendaProps {
  vendaInicial?: Venda & { items?: VendaItem[] }
  modo: 'criar' | 'editar'
}

export function FormVenda({ vendaInicial, modo }: FormVendaProps) {
  const router = useRouter()
  const [clienteNome, setClienteNome] = useState(vendaInicial?.clienteNome || '')
  const [dataVenda, setDataVenda] = useState(
    vendaInicial?.dataVenda ? vendaInicial.dataVenda.split('T')[0] : new Date().toISOString().split('T')[0]
  )
  const [itens, setItens] = useState<(VendaItem & { id?: number })[]>(
    vendaInicial?.itens || [{ id: 0, produto: '', precoUnitario: 0, quantidade: 1 }]
  )

  const [carregando, setCarregando] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const [sucesso, setSucesso] = useState(false)

  // Validação básica
  const validar = (): string | null => {
    if (!clienteNome.trim()) return 'Cliente é obrigatório'
    if (!dataVenda) return 'Data é obrigatória'
    if (itens.length === 0) return 'Adicione pelo menos um item'
    
    for (let item of itens) {
      if (!item.produto?.trim()) return 'Todos os itens devem ter descrição'
      if (!item.precoUnitario || item.precoUnitario <= 0) return 'Valor do item deve ser maior que 0'
      if (!item.quantidade || item.quantidade <= 0) return 'Quantidade deve ser maior que 0'
    }
    
    return null
  }

  // Handlers para itens
  const adicionarItem = () => {
    setItens([...itens, { id: 0, produto: '', precoUnitario: 0, quantidade: 1 }])
  }

  const removerItem = (index: number) => {
    if (itens.length > 1) {
      setItens(itens.filter((_, i) => i !== index))
    } else {
      alert('Você deve ter pelo menos um item!')
    }
  }

  const atualizarItem = (index: number, campo: string, valor: any) => {
    const novosItens = [...itens]
    novosItens[index] = { ...novosItens[index], [campo]: valor }
    setItens(novosItens)
  }

  // Submeter form
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    const erroValidacao = validar()
    if (erroValidacao) {
      setErro(erroValidacao)
      return
    }

    try {
      setCarregando(true)
      setErro(null)
      setSucesso(false)

      const dadosVenda: VendaRequest = {
        clienteNome,
        dataVenda: new Date(dataVenda).toISOString(),
        valor: itens.reduce((acc, item) => acc + item.precoUnitario * item.quantidade, 0),
        status: 1, // 1 = Pendente
        itens: itens.map(({ id, ...rest }) => ({
          descricao: rest.produto,
          valor: rest.precoUnitario,
          quantidade: rest.quantidade,
        })),
      }

      let response: ApiResponse<any>
      
      if (modo === 'criar') {
        response = await vendaService.criar(dadosVenda as any)
      } else {
        response = await vendaService.atualizar(
          vendaInicial?.id || 0,
          dadosVenda as any
        )
      }

      if (response.sucesso) {
        setSucesso(true)
        setTimeout(() => {
          router.push(`/vendas/${response.dados || vendaInicial?.id}`)
        }, 1500)
      } else {
        setErro(response.mensagem || 'Erro ao salvar venda')
      }
    } catch (error) {
      setErro('Falha ao conectar com o servidor')
      console.error(error)
    } finally {
      setCarregando(false)
    }
  }

  const totalVenda = itens.reduce((acc, item) => acc + (item.precoUnitario * item.quantidade || 0), 0)

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">
          {modo === 'criar' ? '📝 Nova Venda' : '✏️ Editar Venda'}
        </h1>
        <p className="text-gray-600 mt-1">
          {modo === 'criar' ? 'Crie uma nova venda no sistema' : 'Edite os dados da venda'}
        </p>
      </div>

      {/* Alert de Sucesso */}
      {sucesso && (
        <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg">
          <p className="text-green-800">✅ Venda salva com sucesso! Redirecionando...</p>
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

      {/* Form Container */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Form - Left Side */}
        <form onSubmit={handleSubmit} className="lg:col-span-2 space-y-6">
          {/* Card - Informações Básicas */}
          <div className="card">
            <h2 className="text-xl font-bold text-gray-900 mb-6">👤 Informações Básicas</h2>

            <div className="space-y-4">
              {/* Cliente */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Cliente *
                </label>
                <input
                  type="text"
                  value={clienteNome}
                  onChange={(e) => setClienteNome(e.target.value)}
                  placeholder="Nome do cliente..."
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  disabled={carregando}
                />
              </div>

              {/* Data */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Data da Venda *
                </label>
                <input
                  type="date"
                  value={dataVenda}
                  onChange={(e) => setDataVenda(e.target.value)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  disabled={carregando}
                />
              </div>
            </div>
          </div>

          {/* Card - Itens da Venda */}
          <div className="card">
            <div className="flex flex-col md:flex-row md:items-center md:justify-between mb-6 pb-6 border-b border-gray-200">
              <h2 className="text-xl font-bold text-gray-900">📦 Itens da Venda</h2>
              <button
                type="button"
                onClick={adicionarItem}
                disabled={carregando}
                className="mt-3 md:mt-0 px-4 py-2 bg-green-500 hover:bg-green-600 disabled:opacity-50 text-white rounded-lg font-semibold transition"
              >
                ➕ Adicionar Item
              </button>
            </div>

            {/* Items List */}
            <div className="space-y-4">
              {itens.map((item, index) => (
                <div key={index} className="p-4 border border-gray-200 rounded-lg bg-gray-50">
                  <div className="flex justify-between items-start mb-3">
                    <span className="font-semibold text-gray-900">Item {index + 1}</span>
                    <button
                      type="button"
                      onClick={() => removerItem(index)}
                      disabled={carregando || itens.length === 1}
                      className="px-2 py-1 bg-red-500 hover:bg-red-600 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded text-xs font-semibold"
                    >
                      🗑️ Remover
                    </button>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                    {/* Produto */}
                    <div>
                      <label className="block text-xs font-medium text-gray-700 mb-1">
                        Produto *
                      </label>
                      <input
                        type="text"
                        value={item.produto || ''}
                        onChange={(e) => atualizarItem(index, 'produto', e.target.value)}
                        placeholder="Ex: Produto A"
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                        disabled={carregando}
                      />
                    </div>

                    {/* Preço Unitário */}
                    <div>
                      <label className="block text-xs font-medium text-gray-700 mb-1">
                        Preço Unit. *
                      </label>
                      <input
                        type="number"
                        step="0.01"
                        min="0"
                        value={item.precoUnitario || 0}
                        onChange={(e) => atualizarItem(index, 'precoUnitario', parseFloat(e.target.value))}
                        placeholder="0,00"
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                        disabled={carregando}
                      />
                    </div>

                    {/* Quantidade */}
                    <div>
                      <label className="block text-xs font-medium text-gray-700 mb-1">
                        Quantidade *
                      </label>
                      <input
                        type="number"
                        step="1"
                        min="1"
                        value={item.quantidade || 1}
                        onChange={(e) => atualizarItem(index, 'quantidade', parseInt(e.target.value))}
                        placeholder="1"
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                        disabled={carregando}
                      />
                    </div>
                  </div>

                  {/* Subtotal do Item */}
                  <div className="mt-3 pt-3 border-t border-gray-200 text-right">
                    <p className="text-sm text-gray-600">
                      Subtotal:{' '}
                      <span className="font-bold text-gray-900">
                        {formatCurrency((item.precoUnitario || 0) * (item.quantidade || 1))}
                      </span>
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Botões de Ação */}
          <div className="flex gap-3 flex-col sm:flex-row">
            <button
              type="submit"
              disabled={carregando}
              className="flex-1 px-6 py-3 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-lg font-bold transition"
            >
              {carregando ? '⏳ Salvando...' : modo === 'criar' ? '✅ Criar Venda' : '✅ Salvar Alterações'}
            </button>
            <Link
              href="/vendas"
              className="flex-1 px-6 py-3 bg-gray-300 hover:bg-gray-400 text-gray-800 rounded-lg font-bold transition text-center"
            >
              ← Cancelar
            </Link>
          </div>
        </form>

        {/* Summary Card - Right Side */}
        <div className="space-y-6">
          {/* Total */}
          <div className="card bg-blue-50 border-l-4 border-blue-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">💰 Resumo</h3>

            <div className="space-y-3">
              <div className="flex justify-between text-sm">
                <span className="text-gray-700">Total de Itens:</span>
                <span className="font-semibold text-gray-900">{itens.length}</span>
              </div>

              <div className="flex justify-between text-sm pb-3 border-b border-gray-200">
                <span className="text-gray-700">Quantidade Total:</span>
                <span className="font-semibold text-gray-900">
                  {itens.reduce((acc, item) => acc + (item.quantidade || 0), 0)}x
                </span>
              </div>

              <div className="bg-blue-100 px-4 py-3 rounded-lg">
                <p className="text-sm text-gray-600 mb-1">Total da Venda</p>
                <p className="text-3xl font-bold text-blue-600">
                  {formatCurrency(totalVenda)}
                </p>
              </div>
            </div>
          </div>

          {/* Info */}
          <div className="card bg-yellow-50 border-l-4 border-yellow-500">
            <h3 className="text-lg font-bold text-gray-900 mb-4">ℹ️ Informações</h3>

            <div className="text-sm text-gray-700 space-y-2">
              <p>
                • Todos os campos marcados com <span className="text-red-600 font-bold">*</span> são obrigatórios
              </p>
              <p>
                • Você pode adicionar múltiplos itens à venda
              </p>
              <p>
                • O total é calculado automaticamente
              </p>
              <p>
                • A venda será criada com status <span className="font-semibold">Pendente</span>
              </p>
            </div>
          </div>

          {/* Validação */}
          {erro && (
            <div className="card bg-red-50 border-l-4 border-red-500">
              <h3 className="text-lg font-bold text-red-900 mb-2">⚠️ Erro de Validação</h3>
              <p className="text-sm text-red-800">{erro}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
