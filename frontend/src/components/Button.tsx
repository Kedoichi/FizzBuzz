import React from "react";

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: "primary" | "secondary" | "success" | "danger";
    size?: "sm" | "md" | "lg";
    isLoading?: boolean;
    loadingText?: string;
    fullWidth?: boolean;
}

export default function Button({
    children,
    variant = "primary",
    size = "md",
    isLoading = false,
    loadingText,
    fullWidth = false,
    className = "",
    disabled,
    ...props
}: ButtonProps) {
    const baseStyles =
        "inline-flex items-center justify-center font-medium rounded-md transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2";

    const variantStyles = {
        primary: "bg-blue-600 hover:bg-blue-700 text-white focus:ring-blue-500",
        secondary:
            "bg-gray-200 hover:bg-gray-300 text-gray-800 focus:ring-gray-500",
        success:
            "bg-green-600 hover:bg-green-700 text-white focus:ring-green-500",
        danger: "bg-red-600 hover:bg-red-700 text-white focus:ring-red-500",
    };

    const sizeStyles = {
        sm: "text-sm py-1 px-3",
        md: "py-2 px-4",
        lg: "text-lg py-3 px-6",
    };

    const widthStyles = fullWidth ? "w-full" : "";
    const disabledStyles =
        disabled || isLoading ? "opacity-70 cursor-not-allowed" : "";

    const buttonStyles = `${baseStyles} ${variantStyles[variant]} ${sizeStyles[size]} ${widthStyles} ${disabledStyles} ${className}`;

    return (
        <button
            disabled={disabled || isLoading}
            className={buttonStyles}
            {...props}
        >
            {isLoading ? (
                <>
                    <svg
                        className="animate-spin -ml-1 mr-2 h-4 w-4"
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                    >
                        <circle
                            className="opacity-25"
                            cx="12"
                            cy="12"
                            r="10"
                            stroke="currentColor"
                            strokeWidth="4"
                        ></circle>
                        <path
                            className="opacity-75"
                            fill="currentColor"
                            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                        ></path>
                    </svg>
                    {loadingText || children}
                </>
            ) : (
                children
            )}
        </button>
    );
}
