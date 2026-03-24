DELETE FROM "Vendas" WHERE "DataVenda" < NOW() - INTERVAL '90 days';
