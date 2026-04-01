describe("Smoke Test - Páginas principais", () => {
  it("deve carregar a página de pessoas", () => {
    cy.visit("/pessoas");

    cy.contains("Pessoa").should("be.visible");
  });

  it("deve carregar a página de categorias", () => {
    cy.visit("/categorias");

    cy.contains("Categoria").should("be.visible");
  });

  it("deve carregar a página de transações", () => {
    cy.visit("/transacoes");

    cy.contains("Transação").should("be.visible");
  });

  it("deve carregar a página de totais", () => {
    cy.visit("/totais");

    cy.contains("Total").should("be.visible");
  });
});