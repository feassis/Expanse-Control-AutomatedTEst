using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MinhasFinancas.API.Controllers;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.ValueObjects;
using MinhasFinancas.Domain.Entities;

namespace UnitTests.Controllers
{
    public class CategoriasControllerTests
    {
        private readonly Mock<ICategoriaService> _mockService;
        private readonly CategoriasController _controller;

        public CategoriasControllerTests()
        {
            _mockService = new Mock<ICategoriaService>();
            _controller = new CategoriasController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithPagedCategories()
        {
            var pagedResult = new PagedResult<CategoriaDto>
            {
                Items = new List<CategoriaDto>
                {
                    new CategoriaDto
                    {
                        Id = Guid.NewGuid(),
                        Descricao = "Alimentação",
                        Finalidade = Categoria.EFinalidade.Despesa
                    }
                }
            };
            _mockService.Setup(s => s.GetAllAsync(It.IsAny<PagedRequest>())).ReturnsAsync(pagedResult);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PagedResult<CategoriaDto>>(okResult.Value);
            Assert.Single(value.Items);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenCategoriaExists()
        {
            var categoriaId = Guid.NewGuid();
            var categoriaDto = new CategoriaDto { Id = categoriaId, Descricao = "Transporte", Finalidade = Categoria.EFinalidade.Despesa };
            _mockService.Setup(s => s.GetByIdAsync(categoriaId)).ReturnsAsync(categoriaDto);

            var result = await _controller.GetById(categoriaId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<CategoriaDto>(okResult.Value);
            Assert.Equal("Transporte", value.Descricao);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenCategoriaDoesNotExist()
        {
            _mockService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CategoriaDto?)null);

            var result = await _controller.GetById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenDtoIsValid()
        {
            var dto = new CreateCategoriaDto { Descricao = "Lazer", Finalidade = Categoria.EFinalidade.Despesa };
            var createdDto = new CategoriaDto { Id = Guid.NewGuid(), Descricao = "Lazer", Finalidade = Categoria.EFinalidade.Despesa };
            _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(createdDto);

            var result = await _controller.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var value = Assert.IsType<CategoriaDto>(createdResult.Value);
            Assert.Equal("Lazer", value.Descricao);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Descricao", "Obrigatório");
            var dto = new CreateCategoriaDto();

            var result = await _controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
