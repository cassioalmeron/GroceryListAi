import TodoList from "../../components/TodoList"
import "./styles.css"

export const Home = () => {
    return (
      <div className="home-container">
        <div className="home-content">
          <TodoList />
        </div>
      </div>
  )
}
