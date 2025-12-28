import { createContext, useContext, useState, useEffect, ReactNode } from 'react';

export type Theme = 'light' | 'dark' | 'auto';
export type ActualTheme = 'light' | 'dark';

interface ThemeContextType {
  theme: Theme;
  actualTheme: ActualTheme;
  setTheme: (theme: Theme) => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const useTheme = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within ThemeProvider');
  }
  return context;
};

interface ThemeProviderProps {
  children: ReactNode;
}

export const ThemeProvider = ({ children }: ThemeProviderProps) => {
  const [theme, setThemeState] = useState<Theme>(() => {
    const savedTheme = localStorage.getItem('theme') as Theme | null;
    return savedTheme || 'dark';
  });

  const [actualTheme, setActualTheme] = useState<ActualTheme>(() => {
    const savedActualTheme = localStorage.getItem('actualTheme') as ActualTheme | null;
    return savedActualTheme || 'dark';
  });

  const getSystemTheme = (): ActualTheme => {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  };

  const updateActualTheme = (selectedTheme: Theme) => {
    const newActualTheme = selectedTheme === 'auto' ? getSystemTheme() : selectedTheme;
    setActualTheme(newActualTheme);
    localStorage.setItem('actualTheme', newActualTheme);
    document.documentElement.setAttribute('data-theme', newActualTheme);
  };

  const setTheme = (newTheme: Theme) => {
    setThemeState(newTheme);
    localStorage.setItem('theme', newTheme);
    updateActualTheme(newTheme);
  };

  useEffect(() => {
    updateActualTheme(theme);

    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    const handleChange = (e: MediaQueryListEvent) => {
      if (theme === 'auto') {
        const newTheme = e.matches ? 'dark' : 'light';
        setActualTheme(newTheme);
        localStorage.setItem('actualTheme', newTheme);
        document.documentElement.setAttribute('data-theme', newTheme);
      }
    };

    mediaQuery.addEventListener('change', handleChange);

    return () => {
      mediaQuery.removeEventListener('change', handleChange);
    };
  }, [theme]);

  return (
    <ThemeContext.Provider value={{ theme, actualTheme, setTheme }}>
      {children}
    </ThemeContext.Provider>
  );
};
