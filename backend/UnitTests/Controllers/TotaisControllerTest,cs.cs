using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.API.Controllers;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Domain.ValueObjects;
using Moq;
using Xunit;

namespace UnitTests.Controllers
{
    public class TotaisControllerTests
    {
        private readonly Mock<ITotalService> _serviceMock;
        private readonly TotaisController _controller;

        public TotaisControllerTests()
        {
            _serviceMock = new Mock<ITotalService>();
            _controller = new TotaisController(_serviceMock.Object);
        }

        [Fact]
        public async Task Deve_Retornar_Ok_Em_Totais_Por_Pessoa()
        {
            var resultMock = new PagedResult<TotalPorPessoa>();

            _serviceMock
                .Setup(s => s.GetTotaisPorPessoaAsync(
                    It.IsAny<TotaisPorPessoaFilter>(),
                    It.IsAny<PagedRequest>()))
                .ReturnsAsync(resultMock);

            var result = await _controller.GetTotaisPorPessoa();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resultMock, ok.Value);
        }

        [Fact]
        public async Task Deve_Retornar_Ok_Em_Totais_Por_Categoria()
        {
            var resultMock = new PagedResult<TotalPorCategoria>();

            _serviceMock
                .Setup(s => s.GetTotaisPorCategoriaAsync(
                    It.IsAny<TotaisPorCategoriaFilter>(),
                    It.IsAny<PagedRequest>()))
                .ReturnsAsync(resultMock);

            var result = await _controller.GetTotaisPorCategoria();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resultMock, ok.Value);
        }

        [Fact]
        public async Task Deve_Chamar_Query_Para_Totais_Por_Pessoa()
        {
            var queryMock = new Mock<ITotaisQuery>();

            var resultMock = new PagedResult<TotalPorPessoa>();

            queryMock
                .Setup(q => q.GetTotaisPorPessoaAsync(
                    It.IsAny<TotaisPorPessoaFilter>(),
                    It.IsAny<PagedRequest>()))
                .ReturnsAsync(resultMock);

            var service = new TotalService(queryMock.Object);

            var result = await service.GetTotaisPorPessoaAsync();

            Assert.Equal(resultMock, result);
        }
    }
}
