describe("Home - estrutura", () => {
  beforeEach(() => {
    cy.visit("/");
  });

  it("deve renderizar o header", () => {
    cy.contains("Home").should("be.visible"); 
  });

  it("deve exibir os cards de estatísticas", () => {
    cy.contains("Saldo").should("be.visible");
    cy.contains("Receitas").should("be.visible");
    cy.contains("Despesas").should("be.visible");
  });

  it("deve exibir lista de transações", () => {
    cy.contains("Transações").should("be.visible");
  });

  it("deve exibir resumo mensal", () => {
    cy.contains("Resumo").should("be.visible");
  });
});


describe("Home - dados", () => {
  beforeEach(() => {
    cy.intercept("GET", "**/totais/categorias*", {
      body: [
        {
          descricao: "Salário",
          totalReceitas: 1000,
          totalDespesas: 0,
        },
        {
          descricao: "Aluguel",
          totalReceitas: 0,
          totalDespesas: 400,
        },
      ],
    });

    cy.visit("/");
  });

  it("deve calcular saldo corretamente", () => {
    cy.contains("600").should("be.visible");
  });
});