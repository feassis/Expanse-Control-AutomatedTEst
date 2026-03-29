# 🧪 Expense Control Tests

Test suite for financial system (backend + frontend).

## Como Rodar o Test Unitario

### Backend
    Vá para a pasta Tests\backend abra um terminal e rode: dotnet test


---

## Piramide de Testes

### Começamos com os testes Unitários como base da piramide de tests

#### Para o Backend
    Testamos os controlles e Serviços, pois estes são os responsaveis pela logica e regras de negócio da aplicação
    

## 🐞 Bugs Encontrados

---

## 1. ❌ Frontend não consegue se comunicar com o Backend

### 📌 Descrição

Ao tentar criar uma nova pessoa pelo sistema, a requisição falha e nenhuma pessoa é criada.

### 🎯 Comportamento Esperado

O frontend deve enviar uma requisição para o backend e a pessoa deve ser criada com sucesso.

### ❌ Comportamento Atual

A requisição falha com erro de conexão:

```
ERR_CONNECTION_REFUSED
```

### 🔍 Evidência

Requisição realizada para:

```
http://localhost:5000/api/v1.0/pessoas
```

Erro observado no navegador (aba Network):

```
Failed to load resource: net::ERR_CONNECTION_REFUSED
```

### 🧠 Causa Provável

O frontend está configurado para consumir a API na porta `5000`, porém o backend não está disponível nessa porta (porta incorreta ou backend não iniciado).

### 💥 Impacto

* Não é possível criar pessoas
* Funcionalidade principal do sistema fica indisponível

### 🛠️ Sugestão de Correção

* Ajustar a `baseURL` do cliente HTTP no frontend
* Garantir que o backend esteja rodando na porta correta

---

## 2. ⚠️ Configuração de CORS não aplicada corretamente

### 📌 Descrição

Apesar de o backend possuir configuração de CORS permitindo qualquer origem, o frontend não consegue consumir a API.

### 🎯 Comportamento Esperado

O backend deve responder com o header:

```
Access-Control-Allow-Origin
```

permitindo requisições do frontend.

### ❌ Comportamento Atual

O navegador bloqueia a requisição com erro de CORS.

### 🔍 Evidência

Erro no navegador:

```
No 'Access-Control-Allow-Origin' header is present
```

### 🧠 Causa Provável

A ordem dos middlewares no pipeline está incorreta, impedindo que a política de CORS seja aplicada corretamente.

### 💥 Impacto

* Frontend não consegue acessar a API
* Sistema não funciona integrado

### 🛠️ Sugestão de Correção

Reorganizar o pipeline de middlewares, garantindo que `UseCors` seja aplicado antes do mapeamento dos controllers.

---

## 3. 🔒 Redirecionamento HTTPS causa falha de CORS

### 📌 Descrição

O frontend não consegue consumir a API devido a falha de CORS causada por redirecionamento automático de HTTP para HTTPS.

### 🎯 Comportamento Esperado

O frontend deve conseguir realizar requisições ao backend sem bloqueios.

### ❌ Comportamento Atual

As requisições são redirecionadas de HTTP para HTTPS, causando erro de CORS no navegador.

### 🔍 Evidência

Erro no navegador:

```
Access to XMLHttpRequest has been blocked by CORS policy
```

### 🧠 Causa Raiz

O middleware:

```
app.UseHttpsRedirection();
```

redireciona requisições HTTP para HTTPS, alterando a origem da requisição e quebrando a política de CORS.

### 💥 Impacto

* Frontend não consegue consumir a API
* Funcionalidades principais ficam indisponíveis

### 🛠️ Sugestão de Correção

* Alinhar frontend e backend para usar HTTPS
* Ou desabilitar redirecionamento em ambiente de desenvolvimento

---

## 4. 🚨 Regra de negócio violada: menor de idade pode ter receita

### 📌 Descrição

O sistema permite criar receitas para pessoas menores de idade.

### 🎯 Regra Esperada

Menores de idade **não podem possuir receitas**.

### ❌ Resultado Atual

A transação é criada normalmente.

### 💥 Impacto

* Violação de regra de negócio
* Inconsistência nos dados financeiros

### 🛠️ Sugestão de Correção

* Validar idade antes de permitir criação de receita
* Implementar regra no domínio (ex: Service ou Domain Layer)
* Retornar erro de validação apropriado (HTTP 400)

---

## 5. 🧨 NullReference ao deletar pessoa

### 📌 Descrição

O método `DeleteAsync` da `PessoaService` lança `NullReferenceException` caso o repositório não esteja corretamente inicializado.

### 🧠 Causa

Ausência de validação de `null` para `_unitOfWork.Pessoas`.

### 💥 Impacto

* Falha inesperada em tempo de execução
* Possível quebra de fluxo da aplicação

### 🛠️ Sugestão de Correção

* Validar `_unitOfWork.Pessoas` antes do uso
* Garantir injeção correta das dependências
* Adicionar tratamento de erro apropriado (ex: exceção customizada)

---

## ✅ Observações Gerais

* Priorizar correção dos problemas de comunicação (porta, CORS e HTTPS)
* Garantir que regras de negócio estejam centralizadas no backend
* Adicionar testes automatizados para evitar regressões
