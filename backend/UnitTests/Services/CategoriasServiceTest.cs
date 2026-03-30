using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Domain.ValueObjects;

namespace UnitTests.Services
{
    public class CategoriaServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICategoriaRepository> _categoriaRepoMock;
        private readonly CategoriaService _service;

        public CategoriaServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _categoriaRepoMock = new Mock<ICategoriaRepository>();

            _unitOfWorkMock.Setup(u => u.Categorias)
                .Returns(_categoriaRepoMock.Object);

            _service = new CategoriaService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_DeveRetornarCategorias_SemFiltro()
        {
            var result = new PagedResult<CategoriaDto>
            {
                Items = new List<CategoriaDto>
            {
                new CategoriaDto { Id = Guid.NewGuid(), Descricao = "Alimentação" }
            }
            };

            _categoriaRepoMock
                .Setup(r => r.GetPagedAsync(
                    It.IsAny<PagedRequest>(),
                    It.IsAny<MinhasFinancas.Application.Specifications.LambdaSpecification<Categoria, CategoriaDto>>()
                ))
                .ReturnsAsync(result);

            var response = await _service.GetAllAsync();

            response.Should().NotBeNull();
            response.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllAsync_ComSearch_DeveAplicarFiltro()
        {
            var request = new PagedRequest { Search = "Ali" };

            _categoriaRepoMock
            .Setup(r => r.GetPagedAsync(
                request,
                It.IsAny<MinhasFinancas.Application.Specifications.LambdaSpecification<Categoria, CategoriaDto>>()
            ))
            .ReturnsAsync(new PagedResult<CategoriaDto>());

            await _service.GetAllAsync(request);

            _categoriaRepoMock.Verify(r =>
                r.GetPagedAsync(
                    request,
                    It.IsAny<MinhasFinancas.Application.Specifications.LambdaSpecification<Categoria, CategoriaDto>>()
                ),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_QuandoExiste_DeveRetornarDto()
        {
            var id = Guid.NewGuid();

            var categoria = new Categoria
            {
                Id = id,
                Descricao = "Teste",
                Finalidade = Categoria.EFinalidade.Receita
            };

            _categoriaRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(categoria);

            var result = await _service.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Descricao.Should().Be("Teste");
            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdAsync_QuandoNaoExiste_DeveRetornarNull()
        {
            var id = Guid.NewGuid();

            _categoriaRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Categoria?)null);

            var result = await _service.GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_DeveCriarCategoria_EChamarSaveChanges()
        {
            var dto = new CreateCategoriaDto
            {
                Descricao = "Nova Categoria",
                Finalidade = Categoria.EFinalidade.Receita
            };

            Categoria? categoriaCriada = null;

            _categoriaRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Categoria>()))
                .Callback<Categoria>(c => categoriaCriada = c)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(0);

            var result = await _service.CreateAsync(dto);

            categoriaCriada.Should().NotBeNull();
            categoriaCriada!.Descricao.Should().Be(dto.Descricao);

            result.Descricao.Should().Be(dto.Descricao);

            _categoriaRepoMock.Verify(r => r.AddAsync(It.IsAny<Categoria>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ComDtoNull_DeveLancarExcecao()
        {
            Func<Task> act = async () => await _service.CreateAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }
    }
}
