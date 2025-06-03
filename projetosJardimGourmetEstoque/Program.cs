#nullable enable // Habilita o contexto de anotações anuláveis para o arquivo

using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient; // Diretiva using no local correto
using System.Data;          // Diretiva using no local correto

// Para representar um item no estoque
public class ItemEstoque
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string UnidadeMedida { get; set; }
    public int EstoqueMinimoDesejado { get; set; }
    public int QuantidadeAtual { get; set; }
    public string? Categoria { get; set; }
    public string? FornecedorPadrao { get; set; }

    public ItemEstoque()
    {
        Nome = string.Empty;
        UnidadeMedida = string.Empty;
    }

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
    private string _connectionString_interna; // Renomeado para clareza, poderia ser _connectionString

    public GerenciadorEstoque(string connectionStringParam) // Construtor recebe a string de conexão
    {
        _connectionString_interna = connectionStringParam; // Armazena a string de conexão
    }

    private MySqlConnection GetConnection()
    {
        return new MySqlConnection(_connectionString_interna);
    }

    public string CadastrarItem(string nome, string unidadeMedida, int estoqueMinimo, int quantidadeInicial = 0, string? categoria = null, string? fornecedor = null)
    {
        if (string.IsNullOrWhiteSpace(nome)) return "Erro: Nome do item não pode ser vazio.";
        if (string.IsNullOrWhiteSpace(unidadeMedida)) return "Erro: Unidade de medida não pode ser vazia.";
        if (estoqueMinimo < 0) return "Erro: Estoque mínimo não pode ser negativo.";
        if (quantidadeInicial < 0) return "Erro: Quantidade inicial não pode ser negativa.";

        if (BuscarItemPorNomeExato(nome) != null)
        {
            return $"Erro: Item '{nome}' já cadastrado.";
        }

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                string query = "INSERT INTO ItensEstoque (Nome, UnidadeMedida, EstoqueMinimoDesejado, QuantidadeAtual, Categoria, FornecedorPadrao) " +
                               "VALUES (@Nome, @UnidadeMedida, @EstoqueMinimoDesejado, @QuantidadeAtual, @Categoria, @FornecedorPadrao); SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nome", nome);
                cmd.Parameters.AddWithValue("@UnidadeMedida", unidadeMedida);
                cmd.Parameters.AddWithValue("@EstoqueMinimoDesejado", estoqueMinimo);
                cmd.Parameters.AddWithValue("@QuantidadeAtual", quantidadeInicial);
                cmd.Parameters.AddWithValue("@Categoria", (object?)categoria ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FornecedorPadrao", (object?)fornecedor ?? DBNull.Value);

                long novoId = Convert.ToInt64(cmd.ExecuteScalar());
                return $"Item '{nome}' cadastrado com sucesso. ID: {novoId}";
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // Código de erro para entrada duplicada
                {
                    return $"Erro: Item '{nome}' já cadastrado (detectado pelo banco de dados).";
                }
                // Para outros erros de MySQL, log ou mostre uma mensagem mais genérica ou específica
                Console.WriteLine($"DEBUG MySqlException: {ex.ToString()}"); // Linha de debug
                return $"Erro ao cadastrar item no banco de dados: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG Exception: {ex.ToString()}"); // Linha de debug
                return $"Erro inesperado ao cadastrar item: {ex.Message}";
            }
        }
    }

    private ItemEstoque? BuscarItemPorNomeExato(string nome)
    {
        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                string query = "SELECT * FROM ItensEstoque WHERE Nome = @Nome";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nome", nome);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapReaderToItemEstoque(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar item por nome exato (interno): {ex.Message}");
            }
        }
        return null;
    }

    public ItemEstoque? BuscarItemPorId(int id)
    {
        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                string query = "SELECT * FROM ItensEstoque WHERE Id = @Id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapReaderToItemEstoque(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar item por ID: {ex.Message}");
            }
        }
        return null;
    }

    public List<ItemEstoque> BuscarItemPorNome(string termoBusca)
    {
        List<ItemEstoque> itensEncontrados = new List<ItemEstoque>();
        if (string.IsNullOrWhiteSpace(termoBusca)) return itensEncontrados;

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                string query = "SELECT * FROM ItensEstoque WHERE Nome LIKE @TermoBusca";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TermoBusca", $"%{termoBusca}%");
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itensEncontrados.Add(MapReaderToItemEstoque(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar item por nome: {ex.Message}");
            }
        }
        return itensEncontrados;
    }

    private ItemEstoque MapReaderToItemEstoque(MySqlDataReader reader)
    {
        return new ItemEstoque
        {
            Id = reader.GetInt32("Id"),
            Nome = reader.GetString("Nome"),
            UnidadeMedida = reader.GetString("UnidadeMedida"),
            EstoqueMinimoDesejado = reader.GetInt32("EstoqueMinimoDesejado"),
            QuantidadeAtual = reader.GetInt32("QuantidadeAtual"),
            Categoria = reader.IsDBNull(reader.GetOrdinal("Categoria")) ? null : reader.GetString("Categoria"),
            FornecedorPadrao = reader.IsDBNull(reader.GetOrdinal("FornecedorPadrao")) ? null : reader.GetString("FornecedorPadrao")
        };
    }

    public string RegistrarEntrada(int itemId, int quantidadeEntrada)
    {
        if (quantidadeEntrada <= 0) return "Erro: Quantidade de entrada deve ser positiva.";
        ItemEstoque? item = BuscarItemPorId(itemId);
        if (item == null) return "Erro: Item não encontrado para registrar entrada.";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                string query = "UPDATE ItensEstoque SET QuantidadeAtual = QuantidadeAtual + @QuantidadeEntrada WHERE Id = @ItemId";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@QuantidadeEntrada", quantidadeEntrada);
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    item.QuantidadeAtual += quantidadeEntrada;
                    return $"Entrada de {quantidadeEntrada} {item.UnidadeMedida} de '{item.Nome}' registrada. Novo estoque: {item.QuantidadeAtual} {item.UnidadeMedida}.";
                }
                return "Erro: Não foi possível registrar a entrada (nenhuma linha afetada).";
            }
            catch (Exception ex)
            {
                return $"Erro ao registrar entrada no banco de dados: {ex.Message}";
            }
        }
    }

    public string RegistrarSaida(int itemId, int quantidadeSaida)
    {
        if (quantidadeSaida <= 0) return "Erro: Quantidade de saída deve ser positiva.";
        ItemEstoque? item = BuscarItemPorId(itemId);
        if (item == null) return "Erro: Item não encontrado para registrar saída.";
        if (item.QuantidadeAtual < quantidadeSaida) return $"Erro: Estoque insuficiente de '{item.Nome}'. Disponível: {item.QuantidadeAtual} {item.UnidadeMedida}.";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                string query = "UPDATE ItensEstoque SET QuantidadeAtual = QuantidadeAtual - @QuantidadeSaida WHERE Id = @ItemId";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@QuantidadeSaida", quantidadeSaida);
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    item.QuantidadeAtual -= quantidadeSaida;
                    string alerta = VerificarNivelMinimoAposOperacao(item);
                    return $"Saída de {quantidadeSaida} {item.UnidadeMedida} de '{item.Nome}' registrada. Novo estoque: {item.QuantidadeAtual} {item.UnidadeMedida}.{alerta}";
                }
                return "Erro: Não foi possível registrar a saída (nenhuma linha afetada).";
            }
            catch (Exception ex)
            {
                return $"Erro ao registrar saída no banco de dados: {ex.Message}";
            }
        }
    }

    public string AjustarInventario(int itemId, int quantidadeReal)
    {
        if (quantidadeReal < 0) return "Erro: Quantidade real não pode ser negativa.";
        ItemEstoque? item = BuscarItemPorId(itemId);
        if (item == null) return "Erro: Item não encontrado para ajuste de inventário.";

        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                string query = "UPDATE ItensEstoque SET QuantidadeAtual = @QuantidadeReal WHERE Id = @ItemId";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@QuantidadeReal", quantidadeReal);
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    item.QuantidadeAtual = quantidadeReal;
                    string alerta = VerificarNivelMinimoAposOperacao(item);
                    return $"Inventário de '{item.Nome}' ajustado para {item.QuantidadeAtual} {item.UnidadeMedida}.{alerta}";
                }
                return "Erro: Não foi possível ajustar o inventário (nenhuma linha afetada).";
            }
            catch (Exception ex)
            {
                return $"Erro ao ajustar inventário no banco de dados: {ex.Message}";
            }
        }
    }

    private string VerificarNivelMinimoAposOperacao(ItemEstoque item)
    {
        if (item.QuantidadeAtual <= item.EstoqueMinimoDesejado)
        {
            return $" ATENÇÃO: Estoque baixo para '{item.Nome}'!";
        }
        return "";
    }

    public void ListarTodosItens()
    {
        Console.WriteLine("\n--- Listagem Completa de Itens ---");
        List<ItemEstoque> todosItens = new List<ItemEstoque>();
        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                string query = "SELECT * FROM ItensEstoque ORDER BY Nome";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        todosItens.Add(MapReaderToItemEstoque(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao listar itens: {ex.Message}");
                return;
            }
        }
        if (!todosItens.Any())
        {
            Console.WriteLine("Nenhum item cadastrado.");
            return;
        }
        foreach (var item in todosItens)
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
        List<ItemEstoque> itensBaixoEstoque = new List<ItemEstoque>();
        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                string query = "SELECT * FROM ItensEstoque WHERE QuantidadeAtual <= EstoqueMinimoDesejado ORDER BY Nome";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itensBaixoEstoque.Add(MapReaderToItemEstoque(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar itens com estoque baixo: {ex.Message}");
                return;
            }
        }
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

public class Program // Certifique-se de que esta é a única declaração de classe Program de nível superior
{
    // A string de conexão está definida aqui dentro da classe Program
    // Certifique-se de que esta é a string correta que funciona no seu MySQL Workbench
    private static string connectionString = "Server=127.0.0.1;Port=3306;Database=seu_jardim_gourmet_dbnew;Uid=root;Pwd=Cloud2025@;";

    // gerenciador agora usa a connectionString definida acima
    // Esta inicialização deve estar DENTRO de um método (como Main) ou ser um inicializador de campo estático
    // Se for usado por métodos estáticos, ele próprio deve ser estático.
    static GerenciadorEstoque gerenciador = new GerenciadorEstoque(connectionString);

    public static void Main(string[] args) // Este é o ponto de entrada correto
    {
        // O 'gerenciador' já foi inicializado acima.

        try
        {
            // Teste de conexão usando uma nova instância apenas para o teste, se desejar,
            // ou confie que a inicialização do 'gerenciador' já testaria implicitamente.
            // Para ser explícito, o teste é bom:
            using (MySqlConnection testConn = new MySqlConnection(connectionString))
            {
                testConn.Open();
                Console.WriteLine("Conexão com o banco de dados MySQL bem-sucedida!");
                // testConn.Close(); // Opcional aqui, pois 'using' já cuida disso
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Falha ao conectar ao banco de dados: {ex.Message}");
            Console.WriteLine("Verifique sua string de conexão, se o banco de dados e tabelas existem, e se o servidor MySQL está acessível.");
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ResetColor();
            Console.ReadKey();
            return; // Encerra a aplicação se não puder conectar
        }
        Console.WriteLine("Pressione qualquer tecla para continuar para o menu...");
        Console.ReadKey();

        bool sair = false;
        while (!sair)
        {
            Console.Clear();
            Console.WriteLine("\n--- Jardim Espaço Gourmet - Gestão de Estoque (MySQL) ---");
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
                case "1": CadastrarNovoItemUI(); break;
                case "2": RegistrarEntradaUI(); break;
                case "3": RegistrarSaidaUI(); break;
                case "4": AjustarInventarioUI(); break;
                case "5": gerenciador.ListarTodosItens(); break;
                case "6": gerenciador.ExibirAlertaEstoqueBaixo(); break;
                case "7": BuscarItemPorNomeUI(); break;
                case "8": VerDetalhesItemUI(); break;
                case "0": sair = true; Console.WriteLine("Saindo do sistema..."); break;
                default: Console.WriteLine("Opção inválida. Tente novamente."); break;
            }
            if (!sair)
            {
                Console.WriteLine("\nPressione qualquer tecla para continuar...");
                Console.ReadKey();
            }
        }
    }

    // Todos os métodos UI devem ser estáticos porque Main é estático e eles usam o campo 'gerenciador' que também é estático.
    static void CadastrarNovoItemUI()
    {
        Console.WriteLine("\n--- Cadastro de Novo Item ---");
        Console.Write("Nome do Item: ");
        string? nome = Console.ReadLine();
        Console.Write("Unidade de Medida (Kg, Litro, Unidade, Pacote): ");
        string? unidade = Console.ReadLine();
        int min;
        while (true)
        {
            Console.Write("Estoque Mínimo Desejado: ");
            if (int.TryParse(Console.ReadLine(), out min) && min >= 0) break;
            Console.WriteLine("Estoque mínimo inválido. Deve ser um número não negativo.");
        }
        int qtdInicial;
        while (true)
        {
            Console.Write("Quantidade Inicial (opcional, default 0): ");
            string? inputQtd = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(inputQtd)) { qtdInicial = 0; break; }
            if (int.TryParse(inputQtd, out qtdInicial) && qtdInicial >= 0) break;
            Console.WriteLine("Quantidade inicial inválida. Deve ser um número não negativo.");
        }
        Console.Write("Categoria (opcional): ");
        string? categoria = Console.ReadLine();
        Console.Write("Fornecedor Padrão (opcional): ");
        string? fornecedor = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(nome) && !string.IsNullOrWhiteSpace(unidade))
        {
            string resultado = gerenciador.CadastrarItem(nome, unidade, min, qtdInicial,
                string.IsNullOrWhiteSpace(categoria) ? null : categoria,
                string.IsNullOrWhiteSpace(fornecedor) ? null : fornecedor);
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
        int id;
        while (true) { Console.Write("ID do Item: "); if (int.TryParse(Console.ReadLine(), out id)) break; Console.WriteLine("ID inválido."); }
        int qtd;
        while (true) { Console.Write("Quantidade Entrada: "); if (int.TryParse(Console.ReadLine(), out qtd) && qtd > 0) break; Console.WriteLine("Quantidade inválida."); }
        Console.WriteLine(gerenciador.RegistrarEntrada(id, qtd));
    }

    static void RegistrarSaidaUI()
    {
        Console.WriteLine("\n--- Registrar Saída de Item ---");
        int id;
        while (true) { Console.Write("ID do Item: "); if (int.TryParse(Console.ReadLine(), out id)) break; Console.WriteLine("ID inválido."); }
        int qtd;
        while (true) { Console.Write("Quantidade Saída: "); if (int.TryParse(Console.ReadLine(), out qtd) && qtd > 0) break; Console.WriteLine("Quantidade inválida."); }
        Console.WriteLine(gerenciador.RegistrarSaida(id, qtd));
    }

    static void AjustarInventarioUI()
    {
        Console.WriteLine("\n--- Ajustar Inventário ---");
        int id;
        while (true) { Console.Write("ID do Item: "); if (int.TryParse(Console.ReadLine(), out id)) break; Console.WriteLine("ID inválido."); }
        int qtd;
        while (true) { Console.Write("Quantidade Atual Real (contagem física): "); if (int.TryParse(Console.ReadLine(), out qtd) && qtd >= 0) break; Console.WriteLine("Quantidade inválida."); }
        Console.WriteLine(gerenciador.AjustarInventario(id, qtd));
    }

    static void BuscarItemPorNomeUI()
    {
        Console.WriteLine("\n--- Buscar Item por Nome ---");
        Console.Write("Digite parte do nome do item: ");
        string? termoBusca = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(termoBusca)) { Console.WriteLine("Termo de busca não pode ser vazio."); return; }
        var itensEncontrados = gerenciador.BuscarItemPorNome(termoBusca);
        if (itensEncontrados.Any())
        {
            Console.WriteLine("Itens encontrados:");
            foreach (var item in itensEncontrados) Console.WriteLine(item.ToString());
        }
        else Console.WriteLine("Nenhum item encontrado com este termo.");
    }

    static void VerDetalhesItemUI()
    {
        Console.WriteLine("\n--- Ver Detalhes de um Item ---");
        int id;
        while (true) { Console.Write("ID do Item para ver detalhes: "); if (int.TryParse(Console.ReadLine(), out id)) break; Console.WriteLine("ID inválido."); }
        gerenciador.ExibirDetalhesItem(id);
    }
}
