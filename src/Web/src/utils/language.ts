const LANGUAGE_STORAGE_KEY = 'microphone_language';

export type Language = 'en-US' | 'pt-BR';

export const getLanguage = (): Language => {
  const savedLanguage = localStorage.getItem(LANGUAGE_STORAGE_KEY) as Language;
  return savedLanguage || 'en-US';
};

export const setLanguage = (language: Language): void => {
  localStorage.setItem(LANGUAGE_STORAGE_KEY, language);
};
