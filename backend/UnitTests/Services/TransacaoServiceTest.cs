using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Domain.ValueObjects;
using MinhasFinancas.Application.Specifications;

namespace UnitTests.Services
{
    public class TransacaoServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITransacaoRepository> _transacaoRepoMock;
        private readonly Mock<ICategoriaRepository> _categoriaRepoMock;
        private readonly Mock<IPessoaRepository> _pessoaRepoMock;

        private readonly TransacaoService _service;

        public TransacaoServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _transacaoRepoMock = new Mock<ITransacaoRepository>();
            _categoriaRepoMock = new Mock<ICategoriaRepository>();
            _pessoaRepoMock = new Mock<IPessoaRepository>();

            _unitOfWorkMock.Setup(u => u.Transacoes).Returns(_transacaoRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Categorias).Returns(_categoriaRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Pessoas).Returns(_pessoaRepoMock.Object);

            _service = new TransacaoService(_unitOfWorkMock.Object);
        }

        // ========================
        // GET ALL
        // ========================
        [Fact]
        public async Task GetAllAsync_DeveRetornarLista()
        {
            var result = new PagedResult<TransacaoDto>
            {
                Items = new List<TransacaoDto>
                {
                    new TransacaoDto { Id = Guid.NewGuid(), Descricao = "Teste" }
                }
            };

            _transacaoRepoMock
                .Setup(r => r.GetPagedAsync(
                    It.IsAny<PagedRequest>(),
                    It.IsAny<ISpecification<Transacao, TransacaoDto>>()
                ))
                .ReturnsAsync(result);

            var response = await _service.GetAllAsync();

            response.Should().NotBeNull();
            response.Items.Should().HaveCount(1);
        }

        // ========================
        // GET BY ID
        // ========================
        [Fact]
        public async Task GetByIdAsync_QuandoExiste_DeveRetornarDto()
        {
            var id = Guid.NewGuid();

            var categoria = new Categoria
            {
                Id = Guid.NewGuid(),
                Descricao = "Alimentação",
                Finalidade = Categoria.EFinalidade.Despesa
            };

            var pessoa = new Pessoa
            {
                Id = Guid.NewGuid(),
                Nome = "Felipe",
                DataNascimento = DateTime.Today.AddYears(-25)
            };

            var transacao = new Transacao
            {
                Id = id,
                Descricao = "Teste",
                Valor = 100,
                Tipo = Transacao.ETipo.Despesa,
                Data = DateTime.Now,
            };

            _transacaoRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(transacao);

            var result = await _service.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Descricao.Should().Be("Teste");
            result.CategoriaDescricao.Should().Be("Alimentação");
            result.PessoaNome.Should().Be("Felipe");
        }

        [Fact]
        public async Task GetByIdAsync_QuandoNaoExiste_DeveRetornarNull()
        {
            var id = Guid.NewGuid();

            _transacaoRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Transacao?)null);

            var result = await _service.GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_DeveCriarTransacao_Valida()
        {
            var categoriaId = Guid.NewGuid();
            var pessoaId = Guid.NewGuid();

            var dto = new CreateTransacaoDto
            {
                Descricao = "Compra",
                Valor = 50,
                Tipo = Transacao.ETipo.Despesa,
                Data = DateTime.Now,
                CategoriaId = categoriaId,
                PessoaId = pessoaId
            };

            var categoria = new Categoria
            {
                Id = categoriaId,
                Descricao = "Alimentação",
                Finalidade = Categoria.EFinalidade.Despesa
            };

            var pessoa = new Pessoa
            {
                Id = pessoaId,
                Nome = "Felipe",
                DataNascimento = DateTime.Today.AddYears(-30)
            };

            Transacao? criada = null;

            _categoriaRepoMock.Setup(r => r.GetByIdAsync(categoriaId)).ReturnsAsync(categoria);
            _pessoaRepoMock.Setup(r => r.GetByIdAsync(pessoaId)).ReturnsAsync(pessoa);

            _transacaoRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                .Callback<Transacao>(t => criada = t)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            var result = await _service.CreateAsync(dto);

            criada.Should().NotBeNull();
            criada!.CategoriaId.Should().Be(categoriaId);
            criada.PessoaId.Should().Be(pessoaId);

            result.Descricao.Should().Be(dto.Descricao);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_QuandoCategoriaNaoPermiteTipo_DeveLancarExcecao()
        {
            var dto = new CreateTransacaoDto
            {
                Tipo = Transacao.ETipo.Receita,
                CategoriaId = Guid.NewGuid(),
                PessoaId = Guid.NewGuid()
            };

            var categoria = new Categoria
            {
                Id = dto.CategoriaId,
                Finalidade = Categoria.EFinalidade.Despesa
            };

            var pessoa = new Pessoa
            {
                Id = dto.PessoaId,
                DataNascimento = DateTime.Today.AddYears(-25)
            };

            _categoriaRepoMock.Setup(r => r.GetByIdAsync(dto.CategoriaId)).ReturnsAsync(categoria);
            _pessoaRepoMock.Setup(r => r.GetByIdAsync(dto.PessoaId)).ReturnsAsync(pessoa);

            Func<Task> act = async () => await _service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateAsync_QuandoPessoaMenorReceita_DeveLancarExcecao()
        {
            var dto = new CreateTransacaoDto
            {
                Tipo = Transacao.ETipo.Receita,
                CategoriaId = Guid.NewGuid(),
                PessoaId = Guid.NewGuid()
            };

            var categoria = new Categoria
            {
                Id = dto.CategoriaId,
                Finalidade = Categoria.EFinalidade.Ambas
            };

            var pessoa = new Pessoa
            {
                Id = dto.PessoaId,
                DataNascimento = DateTime.Today.AddYears(-16)
            };

            _categoriaRepoMock.Setup(r => r.GetByIdAsync(dto.CategoriaId)).ReturnsAsync(categoria);
            _pessoaRepoMock.Setup(r => r.GetByIdAsync(dto.PessoaId)).ReturnsAsync(pessoa);

            Func<Task> act = async () => await _service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateAsync_QuandoCategoriaNaoExiste_DeveLancarExcecao()
        {
            var dto = new CreateTransacaoDto
            {
                CategoriaId = Guid.NewGuid(),
                PessoaId = Guid.NewGuid()
            };

            _categoriaRepoMock
                .Setup(r => r.GetByIdAsync(dto.CategoriaId))
                .ReturnsAsync((Categoria?)null);

            Func<Task> act = async () => await _service.CreateAsync(dto);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateAsync_QuandoPessoaNaoExiste_DeveLancarExcecao()
        {
            var categoriaId = Guid.NewGuid();

            var dto = new CreateTransacaoDto
            {
                CategoriaId = categoriaId,
                PessoaId = Guid.NewGuid()
            };

            _categoriaRepoMock
                .Setup(r => r.GetByIdAsync(categoriaId))
                .ReturnsAsync(new Categoria());

            _pessoaRepoMock
                .Setup(r => r.GetByIdAsync(dto.PessoaId))
                .ReturnsAsync((Pessoa?)null);

            Func<Task> act = async () => await _service.CreateAsync(dto);

            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}