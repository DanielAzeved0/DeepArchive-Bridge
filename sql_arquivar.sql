SELECT COUNT(*) as vendas_arquivar, SUM("Valor") as valor_arquivar FROM "Vendas" WHERE "DataVenda" < NOW() - INTERVAL '90 days';
