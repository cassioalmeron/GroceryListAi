import { useState, type FormEvent, type KeyboardEvent } from 'react';
import './styles.css';
import type { GroceryListSummary } from '@/api';
import { CheckIcon, CloseIcon, EditIcon, PlusIcon, TrashIcon } from '@/components/icons';

type Mode = 'idle' | 'create' | 'rename' | 'delete';

interface ListSelectorProps {
  lists: GroceryListSummary[];
  selectedListId: number;
  onSelect: (listId: number) => void;
  onCreate: (name: string) => Promise<boolean>;
  onRename: (listId: number, name: string) => Promise<boolean>;
  onDelete: (listId: number) => Promise<boolean>;
}

export const ListSelector = ({
  lists,
  selectedListId,
  onSelect,
  onCreate,
  onRename,
  onDelete,
}: ListSelectorProps) => {
  const [mode, setMode] = useState<Mode>('idle');
  const [name, setName] = useState('');
  const [busy, setBusy] = useState(false);

  const selectedList = lists.find(list => list.id === selectedListId);
  const actionsDisabled = busy || mode !== 'idle';

  const open = (nextMode: Mode) => {
    setName(nextMode === 'rename' ? selectedList?.name ?? '' : '');
    setMode(nextMode);
  };

  const close = () => setMode('idle');

  const run = async (action: () => Promise<boolean>) => {
    setBusy(true);
    try {
      if (await action()) close();
    } finally {
      setBusy(false);
    }
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const trimmed = name.trim();
    if (trimmed === '') return;
    run(() => (mode === 'create' ? onCreate(trimmed) : onRename(selectedListId, trimmed)));
  };

  const handleKeyDown = (e: KeyboardEvent) => {
    if (e.key === 'Escape') close();
  };

  return (
    <div className="list-selector">
      <div className="list-selector-row">
        <label htmlFor="list-select" className="list-selector-label">List</label>
        <select
          id="list-select"
          className="list-selector-select"
          value={selectedListId}
          onChange={(e) => onSelect(Number(e.target.value))}
          disabled={actionsDisabled}
        >
          {lists.map(list => (
            <option key={list.id} value={list.id}>{list.name}</option>
          ))}
        </select>
        <button
          type="button"
          className="list-selector-action"
          onClick={() => open('create')}
          disabled={actionsDisabled}
          aria-label="New list"
          title="New list"
        >
          <PlusIcon size={18} />
        </button>
        <button
          type="button"
          className="list-selector-action"
          onClick={() => open('rename')}
          disabled={actionsDisabled}
          aria-label="Rename list"
          title="Rename list"
        >
          <EditIcon size={18} />
        </button>
        <button
          type="button"
          className="list-selector-action list-selector-danger"
          onClick={() => open('delete')}
          disabled={actionsDisabled}
          aria-label="Delete list"
          title="Delete list"
        >
          <TrashIcon size={18} />
        </button>
      </div>

      {(mode === 'create' || mode === 'rename') && (
        <form className="list-selector-form" onSubmit={handleSubmit} onKeyDown={handleKeyDown}>
          <input
            type="text"
            className="list-selector-input"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder={mode === 'create' ? 'New list name...' : 'List name...'}
            aria-label={mode === 'create' ? 'New list name' : 'List name'}
            maxLength={100}
            disabled={busy}
            autoFocus
          />
          <button
            type="submit"
            className="list-selector-action list-selector-save"
            disabled={busy || name.trim() === ''}
            aria-label="Save"
            title="Save"
          >
            <CheckIcon size={18} />
          </button>
          <button
            type="button"
            className="list-selector-action"
            onClick={close}
            disabled={busy}
            aria-label="Cancel"
            title="Cancel"
          >
            <CloseIcon size={18} />
          </button>
        </form>
      )}

      {mode === 'delete' && selectedList && (
        <div
          className="list-selector-confirm"
          role="alertdialog"
          aria-label="Confirm list deletion"
          onKeyDown={handleKeyDown}
        >
          <span>
            Delete list &quot;{selectedList.name}&quot; and all its items?
          </span>
          <div className="list-selector-confirm-buttons">
            <button
              type="button"
              className="list-selector-text-button list-selector-text-danger"
              onClick={() => run(() => onDelete(selectedList.id))}
              disabled={busy}
            >
              Delete
            </button>
            <button
              type="button"
              className="list-selector-text-button"
              onClick={close}
              disabled={busy}
            >
              Cancel
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
