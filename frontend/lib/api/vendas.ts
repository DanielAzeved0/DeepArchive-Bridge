import { ApiResponse, CreateVendaRequest, UpdateVendaRequest, Venda, VendaNavigation } from '@/types'
import api from './client'

export const vendaService = {
  buscar: async (filtros: {
    dataInicio: string
    dataFim: string
    clienteId?: string
    status?: string
    skip: number
    take: number
  }) => {
    const response = await api.post<ApiResponse<Venda[]>>('/vendas/buscar', filtros)
    if (response.data.sucesso && Array.isArray(response.data.dados)) {
      return response.data.dados
    }

    return []
  },

  obterPorId: async (id: number) => {
    const response = await api.get<ApiResponse<Venda>>(`/vendas/${id}`)
    return response.data
  },

  obterNavegacao: async (id: number) => {
    const response = await api.get<ApiResponse<VendaNavigation>>(`/vendas/${id}/navigation`)
    return response.data
  },

  criar: async (venda: CreateVendaRequest) => {
    const response = await api.post<ApiResponse<number>>('/vendas', venda)
    return response.data
  },

  atualizar: async (id: number, venda: UpdateVendaRequest) => {
    const response = await api.put<ApiResponse<object>>(`/vendas/${id}`, { ...venda, id })
    return response.data
  },

  aprovar: async (id: number) => {
    const response = await api.post<ApiResponse<object>>(`/vendas/${id}/aprovar`, {})
    return response.data
  },

  deletar: async (id: number) => {
    const response = await api.delete<ApiResponse<object>>(`/vendas/${id}`)
    return response.data
  },
}
