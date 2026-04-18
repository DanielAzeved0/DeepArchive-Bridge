'use client'

import React, { useState, useEffect } from 'react'
import { useParams } from 'next/navigation'
import { FormVenda } from '@/components/FormVenda'
import { vendaService } from '@/lib/api'
import { Venda, VendaItem } from '@/types'

export default function EditarVendaPage() {
  const params = useParams()
  const vendaId = params.id as string

  const [venda, setVenda] = useState<(Venda & { items?: VendaItem[] }) | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)

  useEffect(() => {
    const carregarVenda = async () => {
      try {
        setCarregando(true)
        setErro(null)

        const response = await vendaService.obterPorId(parseInt(vendaId))

        if (response.sucesso && response.dados) {
          // Mapear 'valor' para 'preco' se necessário
          const vendaProcessada = {
            ...response.dados,
            itens: response.dados.itens?.map((item: any) => ({
              ...item,
              preco: item.preco || item.valor || 0
            })) || []
          }
          setVenda(vendaProcessada)
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

  if (carregando) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 md:p-8 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin text-4xl">⏳</div>
          <p className="text-gray-600 mt-4">Carregando venda para edição...</p>
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
            <a href="/vendas" className="btn-primary">
              ← Voltar para Vendas
            </a>
          </div>
        </div>
      </div>
    )
  }

  return <FormVenda vendaInicial={venda} modo="editar" />
}
