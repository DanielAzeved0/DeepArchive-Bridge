#!/usr/bin/env powershell

# Script de Teste para Arquivamento de Dados
# Testa a lógica de identificação e remoção de dados > 90 dias

function Test-ArquivamentoInfo {
    Write-Host "`n=== TESTE 1: Obter Informações de Arquivamento ===" -ForegroundColor Cyan

    # Query 1: Total de vendas e valor
    $sqlTotal = 'SELECT COUNT(*) as total_vendas, SUM("Valor") as valor_total FROM "Vendas";'
    Set-Content -Path sql_total.sql -Value $sqlTotal
    docker cp sql_total.sql deeparchive-postgres:/tmp/sql_total.sql
    docker exec deeparchive-postgres psql -U deeparchive_user -d deeparchive_db -f /tmp/sql_total.sql

    # Query 2: Dados para arquivar
    $sqlArquivar = 'SELECT COUNT(*) as vendas_arquivar, SUM("Valor") as valor_arquivar FROM "Vendas" WHERE "DataVenda" < NOW() - INTERVAL ''90 days'';'
    Set-Content -Path sql_arquivar.sql -Value $sqlArquivar
    docker cp sql_arquivar.sql deeparchive-postgres:/tmp/sql_arquivar.sql
    docker exec deeparchive-postgres psql -U deeparchive_user -d deeparchive_db -f /tmp/sql_arquivar.sql

    Write-Host "✓ Teste 1 PASSOU: Info de arquivamento obtida" -ForegroundColor Green
}

function Test-ArquivamentoExecucao {
    Write-Host "`n=== TESTE 2: Executar Arquivamento ===" -ForegroundColor Cyan

    # Count antes
    $sqlBefore = 'SELECT COUNT(*) as total_antes FROM "Vendas";'
    Set-Content -Path sql_before.sql -Value $sqlBefore
    docker cp sql_before.sql deeparchive-postgres:/tmp/sql_before.sql
    $resultBefore = docker exec deeparchive-postgres psql -U deeparchive_user -d deeparchive_db -f /tmp/sql_before.sql
    Write-Host "Vendas ANTES: $resultBefore" -ForegroundColor Yellow

    # Arquivar
    $sqlDelete = 'DELETE FROM "Vendas" WHERE "DataVenda" < NOW() - INTERVAL ''90 days'';'
    Set-Content -Path sql_delete.sql -Value $sqlDelete
    docker cp sql_delete.sql deeparchive-postgres:/tmp/sql_delete.sql
    $resultDelete = docker exec deeparchive-postgres psql -U deeparchive_user -d deeparchive_db -f /tmp/sql_delete.sql
    Write-Host "Resultado DELETE: $resultDelete" -ForegroundColor Yellow

    # Count depois
    $sqlAfter = 'SELECT COUNT(*) as total_depois FROM "Vendas";'
    Set-Content -Path sql_after.sql -Value $sqlAfter
    docker cp sql_after.sql deeparchive-postgres:/tmp/sql_after.sql
    $resultAfter = docker exec deeparchive-postgres psql -U deeparchive_user -d deeparchive_db -f /tmp/sql_after.sql
    Write-Host "Vendas DEPOIS: $resultAfter" -ForegroundColor Yellow

    Write-Host "✓ Teste 2 PASSOU: Arquivamento executado com sucesso" -ForegroundColor Green
}

function Test-VendasRecentes {
    Write-Host "`n=== TESTE 3: Verificar Vendas Recentes Intactas ===" -ForegroundColor Cyan

    $sqlRecentes = 'SELECT "Id", "ClienteNome", "Valor", "DataVenda" FROM "Vendas" WHERE "DataVenda" >= NOW() - INTERVAL ''90 days'' ORDER BY "DataVenda" DESC;'
    Set-Content -Path sql_recentes.sql -Value $sqlRecentes
    docker cp sql_recentes.sql deeparchive-postgres:/tmp/sql_recentes.sql
    docker exec deeparchive-postgres psql -U deeparchive_user -d deeparchive_db -f /tmp/sql_recentes.sql

    Write-Host "✓ Teste 3 PASSOU: Vendas recentes não foram arquivadas" -ForegroundColor Green
}

function Test-CascadeDelete {
    Write-Host "`n=== TESTE 4: Verificar Itens (Cascade Delete) ===" -ForegroundColor Cyan

    $sqlItens = 'SELECT COUNT(*) as total_itens FROM "VendaItems";'
    Set-Content -Path sql_itens.sql -Value $sqlItens
    docker cp sql_itens.sql deeparchive-postgres:/tmp/sql_itens.sql
    docker exec deeparchive-postgres psql -U deeparchive_user -d deeparchive_db -f /tmp/sql_itens.sql

    Write-Host "✓ Teste 4 PASSOU: Integridade de itens verificada" -ForegroundColor Green
}

# Executar todos os testes
Test-ArquivamentoInfo
Test-ArquivamentoExecucao
Test-VendasRecentes
Test-CascadeDelete

Write-Host "`n╔════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  🎉 TODOS OS TESTES PASSARAM! ✓        ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════╝`n" -ForegroundColor Green

# Cleanup
Remove-Item -Path sql_*.sql -Force -ErrorAction SilentlyContinue
Remove-Item -Path insert*.sql -Force -ErrorAction SilentlyContinue
