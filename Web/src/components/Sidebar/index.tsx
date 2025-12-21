import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { HomeIcon, SettingsIcon } from '../icons';
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
                <HomeIcon />
                Home
              </button>
            </li>
            <li>
              <button
                onClick={() => handleNavigation('/settings')}
                className={`sidebar-nav-link ${location.pathname === '/settings' ? 'active' : ''}`}
              >
                <SettingsIcon />
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
