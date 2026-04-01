describe("Pessoas - loading", () => {
  it("deve exibir loading enquanto carrega", () => {
    cy.intercept("GET", "**/pessoas*", {
      delay: 1000,
      body: { items: [], total: 0, page: 1, pageSize: 8 },
    });

    cy.visit("/");
    cy.contains("Pessoas").click();

    cy.contains("Carregando...").should("be.visible");
  });
});

describe("Pessoas - listagem", () => {
  beforeEach(() => {
    cy.intercept("GET", "**/pessoas*", {
      body: {
        items: [
          {
            id: "1",
            nome: "João",
            dataNascimento: "2000-01-01",
            idade: 24,
          },
        ],
        total: 1,
        page: 1,
        pageSize: 8,
      },
    }).as("getPessoas");

    cy.visit("/");
    cy.contains("Pessoas").click();

    cy.wait("@getPessoas");
  });

  it("deve exibir pessoas na tabela", () => {
    cy.contains("João").should("be.visible");
    cy.contains("24").should("be.visible");
  });
});

describe("Pessoas - modal", () => {
  beforeEach(() => {
    cy.intercept("GET", "**/pessoas*", {
      body: { items: [], total: 0, page: 1, pageSize: 8 },
    });

    cy.visit("/");
    cy.contains("Pessoas").click();
  });

  it("deve abrir modal para adicionar pessoa", () => {
    cy.contains("Adicionar Pessoa").click();

    cy.contains("Adicionar Pessoa").should("be.visible");
  });

  it("deve abrir modal de edição", () => {
    cy.intercept("GET", "**/pessoas*", {
      body: {
        items: [
          { id: "1", nome: "Maria", dataNascimento: "1990-01-01", idade: 34 },
        ],
        total: 1,
        page: 1,
        pageSize: 8,
      },
    }).as("getPessoas");

    cy.visit("/");
    cy.contains("Pessoas").click();
    cy.wait("@getPessoas");

    cy.contains("Maria")
      .parent()
      .contains("Editar")
      .click();

    cy.contains("Editar Pessoa").should("be.visible");
  });
});




