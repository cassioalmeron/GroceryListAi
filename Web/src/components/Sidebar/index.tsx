import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import './style.css';

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ isOpen, onClose }) => {
  const location = useLocation();
  const navigate = useNavigate();

  const handleNavigation = (path: string) => {
    navigate(path);
    onClose(); // Close mobile menu after navigation
  };

  return (
    <>
      {/* Overlay for mobile menu */}
      <div
        className={`sidebar-overlay ${isOpen ? 'open' : ''}`}
        onClick={onClose}
      />

      {/* Sidebar */}
      <aside className={`sidebar ${isOpen ? 'open' : ''}`}>
        {/* Logo/Brand */}
        <div className="sidebar-header">
          <h2 className="sidebar-title">Grocery List AI</h2>
        </div>

        {/* Navigation Links */}
        <nav className="sidebar-nav">
          <ul className="sidebar-nav-list">
            <li>
              <button
                onClick={() => handleNavigation('/')}
                className={`sidebar-nav-link ${location.pathname === '/' ? 'active' : ''}`}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
                  <polyline points="9 22 9 12 15 12 15 22"></polyline>
                </svg>
                Home
              </button>
            </li>
            <li>
              <button
                onClick={() => handleNavigation('/settings')}
                className={`sidebar-nav-link ${location.pathname === '/settings' ? 'active' : ''}`}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <circle cx="12" cy="12" r="3"></circle>
                  <path d="M12 1v6m0 6v6m-6-6h6m6 0h6"></path>
                  <path d="M19.07 4.93l-4.24 4.24m0 5.66l4.24 4.24M4.93 4.93l4.24 4.24m0 5.66l-4.24 4.24"></path>
                </svg>
                Settings
              </button>
            </li>
          </ul>
        </nav>

        {/* Footer info */}
        <div className="sidebar-footer">
          <p>Version 1.0.0</p>
          <p className="sidebar-footer-date">Last updated: Dec 20, 2025 at 3:45 PM</p>
        </div>
      </aside>
    </>
  );
};

export default Sidebar;
