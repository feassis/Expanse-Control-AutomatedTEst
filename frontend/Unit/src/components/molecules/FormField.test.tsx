import { render, screen } from "@testing-library/react";
import { useForm } from "react-hook-form";
import { FormField } from "@/components/molecules/FormField";

type FormData = {
  name: string;
  age: number;
  birthdate: Date;
};

function TestComponent({
  error,
  type = "text",
  placeholder,
  step,
}: {
  error?: any;
  type?: string;
  placeholder?: string;
  step?: string;
}) {
  const { register } = useForm<FormData>();
  return (
    <FormField
      label="Campo de Teste"
      name="name"
      type={type}
      placeholder={placeholder}
      step={step}
      error={error}
      register={register}
    />
  );
}

describe("FormField", () => {
  it("should render label and input", () => {
    render(<TestComponent />);
    expect(screen.getByText("Campo de Teste")).toBeInTheDocument();
    const input = screen.getByLabelText("Campo de Teste");
    expect(input).toBeInTheDocument();
    expect(input).toHaveAttribute("type", "text");
  });

  it("should render input with placeholder and step", () => {
    render(<TestComponent placeholder="Digite algo" step="0.01" type="number" />);
    const input = screen.getByLabelText("Campo de Teste");
    expect(input).toHaveAttribute("placeholder", "Digite algo");
    expect(input).toHaveAttribute("step", "0.01");
    expect(input).toHaveAttribute("type", "number");
  });

  it("should render error message and border class", () => {
    render(<TestComponent error={{ message: "Campo obrigatório" }} />);
    expect(screen.getByText("Campo obrigatório")).toBeInTheDocument();
    const input = screen.getByLabelText("Campo de Teste");
    expect(input).toHaveClass("border-red-500");
  });

  it("should handle date type correctly", () => {
    render(<TestComponent type="date" />);
    const input = screen.getByLabelText("Campo de Teste");
    expect(input).toHaveAttribute("type", "date");
  });

  it("should handle number type correctly", () => {
    render(<TestComponent type="number" step="1" />);
    const input = screen.getByLabelText("Campo de Teste");
    expect(input).toHaveAttribute("type", "number");
    expect(input).toHaveAttribute("step", "1");
  });
});