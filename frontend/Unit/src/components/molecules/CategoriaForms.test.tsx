import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CategoriaForm } from '@/components/molecules/CategoriaForm';
import { useCreateCategoria } from '@/hooks/useCategorias';
import toast from 'react-hot-toast';

jest.mock('@/lib/api', () => ({
  api: {
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    delete: jest.fn(),
  },
}));

// mocks
jest.mock('@/hooks/useCategorias');
jest.mock('react-hot-toast');

describe('CategoriaForm', () => {
  const mockMutate = jest.fn();
  const mockOnSuccess = jest.fn();
  const mockOnCancel = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();

    (useCreateCategoria as jest.Mock).mockReturnValue({
      mutateAsync: mockMutate,
      isPending: false,
    });
  });

  it('should render form fields and buttons', () => {
    render(<CategoriaForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);

    expect(screen.getByText('Descrição')).toBeInTheDocument();
    expect(screen.getByText('Salvar')).toBeInTheDocument();
    expect(screen.getByText('Cancelar')).toBeInTheDocument();
  });

  it('should call onCancel when cancel button is clicked', async () => {
    render(<CategoriaForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);

    await userEvent.click(screen.getByText('Cancelar'));

    expect(mockOnCancel).toHaveBeenCalled();
  });

  it('should submit form successfully', async () => {
    mockMutate.mockResolvedValueOnce({});

    render(<CategoriaForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);

    await userEvent.type(screen.getByPlaceholderText('Digite a descrição'), 'Teste');

    await userEvent.click(screen.getByText('Salvar'));

    expect(mockMutate).toHaveBeenCalled();
    expect(toast.success).toHaveBeenCalledWith('Categoria salva com sucesso!');
    expect(mockOnSuccess).toHaveBeenCalled();
  });

  it('should show error toast when submit fails', async () => {
    mockMutate.mockRejectedValueOnce(new Error('Erro'));

    render(<CategoriaForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);

    await userEvent.type(screen.getByPlaceholderText('Digite a descrição'), 'Teste');

    await userEvent.click(screen.getByText('Salvar'));

    expect(toast.error).toHaveBeenCalledWith(
      'Erro ao salvar categoria. Tente novamente.'
    );
  });

  it('should show loading state', () => {
    (useCreateCategoria as jest.Mock).mockReturnValue({
      mutateAsync: jest.fn(),
      isPending: true,
    });

    render(<CategoriaForm onSuccess={mockOnSuccess} onCancel={mockOnCancel} />);

    expect(screen.getByText('Salvando...')).toBeInTheDocument();
  });
});