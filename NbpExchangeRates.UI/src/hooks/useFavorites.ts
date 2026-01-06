import { useEffect, useState } from "react";

const STORAGE_KEY = "favoriteCurrencies";

export const useFavorites = () => {
  const [favorites, setFavorites] = useState<string[]>(() => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      return stored ? JSON.parse(stored) : [];
    } catch (error) {
      console.error("Failed to parse favorites:", error);
      return [];
    }
  });

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(favorites));
  }, [favorites]);

  const isFavorite = (code: string) => favorites.includes(code);

  const toggleFavorite = (code: string) => {
    setFavorites((prev) =>
      prev.includes(code)
        ? prev.filter((c) => c !== code)
        : [...prev, code]
    );
  };

  return {
    favorites,
    isFavorite,
    toggleFavorite,
  };
};