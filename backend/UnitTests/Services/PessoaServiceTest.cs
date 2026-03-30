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
    public class PessoaServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPessoaRepository> _pessoaRepoMock;
        private readonly PessoaService _service;

        public PessoaServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _pessoaRepoMock = new Mock<IPessoaRepository>();

            _unitOfWorkMock.Setup(u => u.Pessoas)
                .Returns(_pessoaRepoMock.Object);

            _service = new PessoaService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_DeveRetornarLista()
        {
            var result = new PagedResult<PessoaDto>
            {
                Items = new List<PessoaDto>
            {
                new PessoaDto { Id = Guid.NewGuid(), Nome = "Felipe" }
            }
            };

            _pessoaRepoMock
                .Setup(r => r.GetPagedAsync(
                    It.IsAny<PagedRequest>(),
                    It.IsAny<ISpecification<Pessoa, PessoaDto>>()
                ))
                .ReturnsAsync(result);

            var response = await _service.GetAllAsync();

            response.Should().NotBeNull();
            response.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllAsync_ComSearch_DeveChamarRepositorio()
        {
            var request = new PagedRequest { Search = "Fel" };

            _pessoaRepoMock
                .Setup(r => r.GetPagedAsync(
                    It.IsAny<PagedRequest>(),
                    It.IsAny<ISpecification<Pessoa, PessoaDto>>()
                ))
                .ReturnsAsync(new PagedResult<PessoaDto>());

            await _service.GetAllAsync(request);

            _pessoaRepoMock.Verify(r =>
                r.GetPagedAsync(
                    It.Is<PagedRequest>(p => p.Search == "Fel"),
                    It.IsAny<ISpecification<Pessoa, PessoaDto>>()
                ),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_QuandoExiste_DeveRetornarDto()
        {
            var id = Guid.NewGuid();

            var pessoa = new Pessoa
            {
                Id = id,
                Nome = "Felipe",
                DataNascimento = DateTime.Now
            };

            _pessoaRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(pessoa);

            var result = await _service.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Nome.Should().Be("Felipe");
        }

        [Fact]
        public async Task GetByIdAsync_QuandoNaoExiste_DeveRetornarNull()
        {
            var id = Guid.NewGuid();

            _pessoaRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Pessoa?)null);

            var result = await _service.GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_DeveCriarPessoa()
        {
            var dto = new CreatePessoaDto
            {
                Nome = "Felipe",
                DataNascimento = new DateTime(2000, 1, 1)
            };

            Pessoa? pessoaCriada = null;

            _pessoaRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Pessoa>()))
                .Callback<Pessoa>(p => pessoaCriada = p)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1); 

            var result = await _service.CreateAsync(dto);

            pessoaCriada.Should().NotBeNull();
            pessoaCriada!.Nome.Should().Be(dto.Nome);

            result.Nome.Should().Be(dto.Nome);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ComDtoNull_DeveLancarExcecao()
        {
            Func<Task> act = async () => await _service.CreateAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateAsync_DeveAtualizarPessoa()
        {
            var id = Guid.NewGuid();

            var pessoa = new Pessoa
            {
                Id = id,
                Nome = "Antigo",
                DataNascimento = new DateTime(1990, 1, 1)
            };

            var dto = new UpdatePessoaDto
            {
                Nome = "Novo",
                DataNascimento = new DateTime(2000, 1, 1)
            };

            _pessoaRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(pessoa);

            _pessoaRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Pessoa>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            await _service.UpdateAsync(id, dto);

            pessoa.Nome.Should().Be("Novo");

            _pessoaRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Pessoa>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_QuandoNaoExiste_DeveLancarExcecao()
        {
            var id = Guid.NewGuid();

            _pessoaRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Pessoa?)null);

            var dto = new UpdatePessoaDto();

            Func<Task> act = async () => await _service.UpdateAsync(id, dto);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task DeleteAsync_DeveRemoverPessoa()
        {
            var id = Guid.NewGuid();

            _pessoaRepoMock
                .Setup(r => r.DeleteAsync(id))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            await _service.DeleteAsync(id);

            _pessoaRepoMock.Verify(r => r.DeleteAsync(id), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
