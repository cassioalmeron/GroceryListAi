import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { useState } from 'react'
import './App.css'
import { Home } from './Pages/Home'
import { Tests } from './Pages/Tests'
import Settings from './Pages/Settings'
import Sidebar from './components/Sidebar'
import { MenuIcon } from './components/icons'

function App() {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)

  const toggleSidebar = () => {
    setIsSidebarOpen(!isSidebarOpen)
  }

  const closeSidebar = () => {
    setIsSidebarOpen(false)
  }

  return (
    <BrowserRouter>
      <div className="app">
        {/* Mobile Menu Button */}
        <button
          className="menu-toggle"
          onClick={toggleSidebar}
          aria-label="Toggle menu"
        >
          <MenuIcon />
        </button>

        {/* Sidebar Navigation */}
        <Sidebar isOpen={isSidebarOpen} onClose={closeSidebar} />

        {/* Main Content */}
        <div className="main-container">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/tests" element={<Tests />} />
            <Route path="/settings" element={<Settings />} />
          </Routes>
        </div>
      </div>
    </BrowserRouter>
  )
}

export default App
