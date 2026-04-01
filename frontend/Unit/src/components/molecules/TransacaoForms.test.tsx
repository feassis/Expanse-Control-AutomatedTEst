import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import toast from "react-hot-toast";
import { TransacaoForm } from "@/components/molecules/TransacaoForm";
import { useCreateTransacao } from "@/hooks/useTransacoes";
import { TipoTransacao } from "@/types/domain";

jest.mock("react-hot-toast", () => ({
  success: jest.fn(),
  error: jest.fn(),
}));

jest.mock("@/hooks/useTransacoes", () => ({
  useCreateTransacao: jest.fn(),
}));

// mocks básicos para LazySelect e TipoSelect
jest.mock("./LazyPessoaSelect", () => ({
  LazyPessoaSelect: ({ onChange }: any) => (
    <input
      placeholder="Pessoa"
      onChange={(e) => onChange({ id: e.target.value, idade: 20 })}
    />
  ),
}));

jest.mock("./LazyCategoriaSelect", () => ({
  LazyCategoriaSelect: ({ onChange }: any) => (
    <input
      placeholder="Categoria"
      onChange={(e) => onChange({ id: e.target.value })}
    />
  ),
}));

jest.mock("./TipoSelect", () => ({
  TipoSelect: ({ register }: any) => <select {...register("tipo")}></select>,
}));

jest.mock("./FormField", () => ({
  FormField: ({ label, register }: any) => (
    <input placeholder={label} {...register(label.toLowerCase())} />
  ),
}));

jest.mock("./DateInput", () => ({
  DateInput: ({ register }: any) => <input type="date" {...register("data")} />,
}));

describe("TransacaoForm", () => {
  const mockMutateAsync = jest.fn();
  const mockOnSuccess = jest.fn();
  const mockOnCancel = jest.fn();

  beforeEach(() => {
    (useCreateTransacao as jest.Mock).mockReturnValue({
      mutateAsync: mockMutateAsync,
      isPending: false,
    });
    jest.clearAllMocks();
  });

  it("deve renderizar todos os campos", () => {
    render(<TransacaoForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);
    
    expect(screen.getByPlaceholderText("Descrição")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Valor")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancelar/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /salvar/i })).toBeInTheDocument();
  });

  it("deve submeter a transação com sucesso", async () => {
    mockMutateAsync.mockResolvedValue({});
    render(<TransacaoForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);
    
    await userEvent.type(screen.getByPlaceholderText("Descrição"), "Teste");
    await userEvent.type(screen.getByPlaceholderText("Valor"), "100");
    await userEvent.type(screen.getByPlaceholderText("Pessoa"), "1");
    await userEvent.type(screen.getByPlaceholderText("Categoria"), "1");
    
    await userEvent.click(screen.getByRole("button", { name: /salvar/i }));
    
    expect(mockMutateAsync).toHaveBeenCalled();
    expect(toast.success).toHaveBeenCalledWith("Transação salva com sucesso!");
    expect(mockOnSuccess).toHaveBeenCalled();
  });

  it("deve mostrar erro ao submeter falha", async () => {
    mockMutateAsync.mockRejectedValue(new Error("fail"));
    render(<TransacaoForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);
    
    await userEvent.type(screen.getByPlaceholderText("Descrição"), "Teste");
    await userEvent.type(screen.getByPlaceholderText("Valor"), "100");
    await userEvent.type(screen.getByPlaceholderText("Pessoa"), "1");
    await userEvent.type(screen.getByPlaceholderText("Categoria"), "1");

    await userEvent.click(screen.getByRole("button", { name: /salvar/i }));
    
    expect(toast.error).toHaveBeenCalledWith("Erro ao salvar transação. Tente novamente.");
  });

  it("deve impedir receita para menores de 18 anos", async () => {
    render(<TransacaoForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);
    
    // simula pessoa menor
    await userEvent.type(screen.getByPlaceholderText("Pessoa"), "1");
    // força o tipo como receita
    const tipoSelect = screen.getByRole("combobox");
    await userEvent.selectOptions(tipoSelect, TipoTransacao.Receita);
    
    await userEvent.click(screen.getByRole("button", { name: /salvar/i }));
    
    expect(toast.error).toHaveBeenCalledWith("Menores de 18 anos não podem registrar receitas.");
    expect(mockMutateAsync).not.toHaveBeenCalled();
  });

  it("deve chamar onCancel ao clicar em cancelar", async () => {
    render(<TransacaoForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);
    
    await userEvent.click(screen.getByRole("button", { name: /cancelar/i }));
    expect(mockOnCancel).toHaveBeenCalled();
  });
});