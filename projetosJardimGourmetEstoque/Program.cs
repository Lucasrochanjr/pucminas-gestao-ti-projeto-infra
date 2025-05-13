#nullable enable // Habilita o contexto de anotações anuláveis para o arquivo

using System;
using System.Collections.Generic;
using System.Linq;

// Para representar um item no estoque
public class ItemEstoque
{
    public int Id { get; set; } // Identificador único
    public string Nome { get; set; }
    public string UnidadeMedida { get; set; }
    public int EstoqueMinimoDesejado { get; set; }
    public int QuantidadeAtual { get; set; }
    public string? Categoria { get; set; } // Opcional
    public string? FornecedorPadrao { get; set; } // Opcional
    // Futuramente: public decimal CustoUnitario { get; set; }

    // Construtor
    public ItemEstoque(int id, string nome, string unidadeMedida, int estoqueMinimo, int quantidadeInicial = 0, string? categoria = null, string? fornecedor = null)
    {
        Id = id;
        Nome = nome;
        UnidadeMedida = unidadeMedida;
        EstoqueMinimoDesejado = estoqueMinimo;
        QuantidadeAtual = quantidadeInicial;
        Categoria = categoria;
        FornecedorPadrao = fornecedor;
    }

    public override string ToString()
    {
        return $"ID: {Id} | Nome: {Nome} | Qtd: {QuantidadeAtual} {UnidadeMedida} | Mínimo: {EstoqueMinimoDesejado} {UnidadeMedida}";
    }
}

public class GerenciadorEstoque
{
    private List<ItemEstoque> _itens;
    private int _proximoIdItem;

    public GerenciadorEstoque()
    {
        _itens = new List<ItemEstoque>();
        _proximoIdItem = 1;
    }

    // 1. Cadastro de Item
    public string CadastrarItem(string nome, string unidadeMedida, int estoqueMinimo, string? categoria = null, string? fornecedor = null)
    {
        if (string.IsNullOrWhiteSpace(nome)) return "Erro: Nome do item não pode ser vazio.";
        if (_itens.Any(i => i.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase)))
        {
            return $"Erro: Item '{nome}' já cadastrado.";
        }

        ItemEstoque novoItem = new ItemEstoque(_proximoIdItem++, nome, unidadeMedida, estoqueMinimo, 0, categoria, fornecedor);
        _itens.Add(novoItem);
        return $"Item '{nome}' cadastrado com sucesso. ID: {novoItem.Id}";
    }

    public ItemEstoque? BuscarItemPorId(int id)
    {
        return _itens.FirstOrDefault(i => i.Id == id);
    }

    public List<ItemEstoque> BuscarItemPorNome(string termoBusca)
    {
        if (string.IsNullOrWhiteSpace(termoBusca)) return new List<ItemEstoque>(); // Retorna lista vazia se busca for nula/vazia
        return _itens.Where(i => i.Nome.Contains(termoBusca, StringComparison.OrdinalIgnoreCase)).ToList();
    }


    // 2. Registro de Entrada
    public string RegistrarEntrada(int itemId, int quantidadeEntrada /*, DateTime? dataEntrada = null, decimal? custoEntrada = null */)
    {
        if (quantidadeEntrada <= 0) return "Erro: Quantidade de entrada deve ser positiva.";
        
        ItemEstoque? item = BuscarItemPorId(itemId);
        if (item == null)
        {
            return "Erro: Item não encontrado para registrar entrada.";
        }

        item.QuantidadeAtual += quantidadeEntrada;
        return $"Entrada de {quantidadeEntrada} {item.UnidadeMedida} de '{item.Nome}' registrada. Novo estoque: {item.QuantidadeAtual} {item.UnidadeMedida}.";
    }

    // 3. Registro de Saída
    public string RegistrarSaida(int itemId, int quantidadeSaida /*, string? motivoSaida = null, DateTime? dataSaida = null */)
    {
        if (quantidadeSaida <= 0) return "Erro: Quantidade de saída deve ser positiva.";

        ItemEstoque? item = BuscarItemPorId(itemId);
        if (item == null)
        {
            return "Erro: Item não encontrado para registrar saída.";
        }

        if (item.QuantidadeAtual < quantidadeSaida)
        {
            return $"Erro: Estoque insuficiente de '{item.Nome}'. Disponível: {item.QuantidadeAtual} {item.UnidadeMedida}.";
        }

        item.QuantidadeAtual -= quantidadeSaida;
        string alerta = VerificarNivelMinimoAposOperacao(item);
        return $"Saída de {quantidadeSaida} {item.UnidadeMedida} de '{item.Nome}' registrada. Novo estoque: {item.QuantidadeAtual} {item.UnidadeMedida}.{alerta}";
    }

    // 4. Ajuste de Inventário
    public string AjustarInventario(int itemId, int quantidadeReal)
    {
        if (quantidadeReal < 0) return "Erro: Quantidade real não pode ser negativa.";

        ItemEstoque? item = BuscarItemPorId(itemId);
        if (item == null)
        {
            return "Erro: Item não encontrado para ajuste de inventário.";
        }

        item.QuantidadeAtual = quantidadeReal;
        string alerta = VerificarNivelMinimoAposOperacao(item);
        return $"Inventário de '{item.Nome}' ajustado para {item.QuantidadeAtual} {item.UnidadeMedida}.{alerta}";
    }

    // Verificação de Nível Mínimo (Interno)
    private string VerificarNivelMinimoAposOperacao(ItemEstoque item)
    {
        if (item.QuantidadeAtual <= item.EstoqueMinimoDesejado)
        {
            return $" ATENÇÃO: Estoque baixo para '{item.Nome}'!";
        }
        return "";
    }

    // Saídas Geradas
    public void ListarTodosItens()
    {
        Console.WriteLine("\n--- Listagem Completa de Itens ---");
        if (!_itens.Any())
        {
            Console.WriteLine("Nenhum item cadastrado.");
            return;
        }
        foreach (var item in _itens)
        {
            Console.WriteLine(item.ToString());
            if (item.QuantidadeAtual <= item.EstoqueMinimoDesejado)
            {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine($"  -> ALERTA: Estoque baixo!");
                 Console.ResetColor();
            }
        }
    }

    public void ExibirAlertaEstoqueBaixo()
    {
        Console.WriteLine("\n--- Itens com Estoque Baixo ---");
        var itensBaixoEstoque = _itens.Where(i => i.QuantidadeAtual <= i.EstoqueMinimoDesejado).ToList();
        if (!itensBaixoEstoque.Any())
        {
            Console.WriteLine("Nenhum item com estoque baixo no momento.");
            return;
        }
        foreach (var item in itensBaixoEstoque)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(item.ToString());
            Console.ResetColor();
        }
    }

    public void ExibirDetalhesItem(int itemId)
    {
        ItemEstoque? item = BuscarItemPorId(itemId);
        if (item == null)
        {
            Console.WriteLine("Item não encontrado.");
            return;
        }
        Console.WriteLine("\n--- Detalhes do Item ---");
        Console.WriteLine($"ID: {item.Id}");
        Console.WriteLine($"Nome: {item.Nome}");
        Console.WriteLine($"Unidade de Medida: {item.UnidadeMedida}");
        Console.WriteLine($"Quantidade Atual: {item.QuantidadeAtual}");
        Console.WriteLine($"Estoque Mínimo Desejado: {item.EstoqueMinimoDesejado}");
        Console.WriteLine($"Categoria: {item.Categoria ?? "N/A"}");
        Console.WriteLine($"Fornecedor Padrão: {item.FornecedorPadrao ?? "N/A"}");
    }
}

public class Program
{
    static GerenciadorEstoque gerenciador = new GerenciadorEstoque();

    public static void Main(string[] args)
    {
        // Populando com alguns dados iniciais para teste
        gerenciador.CadastrarItem("Farinha de Trigo Tipo 1", "Kg", 5, "Pizzaria");
        gerenciador.CadastrarItem("Polpa de Açaí Médio", "Kg", 10, "Açaí");
        gerenciador.CadastrarItem("Refrigerante Lata 350ml", "Unidade", 24, "Bebidas");
        
        gerenciador.RegistrarEntrada(1, 20); // Entrada de Farinha
        gerenciador.RegistrarEntrada(2, 30); // Entrada de Polpa de Açaí
        gerenciador.RegistrarEntrada(3, 50); // Entrada de Refrigerante


        bool sair = false;
        while (!sair)
        {
            Console.Clear(); // Limpa o console para a próxima iteração
            Console.WriteLine("\n--- Jardim Espaço Gourmet - Gestão de Estoque ---");
            Console.WriteLine("1. Cadastrar Novo Item");
            Console.WriteLine("2. Registrar Entrada de Item");
            Console.WriteLine("3. Registrar Saída de Item");
            Console.WriteLine("4. Ajustar Inventário");
            Console.WriteLine("5. Listar Todos os Itens");
            Console.WriteLine("6. Ver Itens com Estoque Baixo");
            Console.WriteLine("7. Buscar Item por Nome");
            Console.WriteLine("8. Ver Detalhes de um Item");
            Console.WriteLine("0. Sair");
            Console.Write("Escolha uma opção: ");

            string? opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    CadastrarNovoItemUI();
                    break;
                case "2":
                    RegistrarEntradaUI();
                    break;
                case "3":
                    RegistrarSaidaUI();
                    break;
                case "4":
                    AjustarInventarioUI();
                    break;
                case "5":
                    gerenciador.ListarTodosItens();
                    break;
                case "6":
                    gerenciador.ExibirAlertaEstoqueBaixo();
                    break;
                case "7":
                    BuscarItemPorNomeUI();
                    break;
                case "8":
                    VerDetalhesItemUI();
                    break;
                case "0":
                    sair = true;
                    Console.WriteLine("Saindo do sistema...");
                    break;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }
            if (!sair) // Só pausa se não for sair
            {
                Console.WriteLine("\nPressione qualquer tecla para continuar...");
                Console.ReadKey();
            }
        }
    }

    static void CadastrarNovoItemUI()
    {
        Console.WriteLine("\n--- Cadastro de Novo Item ---");
        Console.Write("Nome do Item: ");
        string? nome = Console.ReadLine();
        Console.Write("Unidade de Medida (Kg, Litro, Unidade, Pacote): ");
        string? unidade = Console.ReadLine();
        Console.Write("Estoque Mínimo Desejado: ");
        if (!int.TryParse(Console.ReadLine(), out int min))
        {
            Console.WriteLine("Estoque mínimo inválido. Deve ser um número.");
            return;
        }
        Console.Write("Categoria (opcional): ");
        string? categoria = Console.ReadLine();
        Console.Write("Fornecedor Padrão (opcional): ");
        string? fornecedor = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(nome) && !string.IsNullOrWhiteSpace(unidade))
        {
            string resultado = gerenciador.CadastrarItem(nome, unidade, min, string.IsNullOrWhiteSpace(categoria) ? null : categoria, string.IsNullOrWhiteSpace(fornecedor) ? null : fornecedor);
            Console.WriteLine(resultado);
        }
        else
        {
            Console.WriteLine("Nome e Unidade de Medida são obrigatórios.");
        }
    }

    static void RegistrarEntradaUI()
    {
        Console.WriteLine("\n--- Registrar Entrada de Item ---");
        Console.Write("ID do Item: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("ID inválido. Deve ser um número.");
            return;
        }
        Console.Write("Quantidade Entrada: ");
        if (!int.TryParse(Console.ReadLine(), out int qtd) || qtd <=0)
        {
            Console.WriteLine("Quantidade inválida. Deve ser um número positivo.");
            return;
        }
        string resultado = gerenciador.RegistrarEntrada(id, qtd);
        Console.WriteLine(resultado);
    }

    static void RegistrarSaidaUI()
    {
        Console.WriteLine("\n--- Registrar Saída de Item ---");
        Console.Write("ID do Item: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("ID inválido. Deve ser um número.");
            return;
        }
        Console.Write("Quantidade Saída: ");
        if (!int.TryParse(Console.ReadLine(), out int qtd) || qtd <= 0)
        {
            Console.WriteLine("Quantidade inválida. Deve ser um número positivo.");
            return;
        }
        string resultado = gerenciador.RegistrarSaida(id, qtd);
        Console.WriteLine(resultado);
    }
    
    static void AjustarInventarioUI()
    {
        Console.WriteLine("\n--- Ajustar Inventário ---");
        Console.Write("ID do Item: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("ID inválido. Deve ser um número.");
            return;
        }
        Console.Write("Quantidade Atual Real (contagem física): ");
         if (!int.TryParse(Console.ReadLine(), out int qtd) || qtd < 0)
        {
            Console.WriteLine("Quantidade inválida. Deve ser um número não negativo.");
            return;
        }
        string resultado = gerenciador.AjustarInventario(id, qtd);
        Console.WriteLine(resultado);
    }

    static void BuscarItemPorNomeUI()
    {
        Console.WriteLine("\n--- Buscar Item por Nome ---");
        Console.Write("Digite parte do nome do item: ");
        string? termoBusca = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(termoBusca))
        {
            Console.WriteLine("Termo de busca não pode ser vazio.");
            return;
        }
        var itensEncontrados = gerenciador.BuscarItemPorNome(termoBusca);
        if (itensEncontrados.Any())
        {
            Console.WriteLine("Itens encontrados:");
            foreach(var item in itensEncontrados)
            {
                Console.WriteLine(item.ToString());
            }
        }
        else
        {
            Console.WriteLine("Nenhum item encontrado com este termo.");
        }
    }
    
    static void VerDetalhesItemUI()
    {
         Console.Write("ID do Item para ver detalhes: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("ID inválido. Deve ser um número.");
            return;
        }
        gerenciador.ExibirDetalhesItem(id);
    }
}
