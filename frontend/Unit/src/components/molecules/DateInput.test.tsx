import { render, screen } from '@testing-library/react';
import { useForm } from 'react-hook-form';
import { DateInput } from '@/components/molecules/DateInput';

type FormData = {
  data: Date;
};

// Wrapper reutilizável
function TestComponent({
  error,
  label,
}: {
  error?: any;
  label?: string;
}) {
  const { register } = useForm<FormData>();

  return (
    <DateInput
      register={register}
      name="data"
      error={error}
      label={label}
    />
  );
}

describe('DateInput', () => {
  it('should render label and input', () => {
    render(<TestComponent label="Data de teste" />);

    expect(screen.getByText('Data de teste')).toBeInTheDocument();

    const input = screen.getByLabelText('Data de teste');
    expect(input).toBeInTheDocument();
    expect(input).toHaveAttribute('type', 'date');
  });

  it('should render default label when not provided', () => {
    render(<TestComponent />);

    expect(screen.getByText('Data')).toBeInTheDocument();
  });

  it('should display error message', () => {
    render(
      <TestComponent error={{ message: 'Campo obrigatório' }} />
    );

    expect(screen.getByText('Campo obrigatório')).toBeInTheDocument();
  });

  it('should not display error when none provided', () => {
    render(<TestComponent />);

    expect(screen.queryByText(/Campo/)).not.toBeInTheDocument();
  });

  it('should bind input name correctly', () => {
    render(<TestComponent label="Data de teste" />);

    const input = screen.getByLabelText('Data de teste');
    expect(input).toHaveAttribute('name', 'data');
  });

  it('should set aria-invalid when error exists', () => {
    render(
      <TestComponent
        label="Data de teste"
        error={{ message: 'Erro' }}
      />
    );

    const input = screen.getByLabelText('Data de teste');
    expect(input).toHaveAttribute('aria-invalid', 'true');
  });
});