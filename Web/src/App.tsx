import { BrowserRouter, Route, Routes } from 'react-router-dom'
import './App.css'
import { Home } from './Pages/Home'
import { Tests } from './Pages/Tests'
import Settings from './Pages/Settings'
import Footer from './components/Footer'

function App() {
  return (
    <BrowserRouter>
      <div className="app">
        <div className="main-container">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/tests" element={<Tests />} />
            <Route path="/settings" element={<Settings />} />
          </Routes>
        </div>
        <Footer />
      </div>
    </BrowserRouter>
  )
}

export default App
