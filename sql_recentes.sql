SELECT "Id", "ClienteNome", "Valor" FROM "Vendas" WHERE "DataVenda" >= NOW() - INTERVAL '90 days' ORDER BY "DataVenda" DESC;
