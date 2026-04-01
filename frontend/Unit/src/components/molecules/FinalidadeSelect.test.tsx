import { render, screen } from '@testing-library/react';
import { useForm } from 'react-hook-form';
import { FinalidadeSelect } from '@/components/molecules/FinalidadeSelect';
import { Finalidade } from '@/types/domain';

type FormData = {
  finalidade: Finalidade;
};

function TestComponent({ error }: { error?: any }) {
  const { register } = useForm<FormData>();
  return <FinalidadeSelect register={register} name="finalidade" error={error} />;
}

describe('FinalidadeSelect', () => {
  it('should render label and select', () => {
    render(<TestComponent />);

    const label = screen.getByText('Finalidade');
    expect(label).toBeInTheDocument();

    const select = screen.getByLabelText('Finalidade');
    expect(select).toBeInTheDocument();
    expect(select.tagName).toBe('SELECT');
  });

  it('should render all options', () => {
    render(<TestComponent />);

    const options = screen.getAllByRole('option').map(opt => opt.textContent);
    expect(options).toEqual(['Despesa', 'Receita', 'Ambas']);
  });

  it('should display error message', () => {
    render(<TestComponent error={{ message: 'Campo obrigatório' }} />);

    const errorMessage = screen.getByText('Campo obrigatório');
    expect(errorMessage).toBeInTheDocument();
  });

  it('should not display error message when none provided', () => {
    render(<TestComponent />);

    const errorMessage = screen.queryByText(/Campo/);
    expect(errorMessage).not.toBeInTheDocument();
  });
});