import { useRef } from "react"
import { useNavigate } from "react-router-dom"
import TodoList from "../../components/TodoList"
import Chat from "../../components/Chat"
import "./styles.css"

export const Home = () => {
    const todoListRef = useRef<{ refreshTodos: () => void }>(null)
    const navigate = useNavigate()

    const handleChatResponseComplete = () => {
      todoListRef.current?.refreshTodos()
    }

    const handleSettingsClick = () => {
      navigate("/settings")
    }

    return (
    <>
        <button
          className="settings-icon-button"
          onClick={handleSettingsClick}
          title="Settings"
        >
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="12" cy="12" r="3"></circle>
            <path d="M12 1v6m0 6v6m-6-6h6m6 0h6"></path>
            <path d="M19.07 4.93l-4.24 4.24m0 5.66l4.24 4.24M4.93 4.93l4.24 4.24m0 5.66l-4.24 4.24"></path>
          </svg>
        </button>
        <div className="left-panel">
          <TodoList ref={todoListRef} />
        </div>
        <div className="right-panel">
          <Chat onResponseComplete={handleChatResponseComplete} />
        </div>
    </>
  )
}