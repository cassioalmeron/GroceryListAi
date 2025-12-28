import { useState, useEffect } from 'react';
import { getLanguage, setLanguage as saveLanguage } from '../../utils/language';
import type { Language } from '../../utils/language';
import { useTheme, type Theme } from '../../contexts/ThemeContext';
import { toast } from 'react-toastify';
import './styles.css';

const Settings = () => {
  const [language, setLanguage] = useState<Language>('en-US');
  const { theme, setTheme } = useTheme();

  useEffect(() => {
    setLanguage(getLanguage());
  }, []);

  const handleLanguageChange = (newLanguage: Language) => {
    setLanguage(newLanguage);
    saveLanguage(newLanguage);
    const languageName = newLanguage === 'en-US' ? 'English' : 'Portuguese';
    toast.success(`Language changed to: ${languageName}`);
  };

  const handleThemeChange = (newTheme: Theme) => {
    setTheme(newTheme);
    const themeName = newTheme.charAt(0).toUpperCase() + newTheme.slice(1);
    toast.success(`Theme changed to: ${themeName}`);
  };

  const handleSaveSettings = () => {
    toast.success('Settings saved successfully!');
  };

  return (
    <div className="settings-container">
      <div className="settings-content-wrapper">
        <div className="settings-content">
          <h1 className="settings-title">Settings</h1>

          {/* Microphone Section */}
          <section className="settings-section">
            <h2 className="settings-section-title">Microphone</h2>
            <div className="setting-item">
              <label htmlFor="language-select" className="setting-label">Language</label>
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
          </section>

          {/* Notifications Section */}
          <section className="settings-section">
            <h2 className="settings-section-title">Notifications</h2>
            <div className="settings-toggle-group">
              <div className="settings-toggle-item">
                <div>
                  <label className="setting-label">Enable Notifications</label>
                  <p className="setting-description">Receive notifications for important updates</p>
                </div>
                <label className="toggle-switch">
                  <input type="checkbox" defaultChecked />
                  <span className="toggle-slider"></span>
                </label>
              </div>

              <div className="settings-toggle-item">
                <div>
                  <label className="setting-label">Sound Effects</label>
                  <p className="setting-description">Play sounds when adding or removing items</p>
                </div>
                <label className="toggle-switch">
                  <input type="checkbox" />
                  <span className="toggle-slider"></span>
                </label>
              </div>
            </div>
          </section>

          {/* Appearance Section */}
          <section className="settings-section">
            <h2 className="settings-section-title">Appearance</h2>
            <div className="settings-appearance-group">
              <div>
                <label className="setting-label">Theme</label>
                <div className="theme-buttons">
                  <button
                    className={`theme-button ${theme === 'dark' ? 'active' : ''}`}
                    onClick={() => handleThemeChange('dark')}
                  >
                    Dark
                  </button>
                  <button
                    className={`theme-button ${theme === 'light' ? 'active' : ''}`}
                    onClick={() => handleThemeChange('light')}
                  >
                    Light
                  </button>
                  <button
                    className={`theme-button ${theme === 'auto' ? 'active' : ''}`}
                    onClick={() => handleThemeChange('auto')}
                  >
                    Auto
                  </button>
                </div>
              </div>

              <div>
                <label htmlFor="font-size" className="setting-label">Font Size</label>
                <input
                  type="range"
                  id="font-size"
                  min="12"
                  max="20"
                  defaultValue="16"
                  className="font-size-slider"
                />
                <div className="font-size-labels">
                  <span>Small</span>
                  <span>Medium</span>
                  <span>Large</span>
                </div>
              </div>
            </div>
          </section>

          {/* Save Button */}
          <div className="settings-actions">
            <button onClick={handleSaveSettings} className="save-button">
              Save Changes
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Settings;
