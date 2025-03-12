import { CreateGameRule } from '../types';
import TextInput from './TextInput';

interface RuleFormRowProps {
  index: number;
  rule: CreateGameRule;
  onRuleChange: (index: number, field: keyof CreateGameRule, value: string | number) => void;
  onRemove: () => void;
  canRemove: boolean;
}

export default function RuleFormRow({
  index,
  rule,
  onRuleChange,
  onRemove,
  canRemove
}: RuleFormRowProps) {
  return (
    <div className="bg-gray-50 p-4 rounded-md">
      <div className="flex justify-between items-center mb-3">
        <h3 className="font-medium text-gray-700">Rule #{index + 1}</h3>
        {canRemove && (
          <button
            type="button"
            onClick={onRemove}
            className="text-red-500 hover:text-red-700 text-sm font-medium"
          >
            Remove
          </button>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label className="block text-gray-700 text-sm font-medium mb-1">
            Divisor
          </label>
          <TextInput
            type="number"
            value={rule.divisor}
            onChange={(e) => onRuleChange(index, 'divisor', e.target.value)}
            min={1}
            required
            placeholder="e.g., 3"
          />
        </div>
        <div>
          <label className="block text-gray-700 text-sm font-medium mb-1">
            Replace Word
          </label>
          <TextInput
            type="text"
            value={rule.replaceWord}
            onChange={(e) => onRuleChange(index, 'replaceWord', e.target.value)}
            required
            placeholder="e.g., Fizz"
          />
        </div>
        <div>
          <label className="block text-gray-700 text-sm font-medium mb-1">
            Sort Order (Optional)
          </label>
          <TextInput
            type="number"
            value={rule.sortOrder}
            onChange={(e) => onRuleChange(index, 'sortOrder', e.target.value)}
            min={0}
            placeholder="e.g., 0"
          />
          <p className="text-xs text-gray-500 mt-1">
            Determines the order of words when multiple rules apply
          </p>
        </div>
      </div>
    </div>
  );
}