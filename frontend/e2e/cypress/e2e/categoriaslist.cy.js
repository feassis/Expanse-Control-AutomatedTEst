describe("Categorias - loading", () => {
  it("deve exibir loading enquanto carrega", () => {
    cy.intercept("GET", "**/categorias*", {
      delay: 1000,
      body: { items: [], total: 0, page: 1, pageSize: 8 },
    });

    cy.visit("/");
    cy.contains("Categorias").click();

    cy.contains("Carregando...").should("be.visible");
  });
});

describe("Categorias - listagem", () => {
  beforeEach(() => {
    cy.intercept("GET", "**/categorias*", {
      body: {
        items: [
          { descricao: "Alimentação", finalidade: "despesa" },
          { descricao: "Salário", finalidade: "receita" },
        ],
        total: 2,
        page: 1,
        pageSize: 8,
      },
    }).as("getCategorias");

    cy.visit("/");
    cy.contains("Categorias").click();

    cy.wait("@getCategorias");
  });

  it("deve exibir categorias na tabela", () => {
    cy.contains("Alimentação").should("be.visible");
    cy.contains("Salário").should("be.visible");
  });
});



describe("Categorias - modal", () => {
  beforeEach(() => {
    cy.intercept("GET", "**/categorias*", {
      body: { items: [], total: 0, page: 1, pageSize: 8 },
    });

    cy.visit("/");
    cy.contains("Categorias").click();
  });

  it("deve abrir o modal ao clicar em adicionar", () => {
    cy.contains("Adicionar Categoria").click();

    cy.contains("Adicionar Categoria").should("be.visible"); // título do dialog
  });

  it("deve fechar o modal ao cancelar", () => {
    cy.contains("Adicionar Categoria").click();

    cy.contains("Cancelar").click();

    cy.contains("Adicionar Categoria").should("be.visible"); // botão da página
  });
});