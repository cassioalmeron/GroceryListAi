import { useRef } from "react"
import TodoList from "../../components/TodoList"
import Chat from "../../components/Chat"

export const Home = () => {
    const todoListRef = useRef<{ refreshTodos: () => void }>(null)

    const handleChatResponseComplete = () => {
      todoListRef.current?.refreshTodos()
    }
  
    return (
    <>
        <div className="left-panel">
          <TodoList ref={todoListRef} />
        </div>
        <div className="right-panel">
          <Chat onResponseComplete={handleChatResponseComplete} />
        </div>
    </>
  )
}