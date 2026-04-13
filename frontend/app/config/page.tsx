'use client'

import React, { useState, useEffect } from 'react'
import Link from 'next/link'
import { healthService } from '@/lib/api'
import { HealthStatus } from '@/types'

interface AppSettings {
  theme: 'light' | 'dark' | 'auto'
  autoRefresh: boolean
  itemsPorPagina: number
  notificacoes: boolean
}

export default function ConfigPage() {
  const [settings, setSettings] = useState<AppSettings>(() => {
    // Carregar do localStorage
    if (typeof window !== 'undefined') {
      const saved = localStorage.getItem('appSettings')
      return saved
        ? JSON.parse(saved)
        : {
            theme: 'light',
            autoRefresh: true,
            itemsPorPagina: 10,
            notificacoes: true,
          }
    }
    return {
      theme: 'light',
      autoRefresh: true,
      itemsPorPagina: 10,
      notificacoes: true,
    }
  })

  const [apiVersion, setApiVersion] = useState<string>('N/A')
  const [salvo, setSalvo] = useState(false)

  // Carregar versão da API
  useEffect(() => {
    const carregarVersion = async () => {
      try {
        const response = await healthService.status()
        if (response.sucesso) {
          setApiVersion(response.dados.apiVersion)
        }
      } catch (error) {
        console.error(error)
      }
    }

    carregarVersion()
  }, [])

  // Salvar configurações
  const salvarConfigs = () => {
    localStorage.setItem('appSettings', JSON.stringify(settings))
    setSalvo(true)
    setTimeout(() => setSalvo(false), 3000)
  }

  // Resetar para padrão
  const resetarPadrao = () => {
    const padrao: AppSettings = {
      theme: 'light',
      autoRefresh: true,
      itemsPorPagina: 10,
      notificacoes: true,
    }
    setSettings(padrao)
    localStorage.setItem('appSettings', JSON.stringify(padrao))
    setSalvo(true)
    setTimeout(() => setSalvo(false), 3000)
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">⚙️ Configurações</h1>
        <p className="text-gray-600 mt-1">
          Personalize a aplicação de acordo com suas preferências
        </p>
      </div>

      {/* Alert de Salvo */}
      {salvo && (
        <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg">
          <p className="text-green-800">✅ Configurações salvas com sucesso!</p>
        </div>
      )}

      {/* Main Content */}
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Card - Aparência */}
        <div className="card">
          <h2 className="text-xl font-bold text-gray-900 mb-6 pb-6 border-b border-gray-200">
            🎨 Aparência
          </h2>

          <div className="space-y-4">
            {/* Tema */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Tema da Aplicação
              </label>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                {(['light', 'dark', 'auto'] as const).map((theme) => (
                  <button
                    key={theme}
                    onClick={() => setSettings({ ...settings, theme })}
                    className={`p-4 rounded-lg border-2 transition ${
                      settings.theme === theme
                        ? 'border-blue-500 bg-blue-50'
                        : 'border-gray-200 hover:border-blue-300'
                    }`}
                  >
                    <p className="font-semibold text-gray-900 capitalize">
                      {theme === 'light' ? '☀️ Claro' : theme === 'dark' ? '🌙 Escuro' : '⚡ Automático'}
                    </p>
                    <p className="text-xs text-gray-600 mt-1 capitalize">
                      {theme === 'auto'
                        ? 'Segue configuração do sistema'
                        : `Sempre ${theme}`}
                    </p>
                  </button>
                ))}
              </div>
            </div>
          </div>
        </div>

        {/* Card - Comportamento */}
        <div className="card">
          <h2 className="text-xl font-bold text-gray-900 mb-6 pb-6 border-b border-gray-200">
            ⚙️ Comportamento
          </h2>

          <div className="space-y-4">
            {/* Auto Refresh */}
            <div className="flex items-center justify-between">
              <div>
                <p className="font-medium text-gray-900">Auto-Refresh</p>
                <p className="text-sm text-gray-600">
                  Atualizar dados automaticamente em tempo real
                </p>
              </div>
              <label className="flex items-center cursor-pointer">
                <input
                  type="checkbox"
                  checked={settings.autoRefresh}
                  onChange={(e) => setSettings({ ...settings, autoRefresh: e.target.checked })}
                  className="w-5 h-5"
                />
              </label>
            </div>

            {/* Notificações */}
            <div className="flex items-center justify-between pt-4 border-t border-gray-200">
              <div>
                <p className="font-medium text-gray-900">Notificações</p>
                <p className="text-sm text-gray-600">
                  Receber alertas do sistema
                </p>
              </div>
              <label className="flex items-center cursor-pointer">
                <input
                  type="checkbox"
                  checked={settings.notificacoes}
                  onChange={(e) => setSettings({ ...settings, notificacoes: e.target.checked })}
                  className="w-5 h-5"
                />
              </label>
            </div>

            {/* Itens por Página */}
            <div className="pt-4 border-t border-gray-200">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Itens por Página
              </label>
              <select
                value={settings.itemsPorPagina}
                onChange={(e) => setSettings({ ...settings, itemsPorPagina: parseInt(e.target.value) })}
                className="w-full md:w-40 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value={5}>5 itens</option>
                <option value={10}>10 itens</option>
                <option value={25}>25 itens</option>
                <option value={50}>50 itens</option>
                <option value={100}>100 itens</option>
              </select>
            </div>
          </div>
        </div>

        {/* Card - Sobre */}
        <div className="card">
          <h2 className="text-xl font-bold text-gray-900 mb-6 pb-6 border-b border-gray-200">
            ℹ️ Sobre
          </h2>

          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* App Info */}
              <div>
                <h3 className="font-semibold text-gray-900 mb-3">Aplicação</h3>
                <div className="space-y-2 text-sm">
                  <div>
                    <p className="text-gray-600">Nome</p>
                    <p className="font-semibold text-gray-900">DeepArchive-Bridge</p>
                  </div>
                  <div>
                    <p className="text-gray-600">Versão Frontend</p>
                    <p className="font-semibold text-gray-900">1.0.0</p>
                  </div>
                  <div>
                    <p className="text-gray-600">Framework</p>
                    <p className="font-semibold text-gray-900">Next.js 14</p>
                  </div>
                </div>
              </div>

              {/* API Info */}
              <div>
                <h3 className="font-semibold text-gray-900 mb-3">Servidor API</h3>
                <div className="space-y-2 text-sm">
                  <div>
                    <p className="text-gray-600">URL</p>
                    <p className="font-semibold text-gray-900 break-all">
                      {process.env.NEXT_PUBLIC_API_URL}
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600">Versão API</p>
                    <p className="font-semibold text-gray-900">{apiVersion}</p>
                  </div>
                  <div>
                    <p className="text-gray-600">Tipo DB</p>
                    <p className="font-semibold text-gray-900">SQLite</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Card - Atalhos */}
        <div className="card">
          <h2 className="text-xl font-bold text-gray-900 mb-6 pb-6 border-b border-gray-200">
            🔗 Links Úteis
          </h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <Link
              href="/"
              className="p-3 bg-blue-50 hover:bg-blue-100 border border-blue-200 rounded-lg transition"
            >
              <p className="font-semibold text-blue-900">📊 Dashboard</p>
              <p className="text-xs text-blue-700 mt-1">Voltar para a página inicial</p>
            </Link>

            <Link
              href="/vendas"
              className="p-3 bg-green-50 hover:bg-green-100 border border-green-200 rounded-lg transition"
            >
              <p className="font-semibold text-green-900">📋 Vendas</p>
              <p className="text-xs text-green-700 mt-1">Gerenciar todas as vendas</p>
            </Link>

            <Link
              href="/arquivamento"
              className="p-3 bg-purple-50 hover:bg-purple-100 border border-purple-200 rounded-lg transition"
            >
              <p className="font-semibold text-purple-900">🗂️ Arquivamento</p>
              <p className="text-xs text-purple-700 mt-1">Gerenciar cold storage</p>
            </Link>

            <Link
              href="/health"
              className="p-3 bg-yellow-50 hover:bg-yellow-100 border border-yellow-200 rounded-lg transition"
            >
              <p className="font-semibold text-yellow-900">🏥 Health Status</p>
              <p className="text-xs text-yellow-700 mt-1">Monitorar servidor</p>
            </Link>

            <a
              href="https://github.com"
              target="_blank"
              rel="noopener noreferrer"
              className="p-3 bg-gray-50 hover:bg-gray-100 border border-gray-200 rounded-lg transition"
            >
              <p className="font-semibold text-gray-900">🐙 Repositório</p>
              <p className="text-xs text-gray-700 mt-1">Ver código no GitHub</p>
            </a>

            <a
              href="https://docs.example.com"
              target="_blank"
              rel="noopener noreferrer"
              className="p-3 bg-indigo-50 hover:bg-indigo-100 border border-indigo-200 rounded-lg transition"
            >
              <p className="font-semibold text-indigo-900">📚 Documentação</p>
              <p className="text-xs text-indigo-700 mt-1">Acessar documentação</p>
            </a>
          </div>
        </div>

        {/* Card - Perigo */}
        <div className="card bg-red-50 border-l-4 border-red-500">
          <h2 className="text-xl font-bold text-red-900 mb-6 pb-6 border-b border-red-200">
            ⚠️ Zona de Perigo
          </h2>

          <div className="space-y-4">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div>
                <p className="font-medium text-red-900">Limpar Dados do Navegador</p>
                <p className="text-sm text-red-700 mt-1">
                  Apagar cache, histórico e configurações locais
                </p>
              </div>
              <button
                onClick={() => {
                  if (confirm('Tem certeza? Isto vai limpar todo o cache local.')) {
                    localStorage.clear()
                    alert('Cache limpo!')
                  }
                }}
                className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-semibold whitespace-nowrap"
              >
                🗑️ Limpar Cache
              </button>
            </div>

            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 pt-4 border-t border-red-200">
              <div>
                <p className="font-medium text-red-900">Resetar para Padrão</p>
                <p className="text-sm text-red-700 mt-1">
                  Voltar todas as configurações para o padrão
                </p>
              </div>
              <button
                onClick={() => {
                  if (confirm('Tem certeza? Isto vai resetar todas as configurações.')) {
                    resetarPadrao()
                  }
                }}
                className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-semibold whitespace-nowrap"
              >
                🔄 Resetar
              </button>
            </div>
          </div>
        </div>

        {/* Botões de Ação */}
        <div className="flex gap-3 flex-col sm:flex-row">
          <button
            onClick={salvarConfigs}
            className="flex-1 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-bold transition"
          >
            💾 Salvar Configurações
          </button>
          <Link
            href="/"
            className="flex-1 px-6 py-3 bg-gray-300 hover:bg-gray-400 text-gray-800 rounded-lg font-bold transition text-center"
          >
            ← Voltar
          </Link>
        </div>
      </div>

      {/* Footer Notice */}
      <div className="mt-12 text-center text-sm text-gray-500">
        <p>DeepArchive-Bridge © 2026 • Todas as configurações são armazenadas localmente</p>
      </div>
    </div>
  )
}
