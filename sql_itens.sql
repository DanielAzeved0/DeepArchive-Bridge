SELECT COUNT(*) as total_itens, SUM(CAST("Quantidade" AS INTEGER)) as quantidade_total FROM "VendaItems";
