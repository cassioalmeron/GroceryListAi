import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getLanguage, setLanguage as saveLanguage } from '../../utils/language';
import type { Language } from '../../utils/language';
import './styles.css';

const Settings = () => {
  const [language, setLanguage] = useState<Language>('en-US');
  const navigate = useNavigate();

  useEffect(() => {
    setLanguage(getLanguage());
  }, []);

  const handleLanguageChange = (newLanguage: Language) => {
    setLanguage(newLanguage);
    saveLanguage(newLanguage);
  };

  const handleBackClick = () => {
    navigate('/');
  };

  return (
    <div className="settings-container">
      <button
        className="back-icon-button"
        onClick={handleBackClick}
        title="Back to Home"
      >
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
          <polyline points="9 22 9 12 15 12 15 22"></polyline>
        </svg>
      </button>
      <div className="settings-content">
        <h1>Settings</h1>
        <div className="settings-section">
          <h2>Microphone</h2>
          <div className="setting-item">
            <label htmlFor="language-select">Language</label>
            <select
              id="language-select"
              value={language}
              onChange={(e) => handleLanguageChange(e.target.value as Language)}
              className="language-select"
            >
              <option value="en-US">English</option>
              <option value="pt-BR">Portuguese</option>
            </select>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Settings;
