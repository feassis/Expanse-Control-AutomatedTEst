describe("Transações - loading", () => {
  it("deve exibir loading enquanto carrega", () => {
    cy.intercept("GET", "**/transacoes*", {
      delay: 1000,
      body: { items: [], total: 0, page: 1, pageSize: 8 },
    });

    cy.visit("/");
    cy.contains("Transações").click();

    cy.contains("Carregando...").should("be.visible");
  });
});


describe("Transações - modal", () => {
  beforeEach(() => {
    cy.intercept("GET", "**/transacoes*", {
      body: { items: [], total: 0, page: 1, pageSize: 8 },
    });

    cy.visit("/");
    cy.contains("Transações").click();
  });

  it("deve abrir o modal", () => {
    cy.contains("Adicionar Transação").click();

    cy.contains("Adicionar Transação").should("be.visible");
  });

  it("deve fechar ao cancelar", () => {
    cy.contains("Adicionar Transação").click();

    cy.contains("Cancelar").click();

    cy.contains("Adicionar Transação").should("be.visible");
  });
});

describe("Transações - fluxo completo", () => {
  it("deve criar uma transação com sucesso", () => {
    cy.intercept("GET", "**/transacoes*", (req) => {
      if (req.url.includes("pageSize=5")) return; // ignora Home

      req.reply({
        body: { items: [], total: 0, page: 1, pageSize: 8 },
      });
    }).as("getTransacoes");

    cy.intercept("GET", "**/pessoas*", {
      body: {
        items: [
          { id: "1", nome: "João", idade: 25 },
        ],
      },
    }).as("getPessoas");

    cy.intercept("GET", "**/categorias*", {
      body: {
        items: [
          { id: "1", descricao: "Salário", finalidade: "receita" },
        ],
        total: 1,
        page: 1,
        pageSize: 10,
      },
}).as("getCategorias");

    cy.intercept("POST", "**/transacoes", {
      statusCode: 201,
    }).as("createTransacao");

    cy.visit("/");
    cy.contains("Transações").click();

    cy.url().should("include", "/transacoes");
    cy.wait("@getTransacoes");

    cy.contains("Adicionar Transação").click();
    cy.contains("Adicionar Transação").should("be.visible");

    cy.get('input[name="descricao"]').type("Salário Mensal");
    cy.get('input[name="valor"]').type("1000");
    cy.get('input[name="data"]').type("2024-01-01");

    cy.get('select[name="tipo"]').select("receita");

    cy.get('[aria-label="Lista de pessoas"]').click();
    cy.wait("@getPessoas");
    cy.contains("João").click();

    cy.get('[aria-label="Lista de categorias"]').click();
    cy.wait("@getCategorias");
    cy.get('[role="listbox"]').within(() => {
  cy.contains("Salário").click();
});

    cy.contains("Salvar").click();

    cy.wait("@createTransacao");

    cy.get('[role="dialog"]').should("not.exist");
  });
});

it("não deve permitir receita para menor de idade", () => {
  cy.intercept("GET", "**/pessoas*", {
    body: {
      items: [
        { id: "1", nome: "Pedro", idade: 15 },
      ],
    },
  }).as("getPessoas");

  cy.visit("/");
  cy.contains("Transações").click();

  cy.contains("Adicionar Transação").click();
  cy.contains("Adicionar Transação").should("be.visible");

  cy.get('input[name="descricao"]').type("Teste");
  cy.get('input[name="valor"]').type("100");
  cy.get('input[name="data"]').type("2024-01-01");

  cy.get('[aria-label="Lista de pessoas"]').click();
  cy.wait("@getPessoas");
  cy.contains("Pedro").click();

  cy.contains("Menores só podem registrar despesas.").should("be.visible");

  cy.get('select[name="tipo"] option[value="receita"]').should("be.disabled");

  cy.get('select[name="tipo"]').should("have.value", "despesa");
});