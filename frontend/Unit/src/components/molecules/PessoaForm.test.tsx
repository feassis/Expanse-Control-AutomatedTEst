import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { PessoaForm } from '@/components/molecules/PessoaForm';
import { useCreatePessoa, useUpdatePessoa } from '@/hooks/usePessoas';
import { useFormStore } from '@/stores/formStore';
import toast from 'react-hot-toast';

// mocks
jest.mock('@/hooks/usePessoas');
jest.mock('@/stores/formStore');
jest.mock('react-hot-toast', () => ({
  success: jest.fn(),
  error: jest.fn(),
}));

describe('PessoaForm', () => {
  const mockResetPessoaForm = jest.fn();
  const mockSetPessoaForm = jest.fn();
  const mockOnSuccess = jest.fn();
  const mockOnCancel = jest.fn();

  const mockCreatePessoa = { mutateAsync: jest.fn(), isPending: false };
  const mockUpdatePessoa = { mutateAsync: jest.fn(), isPending: false };

  beforeEach(() => {
    jest.clearAllMocks();

    // mock do zustand store
    (useFormStore as unknown as jest.Mock).mockReturnValue({
      pessoaForm: { nome: '', dataNascimento: new Date('2000-01-01') },
      resetPessoaForm: mockResetPessoaForm,
      setPessoaForm: mockSetPessoaForm,
    });

    // mocks dos hooks de API
    (useCreatePessoa as unknown as jest.Mock).mockReturnValue(mockCreatePessoa);
    (useUpdatePessoa as unknown as jest.Mock).mockReturnValue(mockUpdatePessoa);
  });

  it('renders form fields and buttons', () => {
    render(<PessoaForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);
    
    expect(screen.getByLabelText('Nome')).toBeInTheDocument();
    expect(screen.getByLabelText('Data de Nascimento')).toBeInTheDocument();
    expect(screen.getByText('Cancelar')).toBeInTheDocument();
    expect(screen.getByText('Salvar')).toBeInTheDocument();
  });

  it('calls onCancel and resets store when Cancel button clicked', async () => {
    render(<PessoaForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);
    await userEvent.click(screen.getByText('Cancelar'));

    expect(mockResetPessoaForm).toHaveBeenCalled();
    expect(mockOnCancel).toHaveBeenCalled();
  });

  it('calls createPessoa and shows success toast when submitting new pessoa', async () => {
    mockCreatePessoa.mutateAsync.mockResolvedValueOnce({});
    render(<PessoaForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);

    const inputNome = screen.getByLabelText('Nome');
    await userEvent.type(inputNome, 'Felipe');

    await userEvent.click(screen.getByText('Salvar'));

    expect(mockCreatePessoa.mutateAsync).toHaveBeenCalledWith(expect.objectContaining({
      nome: 'Felipe',
      dataNascimento: expect.any(Date),
    }));
    expect(toast.success).toHaveBeenCalledWith('Pessoa salva com sucesso!');
    expect(mockResetPessoaForm).toHaveBeenCalled();
    expect(mockOnSuccess).toHaveBeenCalled();
  });

  it('calls updatePessoa and shows success toast when editing pessoa', async () => {
    const pessoa = { id: '1', nome: 'Teste', dataNascimento: new Date('1990-01-01') };
    mockUpdatePessoa.mutateAsync.mockResolvedValueOnce({});
    render(<PessoaForm pessoa={pessoa} onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);

    await userEvent.click(screen.getByText('Salvar'));

    expect(mockUpdatePessoa.mutateAsync).toHaveBeenCalledWith({
      id: '1',
      data: expect.objectContaining({
        nome: 'Teste',
        dataNascimento: expect.any(Date),
      }),
    });
    expect(toast.success).toHaveBeenCalledWith('Pessoa atualizada com sucesso!');
    expect(mockResetPessoaForm).toHaveBeenCalled();
    expect(mockOnSuccess).toHaveBeenCalled();
  });

  it('shows error toast when createPessoa fails', async () => {
    mockCreatePessoa.mutateAsync.mockRejectedValueOnce(new Error('Erro'));
    render(<PessoaForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);

    await userEvent.click(screen.getByText('Salvar'));

    expect(toast.error).toHaveBeenCalledWith('Erro ao salvar pessoa. Tente novamente.');
  });
});