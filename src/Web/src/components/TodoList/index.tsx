import React, { useState, useEffect, useCallback, forwardRef, useImperativeHandle } from 'react';
import './styles.css';
import { getGroceryList, addItem, deleteItem, markItemAsChecked, chat } from '@/api';
import { toast } from 'react-toastify';
import { VoiceButton } from '@/components/VoiceButton';
import { TrashIcon } from '@/components/icons';

interface Todo {
  id: number
  description: string
  checked: boolean
}

export interface TodoListRef {
  refreshTodos: () => void
}

interface TodoListProps {
  listId: number
}

export const TodoList = forwardRef<TodoListRef, TodoListProps>(({ listId }, ref) => {
  const [todos, setTodos] = useState<Todo[]>([])
  const [inputValue, setInputValue] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // isCurrent lets a caller drop the response once the user has switched to another list
  const loadTodos = useCallback(async (isCurrent: () => boolean = () => true) => {
    try {
      setLoading(true)
      setError(null)
      const data = await getGroceryList(listId)
      if (!isCurrent()) return
      // Map API response to our Todo interface
      const mappedTodos = data.map((item: { id: string; description: string; checked?: boolean }) => ({
        id: item.id,
        description: item.description,
        checked: item.checked || false
      }))
      setTodos(mappedTodos)
    } catch (err) {
      if (!isCurrent()) return
      setError('Failed to load todos')
      console.error('Error loading todos:', err)
      toast.error('Failed to load todos. Please try again.')
    } finally {
      if (isCurrent()) setLoading(false)
    }
  }, [listId])

  // Load todos from API on mount and whenever the selected list changes
  useEffect(() => {
    let isActive = true
    setTodos([])
    loadTodos(() => isActive)
    return () => {
      isActive = false
    }
  }, [loadTodos])

  // Expose refreshTodos method to parent component
  useImperativeHandle(ref, () => ({
    refreshTodos: () => loadTodos()
  }))

  const addTodo = async () => {
    if (inputValue.trim() === '') return

    try {
      setLoading(true)
      setError(null)
      const description = inputValue.trim()
      const newItem = await addItem(description, listId)
      // Add the new item to our local state
      const newTodo: Todo = {
        id: newItem.id,
        description: description,
        checked: false
      }
      setTodos([...todos, newTodo])
      setInputValue('')
      toast.success('Item added successfully!')
    } catch (err) {
      setError('Failed to add todo')
      console.error('Error adding todo:', err)
      toast.error('Failed to add item. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  const removeTodo = async (id: number) => {
    try {
      setLoading(true)
      setError(null)
      await deleteItem(id)
      // Remove the item from our local state
      setTodos(todos.filter(todo => todo.id !== id))
      toast.success('Item deleted successfully!')
    } catch (err) {
      setError('Failed to delete todo')
      console.error('Error deleting todo:', err)
      toast.error('Failed to delete item. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  const toggleTodo = async (id: number) => {

    const todo = todos.find(t => t.id === id)
    if (!todo) return

    const newCheckedState = !todo.checked

    try {
      setLoading(true)
      setError(null)
      await markItemAsChecked(id, newCheckedState)
      // Update local state after successful API call
      setTodos(todos.map(todo =>
        todo.id === id ? { ...todo, checked: newCheckedState } : todo
      ))
      toast.success(newCheckedState ? 'Item marked as completed!' : 'Item marked as incomplete!')
    } catch (err) {
      setError('Failed to update todo status')
      console.error('Error updating todo:', err)
      toast.error('Failed to update item status. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      addTodo()
    }
  }

  const handleVoiceTranscript = async (text: string) => {
    try {
      setLoading(true)
      await chat(text, listId, () => {})
      await loadTodos()
    } catch (err) {
      console.error('Voice command error:', err)
      toast.error('Voice command failed. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  const retryLoad = () => {
    loadTodos()
  }

  return (
    <div className="todo-list-container">
      <h1>Todo List</h1>
      
      {error && (
        <div className="error-message">
          <span>{error}</span>
          <button onClick={retryLoad} className="retry-button">
            Retry
          </button>
        </div>
      )}
      
      <div className="add-todo">
        <input
          type="text"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="Enter a new todo..."
          className="todo-input"
          disabled={loading}
        />
        <VoiceButton onTranscript={handleVoiceTranscript} disabled={loading} />
        <button
          onClick={addTodo}
          className="add-button"
          disabled={loading || inputValue.trim() === ''}
        >
          {loading ? 'Adding...' : 'Add'}
        </button>
      </div>

      {(() => {
        const uncheckedTodos = todos.filter(t => !t.checked)
        const checkedTodos = todos.filter(t => t.checked)

        const renderItem = (todo: Todo) => (
          <li key={todo.id} className={`todo-item ${todo.checked ? 'completed' : ''}`}>
            <input
              type="checkbox"
              checked={todo.checked}
              onChange={() => toggleTodo(todo.id)}
              className="todo-checkbox"
              disabled={loading}
            />
            <span className="todo-text">{todo.description}</span>
            <button
              onClick={() => removeTodo(todo.id)}
              className="remove-button"
              disabled={loading}
            >
              <TrashIcon size={18} color="white" />
            </button>
          </li>
        )

        return (
          <div className="todo-sections">
            <ul className="todo-list">
              {uncheckedTodos.map(renderItem)}
            </ul>

            {uncheckedTodos.length === 0 && !loading && !error && (
              <p className="empty-message">No items pending. Add one above!</p>
            )}

            {checkedTodos.length > 0 && (
              <>
                <div className="section-divider">
                  <span>Completed</span>
                </div>
                <ul className="todo-list checked-section">
                  {checkedTodos.map(renderItem)}
                </ul>
              </>
            )}
          </div>
        )
      })()}

      {loading && todos.length === 0 && (
        <div className="loading-message">
          <div className="loading-spinner"></div>
          <span>Loading todos...</span>
        </div>
      )}
    </div>
  )
})

TodoList.displayName = 'TodoList'
