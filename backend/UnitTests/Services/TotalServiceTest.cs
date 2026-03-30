using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Domain.ValueObjects;

namespace UnitTests.Services
{
    public class TotalServiceTests
    {
        private readonly Mock<ITotaisQuery> _totaisQueryMock;
        private readonly TotalService _service;

        public TotalServiceTests()
        {
            _totaisQueryMock = new Mock<ITotaisQuery>();
            _service = new TotalService(_totaisQueryMock.Object);
        }

        [Fact]
        public async Task GetTotaisPorPessoaAsync_DeveRetornarResultado()
        {
            var expected = new PagedResult<TotalPorPessoa>();

            _totaisQueryMock
                .Setup(q => q.GetTotaisPorPessoaAsync(
                    It.IsAny<TotaisPorPessoaFilter>(),
                    It.IsAny<PagedRequest>()
                ))
                .ReturnsAsync(expected);

            var result = await _service.GetTotaisPorPessoaAsync();

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetTotaisPorPessoaAsync_DeveChamarQueryComParametros()
        {
            var filter = new TotaisPorPessoaFilter();
            var request = new PagedRequest();

            _totaisQueryMock
                .Setup(q => q.GetTotaisPorPessoaAsync(filter, request))
                .ReturnsAsync(new PagedResult<TotalPorPessoa>());

            await _service.GetTotaisPorPessoaAsync(filter, request);

            _totaisQueryMock.Verify(q =>
                q.GetTotaisPorPessoaAsync(filter, request),
                Times.Once);
        }

        [Fact]
        public async Task GetTotaisPorCategoriaAsync_DeveRetornarResultado()
        {
            var expected = new PagedResult<TotalPorCategoria>();

            _totaisQueryMock
                .Setup(q => q.GetTotaisPorCategoriaAsync(
                    It.IsAny<TotaisPorCategoriaFilter>(),
                    It.IsAny<PagedRequest>()
                ))
                .ReturnsAsync(expected);

            var result = await _service.GetTotaisPorCategoriaAsync();

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetTotaisPorCategoriaAsync_DeveChamarQueryComParametros()
        {
            var filter = new TotaisPorCategoriaFilter();
            var request = new PagedRequest();

            _totaisQueryMock
                .Setup(q => q.GetTotaisPorCategoriaAsync(filter, request))
                .ReturnsAsync(new PagedResult<TotalPorCategoria>());

            await _service.GetTotaisPorCategoriaAsync(filter, request);

            _totaisQueryMock.Verify(q =>
                q.GetTotaisPorCategoriaAsync(filter, request),
                Times.Once);
        }
    }
}
