using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.API.Controllers;
using MinhasFinancas.API.Controllers;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using Moq;
using Moq;
using Xunit;


namespace UnitTests.Controllers
{
    public class TransacoesControllerTests
    {
        private readonly Mock<ITransacaoService> _serviceMock;
        private readonly TransacoesController _controller;

        public TransacoesControllerTests()
        {
            _serviceMock = new Mock<ITransacaoService>();
            _controller = new TransacoesController(_serviceMock.Object);
        }

        [Fact]
        public async Task Deve_Retornar_NotFound_Quando_Transacao_Nao_Existe()
        {
            _serviceMock
                .Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((TransacaoDto?)null);

            var result = await _controller.GetById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Deve_Retornar_Ok_Quando_Transacao_Existe()
        {
            var dto = new TransacaoDto { Id = Guid.NewGuid() };

            _serviceMock
                .Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(dto);

            var result = await _controller.GetById(dto.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, ok.Value);
        }

        [Fact]
        public async Task Deve_Retornar_BadRequest_Quando_ModelState_Invalido()
        {
            _controller.ModelState.AddModelError("erro", "erro");

            var result = await _controller.Create(new CreateTransacaoDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Deve_Retornar_Created_Quando_Criar_Transacao()
        {
            var dto = new CreateTransacaoDto();

            var retorno = new TransacaoDto
            {
                Id = Guid.NewGuid()
            };

            _serviceMock
                .Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(retorno);

            var result = await _controller.Create(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(retorno, created.Value);
        }

        [Fact]
        public async Task Deve_Retornar_BadRequest_Quando_Service_Lancar_ArgumentException()
        {
            _serviceMock
                .Setup(s => s.CreateAsync(It.IsAny<CreateTransacaoDto>()))
                .ThrowsAsync(new ArgumentException("Erro de negócio"));

            var result = await _controller.Create(new CreateTransacaoDto());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Erro de negócio", badRequest.Value);
        }

        [Fact]
        public async Task Deve_Impedir_Receita_Para_Menor_De_Idade()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var categoria = new Categoria { Id = Guid.NewGuid() };

            var pessoa = new Pessoa
            {
                Id = Guid.NewGuid(),
                DataNascimento = DateTime.Now.AddYears(-15) // menor
            };

            unitOfWorkMock.Setup(u => u.Categorias.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(categoria);

            unitOfWorkMock.Setup(u => u.Pessoas.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(pessoa);

            var service = new TransacaoService(unitOfWorkMock.Object);

            var dto = new CreateTransacaoDto
            {
                Tipo = Transacao.ETipo.Receita,
                CategoriaId = categoria.Id,
                PessoaId = pessoa.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
        }
    }
}
