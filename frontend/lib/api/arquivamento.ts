import { ApiResponse, ArquivamentoInfo, ArquivamentoLog } from '@/types'
import api from './client'

export const archivingService = {
  obterInfo: async () => {
    const response = await api.get<ApiResponse<ArquivamentoInfo>>('/arquivamento/info')
    return response.data
  },

  executar: async () => {
    const response = await api.post<ApiResponse<object>>('/arquivamento/executar')
    return response.data
  },

  executarAutomatico: async () => {
    const response = await api.post<ApiResponse<number>>('/arquivamento/executar-automatico')
    return response.data
  },

  listarLogs: async (skip = 0, take = 20) => {
    const response = await api.get<ApiResponse<ArquivamentoLog[]>>('/arquivamento/logs', {
      params: { skip, take },
    })
    return response.data
  },

  obterUltimo: async () => {
    const response = await api.get<ApiResponse<ArquivamentoLog | null>>('/arquivamento/ultimo')
    return response.data
  },
}
