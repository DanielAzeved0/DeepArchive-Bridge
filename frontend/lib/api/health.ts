import { ApiResponse, HealthStatus } from '@/types'
import api from './client'

export const healthService = {
  status: async () => {
    const response = await api.get<ApiResponse<HealthStatus>>('/health')
    return response.data
  },

  ping: async () => {
    const response = await api.get('/health/ping')
    return response.status === 200
  },
}
