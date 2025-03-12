'use client'; // Ensures that this component is client-side only

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { CreateGame, CreateGameRule } from '../../types';
import { gameService } from '../../services/apiService';
import RuleFormRow from '../../components/RuleFormRow';
import Button from '../../components/Button';
import TextInput from '../../components/TextInput';

export default function CreateGamePage() {
  const router = useRouter();
  const [formData, setFormData] = useState<CreateGame>({
    name: '',
    author: '',
    rules: [{ divisor: 3, replaceWord: 'Fizz', sortOrder: 0 }]
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleRuleChange = (index: number, field: keyof CreateGameRule, value: string | number) => {
    const updatedRules = [...formData.rules];
    updatedRules[index] = {
      ...updatedRules[index],
      [field]: typeof value === 'string' && field !== 'replaceWord' ? parseInt(value) : value
    };
    setFormData({ ...formData, rules: updatedRules });
  };

  const addRule = () => {
    setFormData({
      ...formData,
      rules: [...formData.rules, { divisor: 0, replaceWord: '', sortOrder: formData.rules.length }]
    });
  };

  const removeRule = (index: number) => {
    if (formData.rules.length > 1) {
      const updatedRules = formData.rules.filter((_, i) => i !== index);
      setFormData({ ...formData, rules: updatedRules });
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      // Validate form data
      if (!formData.name.trim()) {
        throw new Error('Game name is required');
      }
      if (!formData.author.trim()) {
        throw new Error('Author name is required');
      }
      
      // Validate rules
      for (const rule of formData.rules) {
        if (rule.divisor <= 0) {
          throw new Error('Divisors must be positive numbers');
        }
        if (!rule.replaceWord.trim()) {
          throw new Error('Replace words cannot be empty');
        }
      }

      // Check for duplicate divisors
      const divisors = formData.rules.map(r => r.divisor);
      if (new Set(divisors).size !== divisors.length) {
        throw new Error('Duplicate divisors are not allowed');
      }

      await gameService.createGame(formData);
      router.push('/');
    } catch (err) {
      console.error('Error creating game:', err);
      setError(err instanceof Error ? err.message : 'Failed to create game');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Create New Game</h1>
        <p className="text-gray-600 mt-1">Create your own FizzBuzz-style game with custom rules</p>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 p-4 rounded-md mb-6">
          <p className="font-medium">Error</p>
          <p>{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-sm p-6">
        <div className="space-y-6">
          <div>
            <label htmlFor="name" className="block text-gray-700 font-medium mb-2">
              Game Name
            </label>
            <TextInput
              id="name"
              name="name"
              value={formData.name}
              onChange={handleInputChange}
              placeholder="e.g., FizzBuzz, FooBooLoo"
              required
            />
          </div>

          <div>
            <label htmlFor="author" className="block text-gray-700 font-medium mb-2">
              Author
            </label>
            <TextInput
              id="author"
              name="author"
              value={formData.author}
              onChange={handleInputChange}
              placeholder="Your name"
              required
            />
          </div>

          <div>
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-lg font-semibold text-gray-900">Game Rules</h2>
              <Button 
                type="button" 
                variant="secondary" 
                onClick={addRule}
                size="sm"
              >
                Add Rule
              </Button>
            </div>

            <div className="space-y-4">
              {formData.rules.map((rule, index) => (
                <RuleFormRow
                  key={index}
                  index={index}
                  rule={rule}
                  onRuleChange={handleRuleChange}
                  onRemove={() => removeRule(index)}
                  canRemove={formData.rules.length > 1}
                />
              ))}
            </div>
          </div>
        </div>

        <div className="mt-8 flex justify-end space-x-4">
          <Button
            type="button"
            variant="secondary"
            onClick={() => router.push('/')}
            disabled={loading}
          >
            Cancel
          </Button>
          <Button
            type="submit"
            variant="primary"
            isLoading={loading}
            loadingText="Creating..."
          >
            Create Game
          </Button>
        </div>
      </form>
    </div>
  );
}
