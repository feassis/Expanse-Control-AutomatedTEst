using MinhasFinancas.Application.DTOs;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using Xunit;


namespace IntegrationTests
{
    public class IntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public IntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Deve_criar_categoria_com_sucesso()
        {
            var dto = new
            {
                descricao = "Alimentação",
                finalidade = 2 // ajuste conforme enum
            };

            var response = await _client.PostAsJsonAsync("/api/v1/Categorias", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var result = await response.Content.ReadFromJsonAsync<CategoriaDto>();

            result.Should().NotBeNull();
            result.Descricao.Should().Be("Alimentação");
        }

        [Fact]
        public async Task Deve_criar_e_buscar_pessoa()
        {
            var dto = new
            {
                nome = "Felipe",
                dataNascimento = DateTime.Now.AddYears(-25)
            };

            var create = await _client.PostAsJsonAsync("/api/v1/Pessoas", dto);

            create.StatusCode.Should().Be(HttpStatusCode.Created);

            var pessoa = await create.Content.ReadFromJsonAsync<PessoaDto>();

            var get = await _client.GetAsync($"/api/v1/Pessoas/{pessoa.Id}");

            get.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Nao_deve_permitir_receita_para_menor()
        {
            // Pessoa menor
            var pessoa = await _client.PostAsJsonAsync("/api/v1/Pessoas", new
            {
                nome = "Menor",
                dataNascimento = DateTime.Now.AddYears(-10)
            });

            var pessoaObj = await pessoa.Content.ReadFromJsonAsync<PessoaDto>();

            // Categoria Receita
            var categoria = await _client.PostAsJsonAsync("/api/v1/Categorias", new
            {
                descricao = "Salário",
                finalidade = 1
            });

            var categoriaObj = await categoria.Content.ReadFromJsonAsync<CategoriaDto>();

            // Tenta criar transação
            var response = await _client.PostAsJsonAsync("/api/v1/Transacoes", new
            {
                pessoaId = pessoaObj.Id,
                categoriaId = categoriaObj.Id,
                valor = 100
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Nao_deve_permitir_uso_invalido_da_categoria()
        {
            var pessoa = await _client.PostAsJsonAsync("/api/v1/Pessoas", new
            {
                nome = "Adulto",
                dataNascimento = DateTime.Now.AddYears(-30)
            });

            var pessoaObj = await pessoa.Content.ReadFromJsonAsync<PessoaDto>();

            // Categoria só despesa
            var categoria = await _client.PostAsJsonAsync("/api/v1/Categorias", new
            {
                descricao = "Conta",
                finalidade = 2 // despesa
            });

            var categoriaObj = await categoria.Content.ReadFromJsonAsync<CategoriaDto>();

            // tenta usar como receita (se o DTO tiver tipo)
            var response = await _client.PostAsJsonAsync("/api/v1/Transacoes", new
            {
                pessoaId = pessoaObj.Id,
                categoriaId = categoriaObj.Id,
                valor = 100
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact]
        public async Task Deve_remover_transacoes_ao_excluir_pessoa()
        {
            var pessoa = await _client.PostAsJsonAsync("/api/v1/Pessoas", new
            {
                nome = "Teste",
                dataNascimento = DateTime.Now.AddYears(-30)
            });

            var pessoaObj = await pessoa.Content.ReadFromJsonAsync<PessoaDto>();

            var categoria = await _client.PostAsJsonAsync("/api/v1/Categorias", new
            {
                descricao = "Alimentação",
                finalidade = 2
            });

            var categoriaObj = await categoria.Content.ReadFromJsonAsync<CategoriaDto>();

            var transacao = await _client.PostAsJsonAsync("/api/v1/Transacoes", new
            {
                pessoaId = pessoaObj.Id,
                categoriaId = categoriaObj.Id,
                valor = 50
            });

            var transacaoObj = await transacao.Content.ReadFromJsonAsync<TransacaoDto>();

            // remove pessoa
            await _client.DeleteAsync($"/api/v1/Pessoas/{pessoaObj.Id}");

            // tenta buscar transação
            var response = await _client.GetAsync($"/api/v1/Transacoes/{transacaoObj.Id}");

            // 👇 pode falhar (BUG)
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Deve_criar_transacao()
        {
            // cria pessoa
            var pessoa = await _client.PostAsJsonAsync("/api/pessoas", new
            {
                nome = "Carlos",
                idade = 30
            });

            var pessoaObj = await pessoa.Content.ReadFromJsonAsync<dynamic>();

            // cria categoria
            var categoria = await _client.PostAsJsonAsync("/api/categorias", new
            {
                descricao = "Salário"
            });

            var categoriaObj = await categoria.Content.ReadFromJsonAsync<dynamic>();

            // cria transação
            var transacao = await _client.PostAsJsonAsync("/api/transacoes", new
            {
                pessoaId = pessoaObj.id,
                categoriaId = categoriaObj.id,
                valor = 1000,
                tipo = "Receita"
            });

            transacao.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Deve_calcular_total()
        {
            var pessoa = await _client.PostAsJsonAsync("/api/pessoas", new
            {
                nome = "Ana",
                idade = 30
            });

            var pessoaObj = await pessoa.Content.ReadFromJsonAsync<dynamic>();

            var categoria = await _client.PostAsJsonAsync("/api/categorias", new
            {
                descricao = "Geral"
            });

            var categoriaObj = await categoria.Content.ReadFromJsonAsync<dynamic>();

            await _client.PostAsJsonAsync("/api/transacoes", new
            {
                pessoaId = pessoaObj.id,
                categoriaId = categoriaObj.id,
                valor = 100,
                tipo = "Receita"
            });

            await _client.PostAsJsonAsync("/api/transacoes", new
            {
                pessoaId = pessoaObj.id,
                categoriaId = categoriaObj.id,
                valor = 50,
                tipo = "Despesa"
            });

            var response = await _client.GetAsync($"/api/total/{pessoaObj.id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();

            ((decimal)result.total).Should().Be(50);
        }
    }
}
