import React, { useState, useEffect, forwardRef, useImperativeHandle } from 'react';
import './style.css';
import { getGroceryList, addItem, deleteItem, markItemAsChecked } from '../../../api';

interface Todo {
  id: number
  description: string
  checked: boolean
}

export interface TodoListRef {
  refreshTodos: () => void
}

const TodoList = forwardRef<TodoListRef>((_props, ref) => {
  const [todos, setTodos] = useState<Todo[]>([])
  const [inputValue, setInputValue] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Load todos from API on component mount
  useEffect(() => {
    loadTodos()
  }, [])

  // Expose refreshTodos method to parent component
  useImperativeHandle(ref, () => ({
    refreshTodos: loadTodos
  }))

  const loadTodos = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await getGroceryList()
      // Map API response to our Todo interface
      const mappedTodos = data.map((item: { id: string; description: string; completed?: boolean }) => ({
        id: item.id,
        description: item.description,
        checked: item.checked || false
      }))
      setTodos(mappedTodos)
    } catch (err) {
      setError('Failed to load todos')
      console.error('Error loading todos:', err)
    } finally {
      setLoading(false)
    }
  }

  const addTodo = async () => {
    if (inputValue.trim() === '') return

    try {
      setLoading(true)
      setError(null)
      const description = inputValue.trim()
      const newItem = await addItem(description)
      // Add the new item to our local state
      const newTodo: Todo = {
        id: newItem.id,
        description: description,
        checked: false
      }
      setTodos([...todos, newTodo])
      setInputValue('')
    } catch (err) {
      setError('Failed to add todo')
      console.error('Error adding todo:', err)
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
    } catch (err) {
      setError('Failed to delete todo')
      console.error('Error deleting todo:', err)
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
    } catch (err) {
      setError('Failed to update todo status')
      console.error('Error updating todo:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      addTodo()
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
        <button 
          onClick={addTodo} 
          className="add-button"
          disabled={loading || inputValue.trim() === ''}
        >
          {loading ? 'Adding...' : 'Add'}
        </button>
      </div>

      <ul className="todo-list">
        {todos.map(todo => (
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
              {loading ? '...' : 'Remove'}
            </button>
          </li>
        ))}
      </ul>

      {todos.length === 0 && !loading && !error && (
        <p className="empty-message">No todos yet. Add one above!</p>
      )}

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

export default TodoList