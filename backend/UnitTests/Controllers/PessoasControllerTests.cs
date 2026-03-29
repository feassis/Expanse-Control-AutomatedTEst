using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.API.Controllers;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using Moq;
using Xunit;

namespace UnitTests.Controllers
{
    public class PessoasControllerTests
    {
        private readonly Mock<IPessoaService> _serviceMock;
        private readonly PessoasController _controller;

        public PessoasControllerTests()
        {
            _serviceMock = new Mock<IPessoaService>();
            _controller = new PessoasController(_serviceMock.Object);
        }

        [Fact]
        public async Task Deve_Retornar_NotFound_Quando_Pessoa_Nao_Existe()
        {
            _serviceMock
                .Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((PessoaDto?)null);

            var result = await _controller.GetById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Deve_Retornar_Ok_Quando_Pessoa_Existe()
        {
            var pessoa = new PessoaDto { Id = Guid.NewGuid(), Nome = "Felipe" };

            _serviceMock
                .Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(pessoa);

            var result = await _controller.GetById(pessoa.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pessoa, ok.Value);
        }

        [Fact]
        public async Task Deve_Retornar_BadRequest_Quando_ModelState_Invalido()
        {
            _controller.ModelState.AddModelError("erro", "erro");

            var result = await _controller.Create(new CreatePessoaDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Deve_Retornar_Created_Quando_Criar_Pessoa()
        {
            var dto = new CreatePessoaDto { Nome = "Maria" };

            var retorno = new PessoaDto { Id = Guid.NewGuid(), Nome = "Maria" };

            _serviceMock
                .Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(retorno);

            var result = await _controller.Create(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(retorno, created.Value);
        }

        [Fact]
        public async Task Deve_Retornar_NotFound_Quando_Update_De_Pessoa_Inexistente()
        {
            _serviceMock
                .Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdatePessoaDto>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.Update(Guid.NewGuid(), new UpdatePessoaDto());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Deve_Retornar_NoContent_Quando_Update_Sucesso()
        {
            var result = await _controller.Update(Guid.NewGuid(), new UpdatePessoaDto());

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Deve_Retornar_NotFound_Quando_Delete_De_Pessoa_Inexistente()
        {
            _serviceMock
                .Setup(s => s.DeleteAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Deve_Retornar_NoContent_Quando_Delete_Sucesso()
        {
            var result = await _controller.Delete(Guid.NewGuid());

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_Nao_Lanca_Excecao_Quando_Pessoa_Nao_Existe()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var service = new PessoaService(unitOfWorkMock.Object);

            await service.DeleteAsync(Guid.NewGuid());
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Atualizar_Pessoa_Inexistente()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            unitOfWorkMock
                .Setup(u => u.Pessoas.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Pessoa?)null);

            var service = new PessoaService(unitOfWorkMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.UpdateAsync(Guid.NewGuid(), new UpdatePessoaDto()));
        }
    }

}
