-- Teste de CRUD para Vendas

-- 1. Inserir uma venda de teste
INSERT INTO "Vendas" ("ClienteId", "ClienteNome", "Valor", "DataVenda", "Status", "DataCriacao") 
VALUES ('CLI-001', 'João Silva', 1500.00, NOW(), 1, NOW());

-- 2. Inserir outra venda
INSERT INTO "Vendas" ("ClienteId", "ClienteNome", "Valor", "DataVenda", "Status", "DataCriacao") 
VALUES ('CLI-002', 'Maria Santos', 2500.00, NOW(), 1, NOW());

-- 3. Inserir itens para a primeira venda
INSERT INTO "VendaItems" ("Produto", "Quantidade", "PrecoUnitario", "VendaId") 
VALUES ('Notebook', 1, 3000.00, 1);

INSERT INTO "VendaItems" ("Produto", "Quantidade", "PrecoUnitario", "VendaId") 
VALUES ('Mouse', 2, 50.00, 1);

-- 4. Contar vendas
SELECT COUNT(*) as total_vendas FROM "Vendas";

-- 5. Listar todas as vendas
SELECT "Id", "ClienteId", "ClienteNome", "Valor", "Status" FROM "Vendas";

-- 6. Listar itens da venda 1
SELECT "Id", "Produto", "Quantidade", "PrecoUnitario", "Total" FROM "VendaItems" WHERE "VendaId" = 1;
