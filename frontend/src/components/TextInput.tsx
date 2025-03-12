import React, { forwardRef } from "react";

interface TextInputProps {
  id?: string;
  value: string | number;
  name?: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  className?: string;
  placeholder?: string;
  autoComplete?: string;
  autoFocus?: boolean;
  required?: boolean;
  type?: string;
  min?: string | number;
}

const TextInput = forwardRef<HTMLInputElement, TextInputProps>(
  (
    {
      id,
      name,
      value,
      onChange,
      className,
      placeholder,
      autoComplete,
      autoFocus,
      required,
      type,
      min,
    },
    ref
  ) => {
    return (
      <input
        id={id}
        name={name}
        value={value}
        onChange={onChange}
        className={className}
        placeholder={placeholder}
        autoComplete={autoComplete}
        autoFocus={autoFocus}
        ref={ref}
        required={required}
        type={type}
        min={min}
      />
    );
  }
);

TextInput.displayName = "TextInput";

export default TextInput;
